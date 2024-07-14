using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using QFSW.QC;
using TMPro;

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
            socket.Bind(new IPEndPoint(IPAddress.Any, 3000));
            socket.Listen(10);
            socket.Blocking = false;
            Debug.Log("Server started, waiting for connections...");
        }

        private void OnDestroy()
        {
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
                if (clients.Count == maxClients)
                {
                    acceptingNewClients = false;
                }
                
                Socket newClient = socket.Accept();
                clients.Add(newClient);
                Debug.Log("New client connected.");
                connectedToServer?.Invoke();
                
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
                            MoveActionPacket moveActionPacket = new MoveActionPacket().Deserialize(buffer);
                            Debug.Log($"Received move action from {moveActionPacket.playerData.name}: {moveActionPacket.targetPosition}");
                            break;
                    }
                }
            }
        }
        
        public void BroadcastGameStartPacket()
        {
            foreach (Socket client in clients)
            {
                GameStartPacket packet = new GameStartPacket(BasePacket.Type.GameStart, new PlayerData(), clients.IndexOf(client));
                client.Send(packet.Serialize());
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
