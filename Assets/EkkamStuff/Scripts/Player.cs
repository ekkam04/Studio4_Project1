using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{
    public class Player : Agent
    {
        [Header("--- Player Settings ---")]
        
        public GameObject mousePosition3DPrefab;
        
        private UIManager uiManager;
        private MousePosition3D mousePosition3D;
        public List<PathfindingNode> reachableNodes = new List<PathfindingNode>();
        
        public PathfindingNode lastSelectedNode;
        public bool selectingTarget;
        
        private NetworkComponent networkComponent;

        private new void Start()
        {
            base.Start();
            networkComponent = GetComponent<NetworkComponent>();
            if (!networkComponent.IsMine()) return;
            
            networkComponent = GetComponent<NetworkComponent>();
            mousePosition3D = Instantiate(mousePosition3DPrefab).GetComponent<MousePosition3D>();
            
            uiManager = GameObject.FindObjectOfType<UIManager>();
            uiManager.AssignPlayerActions(this);
        }

        private new void Update()
        {
            base.Update();
            if (!networkComponent.IsMine()) return;
            
            if (isTakingAction) return;
            
            Vector2Int mousePositionOnGrid = grid.GetPositionFromWorldPoint(mousePosition3D.transform.position);
            
            if (
                Input.GetMouseButtonDown(0)
                && grid.GetNode(mousePositionOnGrid) != null
            )
            {
                var selectedNode = grid.GetNode(mousePositionOnGrid);

                if (!selectingTarget)
                {
                    UnselectAction();
                    
                    print("Occupant: " + selectedNode.occupantText.text);
                    if (selectedNode.Occupant == null) return;

                    if (selectedNode.Occupant == this.gameObject)
                    {
                        print("Selected self");
                        selectedNode.SetActionable(false, PathfindingNode.VisualType.Selected);
                        lastSelectedNode = selectedNode;
                        uiManager.playerActionsUI.SetActive(true);
                    }
                }
                else
                {
                    if (selectedNode.Occupant == this.gameObject)
                    {
                        print("Selected self while selecting target");
                        selectedNode.SetActionable(false);
                        UnselectAction();
                        return;
                    }
                    
                    if (!selectedNode.pathVisual.activeSelf)
                    {
                        print("Target is unreachable");
                        return;
                    }

                    // Move action
                    MoveAction(mousePositionOnGrid);
                    NetworkManager.instance.SendMoveAction(mousePositionOnGrid);
                }
            }
        }
        
        private void UnselectAction()
        {
            lastSelectedNode?.SetActionable(false);
            uiManager.playerActionsUI.SetActive(false);
            foreach (var node in reachableNodes)
            {
                node.SetActionable(false);
            }
            selectingTarget = false;
        }

        public override void StartTurn()
        {
            base.StartTurn();
            if (!networkComponent.IsMine()) return;
            
            uiManager.gameUI.SetActive(true);
        }
        
        public override void OnActionStart()
        {
            base.OnActionStart();
            if (!networkComponent.IsMine()) return;
            
            uiManager.endTurnButton.interactable = false;
            
            UnselectAction();
            uiManager.playerActionsUI.SetActive(false);
            lastSelectedNode?.SetActionable(false);
        }
        
        public override void OnActionEnd()
        {
            base.OnActionEnd();
            if (!networkComponent.IsMine()) return;
            
            uiManager.endTurnButton.interactable = true;
        }
        
        public void EndTurnButton()
        {
            EndTurn();
            NetworkManager.instance.SendEndTurn();
            
            UnselectAction();
            uiManager.gameUI.SetActive(false);
        }

        public void MoveButton()
        {
            if (movementPoints <= 0) Debug.LogWarning("Not enough movement points");
            
            reachableNodes = GetReachableNodes(movementPoints);
            foreach (var node in reachableNodes)
            {
                node.SetActionable(true, PathfindingNode.VisualType.Path);
            }
            selectingTarget = true;
        }
        
        public void AttackButton()
        {
            if (actionPoints <= 0) Debug.LogWarning("Not enough action points");
            
            // Placeholder for attack action
            AttackAction(Vector2Int.zero);
            NetworkManager.instance.SendAttackAction(Vector2Int.zero);
        }
    }
}