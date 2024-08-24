using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using ParrelSync;
#endif

namespace Ekkam
{

    public class Server : MonoBehaviour
    {
        private Socket socket;
        public List<Socket> clients = new List<Socket>();

        public delegate void ConnectedToServer();

        public event ConnectedToServer connectedToServer;

        public static Server instance;
        
        public bool acceptingNewClients = true;
        public int maxClients = 3;

        void Start()
        {
            #if UNITY_EDITOR
            if (ClonesManager.IsClone())
            {
                Destroy(gameObject);
                return;
            }
            #endif
            
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
            socket.Bind(new IPEndPoint(IPAddress.Any, 3000));
            socket.Listen(10);
            socket.Blocking = false;
            Debug.Log("Server started, waiting for connections...");
        }
        
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!acceptingNewClients && scene.name == "Lobby")
            {
                // DisconnectAllClients();
            }
        }
        
        public async void DisconnectAllClients()
        {
            NetworkManager.instance.socket.Close();
            await Task.Delay(100);
            foreach (Socket client in clients)
            {
                client.Close();
                await Task.Delay(100);
            }
            socket.Close();
            clients.Clear();
            
            acceptingNewClients = true;
            Debug.Log("Server is accepting new clients again.");
            
            await Task.Delay(1000);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 3000));
            socket.Listen(10);
            socket.Blocking = false;
            Debug.Log("Server started, waiting for connections...");
        }

        private void OnDestroy()
        {
            #if UNITY_EDITOR
            if (ClonesManager.IsClone()) return;
            #endif
            socket.Close();
        }

        void Update()
        {
            if (acceptingNewClients) AcceptNewClients();
            ReceiveDataFromClients();
            if (Input.GetKeyDown(KeyCode.P)) BroadcastGameStartPacket();
        }

        private void AcceptNewClients()
        {
            try
            {
                Socket newClient = socket.Accept();
                clients.Add(newClient);
                Debug.Log("New client connected.");
                connectedToServer?.Invoke();
                
                if (clients.Count == maxClients)
                {
                    acceptingNewClients = false;
                    BroadcastGameStartPacket();
                }
                
            }
            catch
            {
                // No pending connections
            }
        }

        private void ReceiveDataFromClients()
        {
            foreach (Socket client in clients)
            {
                if (client.Available > 0)
                {
                    byte[] buffer = new byte[client.Available];
                    client.Receive(buffer);
                    
                    BasePacket packet = new BasePacket().BaseDeserialize(buffer);
                    BroadcastData(buffer, client);
                    
                    switch (packet.type)
                    {
                        case BasePacket.Type.MoveAction:
                            GridPositionPacket moveActionPacket = new GridPositionPacket().Deserialize(buffer);
                            Debug.Log($"Server received move action from {moveActionPacket.AgentData.name}: {moveActionPacket.targetPosition}");
                            break;
                        case BasePacket.Type.AttackAction:
                            AttackActionPacket attackActionPacket = new AttackActionPacket().Deserialize(buffer);
                            Debug.Log($"Server received attack action from {attackActionPacket.AgentData.name}: {attackActionPacket.targetPosition} with {attackActionPacket.damage} damage");
                            break;
                        case BasePacket.Type.ItemPickup:
                            ItemPacket itemPacket = new ItemPacket().Deserialize(buffer);
                            Debug.Log($"Server received item pickup from {itemPacket.AgentData.name}: {itemPacket.itemKey}");
                            break;
                    }
                }
            }
        }
        
        public async void BroadcastGameStartPacket()
        {
            await Task.Delay(1000);
            foreach (Socket client in clients)
            {
                GameStartPacket packet = new GameStartPacket(BasePacket.Type.GameStart, new AgentData(), clients.IndexOf(client), clients.Count);
                client.Send(packet.Serialize());
                await Task.Delay(200);
            }
        }

        private void BroadcastData(byte[] data, Socket exceptClient)
        {
            foreach (Socket client in clients)
            {
                if (client != exceptClient)
                {
                    client.Send(data);
                }
            }
        }
    }
}