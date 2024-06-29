using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace Ekkam
{
    public class LobbyManager : MonoBehaviour
    {
        public TMP_InputField nameInput;
        public Button connectButton;

        private void Start()
        {
            // Server.instance.connectedToServer += OnConnectedToServer;
            connectButton.onClick.AddListener(Connect);
        }
        
        void Connect()
        {
            Client.instance.playerData = new PlayerData(Guid.NewGuid().ToString(), nameInput.text);
            Client.instance.ConnectToServer("127.0.0.1", nameInput.text);
            OnConnectedToServer();
        }

        private void OnConnectedToServer()
        {
            SceneManager.LoadScene("MainGame");
        }
    }
}