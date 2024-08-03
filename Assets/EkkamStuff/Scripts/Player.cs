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
        // public bool selectingTarget;
        
        public enum SelectingTarget
        {
            None,
            Move,
            Attack
        }
        public SelectingTarget selectingTarget;
        
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
            
            var pickupSystem = GetComponent<PlayerPickUpItems>();
            pickupSystem.inventoryManager = GameObject.FindObjectOfType<InventoryManager>();
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
                
                if (selectedNode.Occupant == this.gameObject)
                {
                    if (selectingTarget == SelectingTarget.None) // Select player and show actions
                    {
                        print("Selected self");
                        selectedNode.SetActionable(false, PathfindingNode.VisualType.Selected);
                        lastSelectedNode = selectedNode;
                        uiManager.playerActionsUI.SetActive(true);
                    }
                    else // Cancel action
                    {
                        print("Selected self while selecting target");
                        selectedNode.SetActionable(false);
                        UnselectAction();
                        return;
                    }
                }

                switch (selectingTarget)
                {
                    case SelectingTarget.Move:
                        
                        if (!selectedNode.pathVisual.activeSelf)
                        {
                            print("Target is unreachable");
                            return;
                        }
                        
                        MoveAction(mousePositionOnGrid);
                        NetworkManager.instance.SendMoveAction(mousePositionOnGrid);
                        break;
                    
                    case SelectingTarget.Attack:
                        
                        if (!selectedNode.enemyVisual.activeSelf)
                        {
                            print("Target does not have an enemy");
                            return;
                        }
                        
                        float damage = 0f;
                        try
                        {
                            Agent targetAgent = grid.GetNode(mousePositionOnGrid).Occupant.GetComponent<Agent>();
                            damage = targetAgent.CalculateDamage(90f, 50f);
                        }
                        catch
                        {
                            Debug.LogWarning("No agent at target position");
                            OnActionEnd();
                            return;
                        }
                        
                        AttackAction(mousePositionOnGrid, damage);
                        NetworkManager.instance.SendAttackAction(mousePositionOnGrid, damage);
                        break;
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

            selectingTarget = SelectingTarget.None;
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
            
            if (reachableNodes.Count > 0)
            {
                selectingTarget = SelectingTarget.Move;
            }
            else
            {
                Debug.LogWarning("No reachable nodes");
                UnselectAction();
            }
        }
        
        public void AttackButton()
        {
            if (actionPoints <= 0) Debug.LogWarning("Not enough action points");
            
            reachableNodes = GetReachableNodes(attackRange, true, AgentType.Hostile);
            foreach (var node in reachableNodes)
            {
                node.SetActionable(true, PathfindingNode.VisualType.Enemy);
            }

            if (reachableNodes.Count > 0)
            {
                selectingTarget = SelectingTarget.Attack;
            }
            else
            {
                Debug.LogWarning("No enemies in range");
                UnselectAction();
            }
        }
    }
}