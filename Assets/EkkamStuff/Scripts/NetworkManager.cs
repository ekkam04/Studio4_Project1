using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
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
        
        public bool spawnPlayerOnSceneLoad = true;

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
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
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
            if (scene.name == "MainGame")
            {
                Debug.Log("Spawning local player...");
                SpawnPlayer(AgentData, Vector3.zero);
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
                        
                        myPlayer.transform.position = spawnPositions[gameStartPacket.clientIndex];
                        var spawnGridPosition = myPlayer.grid.GetPositionFromWorldPoint(spawnPositions[gameStartPacket.clientIndex]);
                        myPlayer.UpdateStartPosition(spawnGridPosition);
                        SendTeleportAction(spawnGridPosition);
                        
                        var turnSystem = FindObjectOfType<TurnSystem>();
                        turnSystem.friendlyCount = gameStartPacket.clientCount;

                        if (gameStartPacket.clientIndex == 0)
                        {
                            var enemyManager = FindObjectOfType<EnemyManager>();
                            enemyManager.isMasterClient = true;
                        }
                        break;
                    
                    case BasePacket.Type.MoveAction:
                        GridPositionPacket moveActionPacket = new GridPositionPacket().Deserialize(buffer);
                        Debug.Log($"Received move action: {moveActionPacket.targetPosition} from {moveActionPacket.AgentData.name}");
                        
                        if (moveActionPacket.AgentData.name.Contains("Enemy"))
                        {
                            var enemyGO = GameObject.Find(moveActionPacket.AgentData.name);
                            if (enemyGO == null) return;
                            actionableAgent = enemyGO.GetComponent<Agent>();
                        }
                        else
                        {
                            SpawnOtherPlayerIfMissing(moveActionPacket.AgentData);
                            actionableAgent = players[moveActionPacket.AgentData.id].GetComponent<Agent>();
                        }
                        actionableAgent.MoveAction(moveActionPacket.targetPosition);
                        break;
                    
                    case BasePacket.Type.TeleportAction: // This only works for players for now
                        GridPositionPacket teleportActionPacket = new GridPositionPacket().Deserialize(buffer);
                        Debug.Log($"Received teleport action: {teleportActionPacket.targetPosition} from {teleportActionPacket.AgentData.name}");
                        
                        SpawnOtherPlayerIfMissing(teleportActionPacket.AgentData);
                        var teleportNodePosition = myPlayer.grid.GetNode(teleportActionPacket.targetPosition).transform.position;
                        var teleportPosition = new Vector3(
                            teleportNodePosition.x,
                            players[teleportActionPacket.AgentData.id].transform.position.y,
                            teleportNodePosition.z
                        );
                        players[teleportActionPacket.AgentData.id].transform.position = teleportPosition;
                        players[teleportActionPacket.AgentData.id].GetComponent<Agent>().UpdateStartPosition(teleportActionPacket.targetPosition);
                        break;
                    
                    case BasePacket.Type.AttackAction:
                        GridPositionPacket attackActionPacket = new GridPositionPacket().Deserialize(buffer);
                        Debug.Log($"Received attack action: {attackActionPacket.targetPosition} from {attackActionPacket.AgentData.name}");
                        
                        if (attackActionPacket.AgentData.name.Contains("Enemy"))
                        {
                            var enemyGO = GameObject.Find(attackActionPacket.AgentData.name);
                            if (enemyGO == null) return;
                            actionableAgent = enemyGO.GetComponent<Agent>();
                        }
                        else
                        {
                            SpawnOtherPlayerIfMissing(attackActionPacket.AgentData);
                            actionableAgent = players[attackActionPacket.AgentData.id].GetComponent<Agent>();
                        }
                        actionableAgent.AttackAction(attackActionPacket.targetPosition);
                        break;
                    
                    case BasePacket.Type.EndTurn:
                        EndTurnPacket endTurnPacket = new EndTurnPacket().Deserialize(buffer);
                        Debug.Log($"Received end turn from {endTurnPacket.AgentData.name}");
                        
                        if (endTurnPacket.AgentData.name.Contains("Enemy"))
                        {
                            var enemyGO = GameObject.Find(endTurnPacket.AgentData.name);
                            if (enemyGO == null) return;
                            actionableAgent = enemyGO.GetComponent<Agent>();
                        }
                        else
                        {
                            SpawnOtherPlayerIfMissing(endTurnPacket.AgentData);
                            actionableAgent = players[endTurnPacket.AgentData.id].GetComponent<Agent>();
                        }
                        actionableAgent.EndTurn();
                        break;
                }
            }
        }
        
        public void SendMoveAction(Vector2Int targetPosition, AgentData agentData = null)
        {
            if (agentData == null) agentData = AgentData;
            GridPositionPacket gridPositionPacket = new GridPositionPacket(BasePacket.Type.MoveAction, agentData, targetPosition);
            SendDataToServer(gridPositionPacket);
        }
        
        public void SendTeleportAction(Vector2Int targetPosition, AgentData agentData = null)
        {
            if (agentData == null) agentData = AgentData;
            GridPositionPacket gridPositionPacket = new GridPositionPacket(BasePacket.Type.TeleportAction, agentData, targetPosition);
            SendDataToServer(gridPositionPacket);
        }
        
        public void SendAttackAction(Vector2Int targetPosition, AgentData agentData = null)
        {
            if (agentData == null) agentData = AgentData;
            GridPositionPacket gridPositionPacket = new GridPositionPacket(BasePacket.Type.AttackAction, agentData, targetPosition);
            SendDataToServer(gridPositionPacket);
        }
        
        public void SendEndTurn(AgentData agentData = null)
        {
            if (agentData == null) agentData = AgentData;
            EndTurnPacket endTurnPacket = new EndTurnPacket(BasePacket.Type.EndTurn, agentData);
            SendDataToServer(endTurnPacket);
        }
        
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
            if (networkComponent.IsMine()) myPlayer = player;
            
            return player;
        }
    }
}