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
        public Button testAreaButton;
        public Button tutorialButton;
        public bool changeSceneOnConnect = true;

        private void Start()
        {
            testAreaButton.onClick.AddListener(() => Connect("MainGame"));
            tutorialButton.onClick.AddListener(() => Connect("Tutorial"));
        }
        
        void Connect(string sceneName)
        {
            NetworkManager.instance.AgentData = new AgentData(Guid.NewGuid().ToString(), nameInput.text);
            NetworkManager.instance.ConnectToServer("127.0.0.1", nameInput.text);
            print("Connected to server");
            OnConnectedToServer(sceneName);
        }

        private void OnConnectedToServer(string sceneName)
        {
            if (changeSceneOnConnect)
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}