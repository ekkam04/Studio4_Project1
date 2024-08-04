﻿/*using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using ParrelSync;

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
            if (ClonesManager.IsClone())
            {
                Destroy(gameObject);
                return;
            }
            
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
            if (ClonesManager.IsClone()) return;
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
                    }
                }
            }
        }
        
        public void BroadcastGameStartPacket()
        {
            foreach (Socket client in clients)
            {
                GameStartPacket packet = new GameStartPacket(BasePacket.Type.GameStart, new AgentData(), clients.IndexOf(client), clients.Count);
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
*/