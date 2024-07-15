using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Ekkam
{
    public class UIManager : MonoBehaviour
    {
        public GameObject gameUI;
        public GameObject playerActionsUI;
        
        public Button moveButton;
        public Button attackButton;
        
        public Button endTurnButton;
        
        public TMP_Text actionPointsText;
        public TMP_Text movementPointsText;
        public TMP_Text turnText;
        
        private Player player;
        
        public void AssignPlayerActions(Player player)
        {
            this.player = player;
            
            moveButton.onClick.AddListener(player.MoveButton);
            attackButton.onClick.AddListener(player.AttackButton);
            endTurnButton.onClick.AddListener(player.EndTurnButton);
            
            actionPointsText.text = "Action Points: " + player.actionPoints;
            movementPointsText.text = "Movement Points: " + player.movementPoints;
        }

        private void Update()
        {
            if (player != null)
            {
                actionPointsText.text = "Action Points: " + player.actionPoints;
                movementPointsText.text = "Movement Points: " + player.movementPoints;
                
                moveButton.interactable = player.movementPoints > 0;
                attackButton.interactable = player.actionPoints > 0;
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