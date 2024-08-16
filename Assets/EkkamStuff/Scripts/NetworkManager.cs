using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using Cinemachine;
using TMPro;
using QFSW.QC;
using UnityEngine.SceneManagement;

namespace Ekkam
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance;

        private Socket socket;
        
        public GameObject playerPrefab;
        public Player myPlayer;
        public AgentData AgentData;
        public Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
        
        public CinemachineVirtualCamera playerVCam;
        public CinemachineVirtualCamera actionVCam;
        
        public bool spawnPlayerOnSceneLoad = true;
        public bool localPlayerLoaded = false;
        private GameStartPacket gameStartPacket;

        public Vector3[] spawnPositions = new Vector3[]
        {
            new Vector3(0.5f, 0, -1f),
            new Vector3(0.5f, 0, 1f),
            new Vector3(-1f, 0, 0.5f)
        };

        void Start()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Player.onLocalPlayerLoaded += OnLocalPlayerLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Player.onLocalPlayerLoaded -= OnLocalPlayerLoaded;
        }
        
        void OnDestroy()
        {
            socket.Close();
        }

        void Update()
        {
            ReceiveDataFromServer();
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!spawnPlayerOnSceneLoad) return;
            if (scene.name != "Lobby")
            {
                Debug.Log("Spawning local player...");
                SpawnPlayer(AgentData, Vector3.zero);
            }
        }
        
        private void OnLocalPlayerLoaded()
        {
            localPlayerLoaded = true;
            if (!spawnPlayerOnSceneLoad) return;
            if (gameStartPacket == null) return;
            print("Setting local player spawn position and syncing for all clients...");

            myPlayer.transform.position = spawnPositions[gameStartPacket.clientIndex];
            var spawnGridPosition = myPlayer.grid.GetPositionFromWorldPoint(spawnPositions[gameStartPacket.clientIndex]);
            myPlayer.UpdateStartPosition(spawnGridPosition);
            SendTeleportAction(spawnGridPosition);
                        
            var turnSystem = FindObjectOfType<TurnSystem>();
            turnSystem.friendlyCount = gameStartPacket.clientCount;

            if (gameStartPacket.clientIndex == 0)
            {
                // var enemyManager = FindObjectOfType<EnemyManager>();
                // enemyManager.isMasterClient = true;
                turnSystem.isMasterClient = true;
            }
        }

        [Command("connect")]
        public void ConnectToServer(string ipAddress = "127.0.0.1", string playerName = "Player")
        {
            AgentData = new AgentData(Guid.NewGuid().ToString(), playerName);
            socket.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), 3000));
            socket.Blocking = false;
            Debug.Log("Connected to server.");
            
            if (!spawnPlayerOnSceneLoad)
            {
                Debug.Log("Spawning local player...");
                SpawnPlayer(AgentData, Vector3.zero);
            }
        }

        private void SendDataToServer(BasePacket packet)
        {
            byte[] data = packet.Serialize();
            socket.Send(data);
        }

        private void ReceiveDataFromServer()
        {
            if (socket.Available > 0)
            {
                byte[] buffer = new byte[socket.Available];
                socket.Receive(buffer);
                
                Agent actionableAgent;

                BasePacket packet = new BasePacket().BaseDeserialize(buffer);
                switch (packet.type)
                {
                    case BasePacket.Type.GameStart:
                        GameStartPacket gameStartPacket = new GameStartPacket().Deserialize(buffer);
                        Debug.Log($"Received game start packet - client index: {gameStartPacket.clientIndex} - client count: {gameStartPacket.clientCount}");
                        
                        this.gameStartPacket = gameStartPacket;
                        if (localPlayerLoaded) OnLocalPlayerLoaded(); // If local player is already loaded, call OnLocalPlayerLoaded again
                        
                        break;
                    
                    case BasePacket.Type.MoveAction:
                        GridPositionPacket moveActionPacket = new GridPositionPacket().Deserialize(buffer);
                        Debug.Log($"Received move action: {moveActionPacket.targetPosition} from {moveActionPacket.AgentData.name}");
                        
                        actionableAgent = GetActionableAgent(moveActionPacket.AgentData);
                        if (actionableAgent == null) return;
                        
                        actionableAgent.MoveAction(moveActionPacket.targetPosition);
                        break;
                    
                    case BasePacket.Type.TeleportAction:
                        GridPositionPacket teleportActionPacket = new GridPositionPacket().Deserialize(buffer);
                        Debug.Log($"Received teleport action: {teleportActionPacket.targetPosition} from {teleportActionPacket.AgentData.name}");
                        
                        actionableAgent = GetActionableAgent(teleportActionPacket.AgentData);
                        if (actionableAgent == null) return;
                        
                        var teleportNodePosition = actionableAgent.grid.GetNode(teleportActionPacket.targetPosition).transform.position;
                        var teleportPosition = new Vector3(
                            teleportNodePosition.x,
                            actionableAgent.transform.position.y,
                            teleportNodePosition.z
                        );
                        actionableAgent.transform.position = teleportPosition;
                        actionableAgent.UpdateStartPosition(teleportActionPacket.targetPosition);
                        break;
                    
                    case BasePacket.Type.AttackAction:
                        AttackActionPacket attackActionPacket = new AttackActionPacket().Deserialize(buffer);
                        Debug.Log($"Received attack action: {attackActionPacket.targetPosition} from {attackActionPacket.AgentData.name}");
                        
                        actionableAgent = GetActionableAgent(attackActionPacket.AgentData);
                        if (actionableAgent == null) return;
                        
                        actionableAgent.AttackAction(attackActionPacket.targetPosition, attackActionPacket.damage);
                        break;
                    
                    case BasePacket.Type.StartTurn:
                        StartTurnPacket startTurnPacket = new StartTurnPacket().Deserialize(buffer);
                        Debug.Log($"Received start turn from {startTurnPacket.AgentData.name}");
                        
                        actionableAgent = players[startTurnPacket.agentID].GetComponent<Agent>();
                        if (actionableAgent == null) return;
                        
                        actionableAgent.StartTurn();
                        break;
                    
                    case BasePacket.Type.EndTurn:
                        EndTurnPacket endTurnPacket = new EndTurnPacket().Deserialize(buffer);
                        Debug.Log($"Received end turn from {endTurnPacket.AgentData.name}");
                        
                        actionableAgent = GetActionableAgent(endTurnPacket.AgentData);
                        if (actionableAgent == null) return;
                        
                        actionableAgent.EndTurn();
                        break;
                    
                    case BasePacket.Type.ItemPickup:
                        ItemPacket itemPacket = new ItemPacket().Deserialize(buffer);
                        Debug.Log($"Received item pickup from {itemPacket.AgentData.name}: {itemPacket.itemKey}");
                        
                        // We don't need to get the actionable agent for item pickup yet. Maybe later on for item pickup animations
                        // actionableAgent = GetActionableAgent(itemPacket.AgentData);
                        // if (actionableAgent == null) return;
                        
                        var allItems = FindObjectsOfType<Item>();
                        foreach (var item in allItems)
                        {
                            if (item.item.itemKey == itemPacket.itemKey)
                            {
                                Destroy(item.gameObject);
                                break;
                            }
                        }
                        
                        break;
                }
            }
        }
        
        private Agent GetActionableAgent(AgentData agentData)
        {
            if (agentData.name.Contains("Enemy"))
            {
                var enemyGO = GameObject.Find(agentData.name);
                if (enemyGO == null) return null;
                return enemyGO.GetComponent<Agent>();
            }
            else
            {
                SpawnOtherPlayerIfMissing(agentData);
                return players[agentData.id].GetComponent<Agent>();
            }
        }
        
        // -------------- Packet sending methods ----------------------------------------------------------------------------------------------------
        public void SendMoveAction(Vector2Int targetPosition, AgentData agentData = null)
        {
            if (agentData == null) agentData = AgentData; // If agentData is null, use the local player's agent data
            GridPositionPacket gridPositionPacket = new GridPositionPacket(BasePacket.Type.MoveAction, agentData, targetPosition);
            SendDataToServer(gridPositionPacket);
        }
        
        public void SendTeleportAction(Vector2Int targetPosition, AgentData agentData = null)
        {
            if (agentData == null) agentData = AgentData;
            GridPositionPacket gridPositionPacket = new GridPositionPacket(BasePacket.Type.TeleportAction, agentData, targetPosition);
            SendDataToServer(gridPositionPacket);
        }
        
        public void SendAttackAction(Vector2Int targetPosition, float damage, AgentData agentData = null)
        {
            if (agentData == null) agentData = AgentData;
            AttackActionPacket gridPositionPacket = new AttackActionPacket(BasePacket.Type.AttackAction, agentData, targetPosition, damage);
            SendDataToServer(gridPositionPacket);
        }
        
        public void SendStartTurn(string agentID, AgentData agentData = null)
        {
            if (agentData == null) agentData = AgentData;
            StartTurnPacket startTurnPacket = new StartTurnPacket(BasePacket.Type.StartTurn, agentData, agentID);
            SendDataToServer(startTurnPacket);
        }
        
        public void SendEndTurn(AgentData agentData = null)
        {
            if (agentData == null) agentData = AgentData;
            EndTurnPacket endTurnPacket = new EndTurnPacket(BasePacket.Type.EndTurn, agentData);
            SendDataToServer(endTurnPacket);
        }

        public void SendItemPacket(string itemKey, AgentData agentData = null)
        {
            if (agentData == null) agentData = AgentData;
            ItemPacket itemPacket = new ItemPacket(BasePacket.Type.ItemPickup, agentData, itemKey);
            SendDataToServer(itemPacket);
        }
        // ------------------------------------------------------------------------------------------------------------------------------------------
        
        private void SpawnOtherPlayerIfMissing(AgentData agentData)
        {
            if (!players.ContainsKey(agentData.id))
            {
                var player = SpawnPlayer(agentData, Vector3.zero);
                player.grid = myPlayer.grid; // There is anyways only one grid
            }
        }
        
        public Player SpawnPlayer(AgentData agentData, Vector3 position)
        {
            GameObject playerObject = Instantiate(playerPrefab, position, Quaternion.identity);
            players.Add(agentData.id, playerObject);
            NetworkComponent networkComponent = playerObject.GetComponent<NetworkComponent>();
            networkComponent.ownerID = agentData.id;
            networkComponent.ownerName = agentData.name;
            
            var player = playerObject.GetComponent<Player>();
            player.enabled = true;
            if (networkComponent.IsMine())
            {
                myPlayer = player;
                
                playerVCam = GameObject.Find("PlayerVCam").GetComponent<CinemachineVirtualCamera>();
                playerVCam.Follow = player.transform;
                playerVCam.LookAt = player.transform;
                playerVCam.gameObject.SetActive(true);
                player.playerCamera = playerVCam;
                
                actionVCam = GameObject.Find("ActionVCam").GetComponent<CinemachineVirtualCamera>();
                actionVCam.gameObject.SetActive(false);
            }
            
            return player;
        }
        
        public void ShowActionCam(Agent agent)
        {
            actionVCam.Follow = agent.transform;
            actionVCam.LookAt = agent.transform;
            actionVCam.gameObject.SetActive(true);
        }
        
        public void HideActionCam()
        {
            actionVCam.gameObject.SetActive(false);
        }
    }
}