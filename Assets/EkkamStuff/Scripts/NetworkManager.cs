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
            if (scene.name == "MainGame")
            {
                Debug.Log("Spawning local player...");
                SpawnPlayer(playerData.id, Vector3.zero);
            }
        }

        [Command("connect")]
        public void ConnectToServer(string ipAddress = "127.0.0.1", string playerName = "Player")
        {
            playerData = new PlayerData(Guid.NewGuid().ToString(), playerName);
            socket.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), 3000));
            socket.Blocking = false;
            Debug.Log("Connected to server.");
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
                        Debug.Log($"Received game start packet - client index: {gameStartPacket.clientIndex}");
                        
                        myPlayer.transform.position = spawnPositions[gameStartPacket.clientIndex];
                        var spawnGridPosition = myPlayer.grid.GetPositionFromWorldPoint(spawnPositions[gameStartPacket.clientIndex]);
                        myPlayer.UpdateStartPosition(spawnGridPosition);
                        SendTeleportAction(spawnGridPosition);
                        break;
                    case BasePacket.Type.MoveAction:
                        MoveActionPacket moveActionPacket = new MoveActionPacket().Deserialize(buffer);
                        Debug.Log($"Received move action: {moveActionPacket.targetPosition} from {moveActionPacket.playerData.name}");
                        
                        if (!players.ContainsKey(moveActionPacket.playerData.id))
                        {
                            var player = SpawnPlayer(moveActionPacket.playerData.id, Vector3.zero);
                            player.grid = myPlayer.grid; // There is anyways only one grid
                        }
                        players[moveActionPacket.playerData.id].GetComponent<Agent>().MoveAction(moveActionPacket.targetPosition);
                        break;
                    case BasePacket.Type.TeleportAction:
                        MoveActionPacket teleportActionPacket = new MoveActionPacket().Deserialize(buffer);
                        Debug.Log($"Received teleport action: {teleportActionPacket.targetPosition} from {teleportActionPacket.playerData.name}");
                        
                        if (!players.ContainsKey(teleportActionPacket.playerData.id))
                        {
                            var player = SpawnPlayer(teleportActionPacket.playerData.id, Vector3.zero);
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
                }
            }
        }

        // public void SendPosition(Vector3 position)
        // {
        //     PositionPacket positionPacket = new PositionPacket(BasePacket.Type.Position, playerData, position);
        //     SendDataToServer(positionPacket);
        // }
        
        // public void SendRotationY(float rotationY)
        // {
        //     RotationYPacket rotationYPacket = new RotationYPacket(BasePacket.Type.Rotation, playerData, rotationY);
        //     SendDataToServer(rotationYPacket);
        // }
        
        // public void SendAnimationState(AnimationStatePacket.AnimationCommandType commandType, string parameterName, bool boolValue, float floatValue)
        // {
        //     AnimationStatePacket animationStatePacket = new AnimationStatePacket(BasePacket.Type.AnimationState, playerData, commandType, parameterName, boolValue, floatValue);
        //     SendDataToServer(animationStatePacket);
        // }
        
        public void SendMoveAction(Vector2Int targetPosition)
        {
            MoveActionPacket moveActionPacket = new MoveActionPacket(BasePacket.Type.MoveAction, playerData, targetPosition);
            SendDataToServer(moveActionPacket);
        }
        
        public void SendTeleportAction(Vector2Int targetPosition)
        {
            MoveActionPacket moveActionPacket = new MoveActionPacket(BasePacket.Type.TeleportAction, playerData, targetPosition);
            SendDataToServer(moveActionPacket);
        }
        
        private Player SpawnPlayer(string playerId, Vector3 position)
        {
            GameObject playerObject = Instantiate(playerPrefab, position, Quaternion.identity);
            players.Add(playerId, playerObject);
            NetworkComponent networkComponent = playerObject.GetComponent<NetworkComponent>();
            networkComponent.ownerID = playerId;
            networkComponent.ownerName = playerData.name;
            
            var player = playerObject.GetComponent<Player>();
            player.enabled = true;
            if (networkComponent.IsMine()) myPlayer = player;
            
            return player;
        }
    }
}