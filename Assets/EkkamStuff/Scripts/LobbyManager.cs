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
        public Button arenaButton;
        public Button quitButton;
        public bool changeSceneOnConnect = true;

        private void Start()
        {
            testAreaButton.onClick.AddListener(() => Connect("Test"));
            tutorialButton.onClick.AddListener(() => Connect("Tutorial"));
            arenaButton.onClick.AddListener(() => Connect("Arena"));
            
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
        
        void Connect(string sceneName)
        {
            NetworkManager.instance.AgentData = new AgentData(Guid.NewGuid().ToString(), nameInput.text);
            NetworkManager.instance.ConnectToServer("127.0.0.1", nameInput.text);
            switch (sceneName)
            {
                case "Test":
                    NetworkManager.instance.spawnPositions = new Vector3[]
                    {
                        new Vector3(0.5f, 0, -1f),
                        new Vector3(0.5f, 0, 1f),
                        new Vector3(-1f, 0, 0.5f),
                        new Vector3(1f, 0, 0.5f),
                    };
                    break;
                case "Tutorial":
                    NetworkManager.instance.spawnPositions = new Vector3[]
                    {
                        new Vector3(0.5f,0f,-1.1f),
                        new Vector3(0.5f,0f,-1.1f),
                        new Vector3(0.5f,0f,-1.1f),
                        new Vector3(0.5f,0f,-1.1f),
                    };
                    break;
                case "Arena":
                    NetworkManager.instance.spawnPositions = new Vector3[]
                    {
                        new Vector3(-33.2f,-0.891906738f,11f),
                        new Vector3(27.6f,-0.891906738f,11f),
                        new Vector3(-33.2f,-0.891906738f,9f),
                        new Vector3(27.6f,-0.891906738f,9f),
                    };
                    break;
            }
            
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
        
        public void OnQuitButtonClicked()
        {
            Application.Quit();
        }
    }
}