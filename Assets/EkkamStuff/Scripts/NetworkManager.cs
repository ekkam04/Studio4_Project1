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
        public PlayerData playerData;
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
                SpawnPlayer(playerData, Vector3.zero);
            }
        }

        [Command("connect")]
        public void ConnectToServer(string ipAddress = "127.0.0.1", string playerName = "Player")
        {
            playerData = new PlayerData(Guid.NewGuid().ToString(), playerName);
            socket.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), 3000));
            socket.Blocking = false;
            Debug.Log("Connected to server.");
            
            if (!spawnPlayerOnSceneLoad)
            {
                Debug.Log("Spawning local player...");
                SpawnPlayer(playerData, Vector3.zero);
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
                        // turnSystem.enabled = true;
                        turnSystem.friendlyCount = gameStartPacket.clientCount;
                        break;
                    case BasePacket.Type.MoveAction:
                        GridPositionPacket moveActionPacket = new GridPositionPacket().Deserialize(buffer);
                        Debug.Log($"Received move action: {moveActionPacket.targetPosition} from {moveActionPacket.playerData.name}");
                        
                        if (!players.ContainsKey(moveActionPacket.playerData.id))
                        {
                            var player = SpawnPlayer(moveActionPacket.playerData, Vector3.zero);
                            player.grid = myPlayer.grid; // There is anyways only one grid
                        }
                        players[moveActionPacket.playerData.id].GetComponent<Agent>().MoveAction(moveActionPacket.targetPosition);
                        break;
                    case BasePacket.Type.TeleportAction:
                        GridPositionPacket teleportActionPacket = new GridPositionPacket().Deserialize(buffer);
                        Debug.Log($"Received teleport action: {teleportActionPacket.targetPosition} from {teleportActionPacket.playerData.name}");
                        
                        if (!players.ContainsKey(teleportActionPacket.playerData.id))
                        {
                            var player = SpawnPlayer(teleportActionPacket.playerData, Vector3.zero);
                            player.grid = myPlayer.grid;
                        }
                        var teleportNodePosition = myPlayer.grid.GetNode(teleportActionPacket.targetPosition).transform.position;
                        var teleportPosition = new Vector3(
                            teleportNodePosition.x,
                            players[teleportActionPacket.playerData.id].transform.position.y,
                            teleportNodePosition.z
                        );
                        players[teleportActionPacket.playerData.id].transform.position = teleportPosition;
                        players[teleportActionPacket.playerData.id].GetComponent<Agent>().UpdateStartPosition(teleportActionPacket.targetPosition);
                        break;
                    case BasePacket.Type.AttackAction:
                        GridPositionPacket attackActionPacket = new GridPositionPacket().Deserialize(buffer);
                        Debug.Log($"Received attack action: {attackActionPacket.targetPosition} from {attackActionPacket.playerData.name}");
                        
                        if (!players.ContainsKey(attackActionPacket.playerData.id))
                        {
                            var player = SpawnPlayer(attackActionPacket.playerData, Vector3.zero);
                            player.grid = myPlayer.grid;
                        }
                        players[attackActionPacket.playerData.id].GetComponent<Agent>().AttackAction(attackActionPacket.targetPosition);
                        break;
                    case BasePacket.Type.EndTurn:
                        EndTurnPacket endTurnPacket = new EndTurnPacket().Deserialize(buffer);
                        Debug.Log($"Received end turn from {endTurnPacket.playerData.name}");
                        
                        if (!players.ContainsKey(endTurnPacket.playerData.id))
                        {
                            var player = SpawnPlayer(endTurnPacket.playerData, Vector3.zero);
                            player.grid = myPlayer.grid;
                        }
                        players[endTurnPacket.playerData.id].GetComponent<Agent>().EndTurn();
                        break;
                }
            }
        }
        
        public void SendMoveAction(Vector2Int targetPosition)
        {
            GridPositionPacket gridPositionPacket = new GridPositionPacket(BasePacket.Type.MoveAction, playerData, targetPosition);
            SendDataToServer(gridPositionPacket);
        }
        
        public void SendTeleportAction(Vector2Int targetPosition)
        {
            GridPositionPacket gridPositionPacket = new GridPositionPacket(BasePacket.Type.TeleportAction, playerData, targetPosition);
            SendDataToServer(gridPositionPacket);
        }
        
        public void SendAttackAction(Vector2Int targetPosition)
        {
            GridPositionPacket gridPositionPacket = new GridPositionPacket(BasePacket.Type.AttackAction, playerData, targetPosition);
            SendDataToServer(gridPositionPacket);
        }
        
        public void SendEndTurn()
        {
            EndTurnPacket endTurnPacket = new EndTurnPacket(BasePacket.Type.EndTurn, playerData);
            SendDataToServer(endTurnPacket);
        }
        
        public Player SpawnPlayer(PlayerData playerData, Vector3 position)
        {
            GameObject playerObject = Instantiate(playerPrefab, position, Quaternion.identity);
            players.Add(playerData.id, playerObject);
            NetworkComponent networkComponent = playerObject.GetComponent<NetworkComponent>();
            networkComponent.ownerID = playerData.id;
            networkComponent.ownerName = playerData.name;
            
            var player = playerObject.GetComponent<Player>();
            player.enabled = true;
            if (networkComponent.IsMine()) myPlayer = player;
            
            return player;
        }
    }
}