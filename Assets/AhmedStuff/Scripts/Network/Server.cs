using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;

namespace AhmedNetwork
{
    public class Server : MonoBehaviour
    {
        private Socket serverSocket;
        private List<Socket> clients = new List<Socket>();
        
        public static Server instance;

        private void Start()
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
            
            serverSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 3000);
            serverSocket.Bind(ipEndPoint);
            serverSocket.Listen(8);
            serverSocket.Blocking = false;
            Debug.Log("waiting for connection");
        }
    }
}

