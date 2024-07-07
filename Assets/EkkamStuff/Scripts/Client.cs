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
    public class Client : MonoBehaviour
    {
        public static Client instance;

        private Socket socket;
        
        public GameObject playerPrefab;

        public delegate void OnPositionReceived(Vector3 position);
        public static event OnPositionReceived onPositionReceived;

        public PlayerData playerData;
        public Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

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
                    case BasePacket.Type.Position:
                        PositionPacket positionPacket = new PositionPacket().Deserialize(buffer);
                        Debug.Log($"Received position: {positionPacket.position} from {positionPacket.playerData.name}");
                        // onPositionReceived?.Invoke(positionPacket.position);
                        
                        if (players.ContainsKey(positionPacket.playerData.id))
                        {
                            // players[positionPacket.playerData.id].GetComponent<Player>().lastSentPosition = positionPacket.position;
                        }
                        else
                        {
                            SpawnPlayer(positionPacket.playerData.id, positionPacket.position);
                        }
                        break;
                    
                    case BasePacket.Type.Rotation:
                        RotationYPacket rotationYPacket = new RotationYPacket().Deserialize(buffer);
                        Debug.Log($"Received rotation: {rotationYPacket.rotationY} from {rotationYPacket.playerData.name}");
                        
                        if (players.ContainsKey(rotationYPacket.playerData.id))
                        {
                            // players[rotationYPacket.playerData.id].GetComponent<Player>().lastSentRotationY = rotationYPacket.rotationY;
                        }
                        else
                        {
                            SpawnPlayer(rotationYPacket.playerData.id, Vector3.zero);
                        }
                        break;
                    case BasePacket.Type.AnimationState:
                        AnimationStatePacket animationStatePacket = new AnimationStatePacket().Deserialize(buffer);
                        Debug.Log($"Received animation state: {animationStatePacket.commandType} from {animationStatePacket.playerData.name}");
                        
                        if (players.ContainsKey(animationStatePacket.playerData.id))
                        {
                            Player player = players[animationStatePacket.playerData.id].GetComponent<Player>();
                            switch (animationStatePacket.commandType)
                            {
                                case AnimationStatePacket.AnimationCommandType.Bool:
                                    // player.anim.SetBool(animationStatePacket.parameterName, animationStatePacket.boolValue);
                                    break;
                                case AnimationStatePacket.AnimationCommandType.Trigger:
                                    // player.anim.SetTrigger(animationStatePacket.parameterName);
                                    break;
                                case AnimationStatePacket.AnimationCommandType.Float:
                                    // player.anim.SetFloat(animationStatePacket.parameterName, animationStatePacket.floatValue);
                                    break;
                            }
                        }
                        else
                        {
                            SpawnPlayer(animationStatePacket.playerData.id, Vector3.zero);
                        }
                        break;
                }
            }
        }

        public void SendPosition(Vector3 position)
        {
            PositionPacket positionPacket = new PositionPacket(BasePacket.Type.Position, playerData, position);
            SendDataToServer(positionPacket);
        }
        
        public void SendRotationY(float rotationY)
        {
            RotationYPacket rotationYPacket = new RotationYPacket(BasePacket.Type.Rotation, playerData, rotationY);
            SendDataToServer(rotationYPacket);
        }
        
        public void SendAnimationState(AnimationStatePacket.AnimationCommandType commandType, string parameterName, bool boolValue, float floatValue)
        {
            AnimationStatePacket animationStatePacket = new AnimationStatePacket(BasePacket.Type.AnimationState, playerData, commandType, parameterName, boolValue, floatValue);
            SendDataToServer(animationStatePacket);
        }
        
        private void SpawnPlayer(string playerId, Vector3 position)
        {
            GameObject playerObject = Instantiate(playerPrefab, position, Quaternion.identity);
            NetworkComponent networkComponent = playerObject.GetComponent<NetworkComponent>();
            networkComponent.ownerID = playerId;
            networkComponent.name = playerData.name;
            if (networkComponent.IsMine())
            {
                CinemachineVirtualCamera playerVCam = GameObject.Find("PlayerVCam").GetComponent<CinemachineVirtualCamera>();
                // playerVCam.Follow = playerObject.GetComponent<Player>().cameraPos;
            }
            players.Add(playerId, playerObject);
        }
    }
}