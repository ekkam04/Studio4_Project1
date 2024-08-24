using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Ekkam
{
    public class UIManager : MonoBehaviour
    {
        public GameObject gameUI;
        public GameObject playerActionsUI;
        public Button openInventoryButton;
        public Button closeInventoryButton;
        
        public Button moveButton;
        public Button attackButton;
        
        public Button endTurnButton;
        
        public Button pauseButton;
        public Button resumeButton;
        public Button mainMenuButton;
        public Button qualitySettingsButton;
        public GameObject pauseMenu;
        public GameObject qualitySettingsMenu;
        public Button highQualityButton;
        public Button mediumQualityButton;
        public Button lowQualityButton;
        
        public TMP_Text actionPointsText;
        public TMP_Text movementPointsText;
        public TMP_Text waitingText;
        public TMP_Text levelText;
        
        public Slider healthSlider;
        public Slider manaSlider;
        
        private Player player;
        
        private TurnSystem turnSystem;
        public GameObject endTurnButtonGO;
        public GameObject moveButtonGO;
        public GameObject shootButtonGO;
        
        private void Start()
        {
            turnSystem = FindObjectOfType<TurnSystem>();
        }
        
        public void AssignPlayerActions(Player player)
        {
            this.player = player;
            
            moveButton.onClick.AddListener(player.MoveButton);
            attackButton.onClick.AddListener(player.AttackButton);
            endTurnButton.onClick.AddListener(player.EndTurnButton);
            
            pauseButton.onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            mainMenuButton.onClick.AddListener(MainMenu);
            qualitySettingsButton.onClick.AddListener(QualitySettingsToggle);
            highQualityButton.onClick.AddListener(SetHighQuality);
            mediumQualityButton.onClick.AddListener(SetMediumQuality);
            lowQualityButton.onClick.AddListener(SetLowQuality);
            
            actionPointsText.text = "Action Points: " + player.actionPoints;
            movementPointsText.text = "Movement Points: " + player.movementPoints;
        }
        
        private void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex, true);
        }
        
        public void SetHighQuality()
        {
            SetQuality(2);
        }
        
        public void SetMediumQuality()
        {
            SetQuality(1);
        }
        
        public void SetLowQuality()
        {
            SetQuality(0);
        }
        
        public void Pause()
        {
            pauseMenu.SetActive(true);
        }
        
        public void Resume()
        {
            pauseMenu.SetActive(false);
        }
        
        public void QualitySettingsToggle()
        {
            qualitySettingsMenu.SetActive(!qualitySettingsMenu.activeSelf);
        }
        
        public void MainMenu()
        {
            // SceneManager.LoadScene("Lobby");
            Application.Quit();
        }

        private void Update()
        {
            if (player != null)
            {
                actionPointsText.text = "Action Points: " + player.actionPoints;
                movementPointsText.text = "Movement Points: " + player.movementPoints;
                levelText.text = player.manaPoints.ToString();
                
                moveButton.interactable = player.movementPoints > 0;
                attackButton.interactable = player.gunEquipped && player.actionPoints > 0;
                
                waitingText.gameObject.SetActive(player.isTakingAction);

                healthSlider.value = player.health;
                manaSlider.value = player.manaPoints;
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                if (openInventoryButton.gameObject.activeSelf)
                {
                    openInventoryButton.onClick.Invoke();
                }
                else
                {
                    closeInventoryButton.onClick.Invoke();
                
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (pauseMenu.activeSelf)
                {
                    pauseMenu.SetActive(false);
                }
                else
                {
                    pauseMenu.SetActive(true);
                }
            }

            if (turnSystem.hostileCount == 0)
            {
                endTurnButtonGO.SetActive(false);
                player.showPoints = false;
            }
            else
            {
                endTurnButtonGO.SetActive(true);
                player.showPoints = true;
            }
        }
        
        private bool IsMouseOverUI()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
        
        private bool IsMouseOverUIWithIgnores()
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;
            
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            for (int i = 0; i < raycastResults.Count; i++)
            {
                if (raycastResults[i].gameObject.CompareTag("IgnoreMouseUIClick"))
                {
                    raycastResults.RemoveAt(i);
                    i--;
                }
            }
            return raycastResults.Count > 0;
        }
    }
}