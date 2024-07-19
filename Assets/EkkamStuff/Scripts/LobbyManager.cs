using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace Ekkam
{
    public class LobbyManager : MonoBehaviour
    {
        public TMP_InputField ipInput;
        public TMP_InputField nameInput;
        public Button connectButton;
        public bool changeSceneOnConnect = true;

        private void Start()
        {
            connectButton.onClick.AddListener(Connect);
        }
        
        void Connect()
        {
            NetworkManager.instance.AgentData = new AgentData(Guid.NewGuid().ToString(), nameInput.text);
            NetworkManager.instance.ConnectToServer("127.0.0.1", nameInput.text);
            OnConnectedToServer();
        }

        private void OnConnectedToServer()
        {
            if (changeSceneOnConnect)
            {
                SceneManager.LoadScene("MainGame");
            }
            else
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}