﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Ekkam
{
    public class Player : Agent
    {
        public delegate void OnLocalPlayerLoaded();
        public static event OnLocalPlayerLoaded onLocalPlayerLoaded;
        
        [Header("--- Player Settings ---")]
        
        public GameObject mousePosition3DPrefab;
        
        private UIManager uiManager;
        private MousePosition3D mousePosition3D;
        public List<PathfindingNode> reachableNodes = new List<PathfindingNode>();
        
        public List<PathfindingNode> attackableNodes = new List<PathfindingNode>();
        public List<PathfindingNode> attackableNodesNorth = new List<PathfindingNode>();
        public List<PathfindingNode> attackableNodesEast = new List<PathfindingNode>();
        public List<PathfindingNode> attackableNodesSouth = new List<PathfindingNode>();
        public List<PathfindingNode> attackableNodesWest = new List<PathfindingNode>();

        public AttackDirection currentAttackDirection;
        [FormerlySerializedAs("currentAttack")] public Ability currentAbility;
        
        public PathfindingNode lastSelectedNode;
        public bool gunEquipped;
        
        public enum SelectingTarget
        {
            None,
            Move,
            Attack,
            Ability
        }
        public SelectingTarget selectingTarget;
        
        private NetworkComponent networkComponent;
        
        public CinemachineVirtualCamera playerCamera;

        private new void Start()
        {
            base.Start();
            networkComponent = GetComponent<NetworkComponent>();
            nameText.text = networkComponent.ownerName;
            
            if (!networkComponent.IsMine())
            {
                healthSlider.fillRect.GetComponent<Image>().color = Color.yellow;
                // agentType = AgentType.Hostile;
                return;
            }
            else
            {
                healthSlider.fillRect.GetComponent<Image>().color = Color.green;
            }
            
            networkComponent = GetComponent<NetworkComponent>();
            mousePosition3D = Instantiate(mousePosition3DPrefab).GetComponent<MousePosition3D>();
            
            uiManager = GameObject.FindObjectOfType<UIManager>();
            uiManager.AssignPlayerActions(this);
            
            var pickupSystem = GetComponent<PlayerPickUpItems>();
            pickupSystem.inventoryManager = GameObject.FindObjectOfType<InventoryManager>();
            
            if (networkComponent.IsMine()) onLocalPlayerLoaded?.Invoke();
        }

        private void OnEnable()
        {
            InventorySlot.onItemDropped += HandleItemDropped;
        }
        
        private void OnDisable()
        {
            InventorySlot.onItemDropped -= HandleItemDropped;
        }

        private new void Update()
        {
            base.Update();
            if (!networkComponent.IsMine()) return;
            
            if (isTakingAction) return;
            
            Vector2Int mousePositionOnGrid = grid.GetPositionFromWorldPoint(mousePosition3D.transform.position);

            // Update current attack direction when selecting target for ability
            if (selectingTarget == SelectingTarget.Ability)
            {
                Vector3 mouseWorldPosition = mousePosition3D.transform.position;
                Vector3 directionToMouse = (mouseWorldPosition - transform.position).normalized;
                
                if (Mathf.Abs(directionToMouse.x) > Mathf.Abs(directionToMouse.z))
                {
                    if (directionToMouse.x > 0) // East
                    {
                        if (currentAttackDirection != AttackDirection.East)
                        {
                            foreach (var node in attackableNodes)
                            {
                                node.SetActionable(false);
                                node.SetActionable(false, PathfindingNode.VisualType.Outline);
                            }
                            foreach (var node in attackableNodesEast)
                            {
                                node.SetActionable(true, PathfindingNode.VisualType.Enemy);
                            }
                        }
                        currentAttackDirection = AttackDirection.East;
                    }
                    else // West
                    {
                        if (currentAttackDirection != AttackDirection.West)
                        {
                            foreach (var node in attackableNodes)
                            {
                                node.SetActionable(false);
                                node.SetActionable(false, PathfindingNode.VisualType.Outline);
                            }
                            foreach (var node in attackableNodesWest)
                            {
                                node.SetActionable(true, PathfindingNode.VisualType.Enemy);
                            }
                        }
                        currentAttackDirection = AttackDirection.West;
                    }
                }
                else
                {
                    if (directionToMouse.z > 0) // North
                    {
                        if (currentAttackDirection != AttackDirection.North)
                        {
                            foreach (var node in attackableNodes)
                            {
                                node.SetActionable(false);
                                node.SetActionable(false, PathfindingNode.VisualType.Outline);
                            }
                            foreach (var node in attackableNodesNorth)
                            {
                                node.SetActionable(true, PathfindingNode.VisualType.Enemy);
                            }
                        }
                        currentAttackDirection = AttackDirection.North;
                    }
                    else // South
                    {
                        if (currentAttackDirection != AttackDirection.South)
                        {
                            foreach (var node in attackableNodes)
                            {
                                node.SetActionable(false);
                                node.SetActionable(false, PathfindingNode.VisualType.Outline);
                            }
                            foreach (var node in attackableNodesSouth)
                            {
                                node.SetActionable(true, PathfindingNode.VisualType.Enemy);
                            }
                        }
                        currentAttackDirection = AttackDirection.South;
                    }
                }
            }
            
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
                        // print("Selected self");
                        // selectedNode.SetActionable(false, PathfindingNode.VisualType.Selected);
                        // lastSelectedNode = selectedNode;
                        // uiManager.playerActionsUI.SetActive(true);
                        SelectSelf();
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
                        
                        float damage;
                        try
                        {
                            Agent targetAgent = grid.GetNode(mousePositionOnGrid).Occupant.GetComponent<Agent>();
                            damage = targetAgent.CalculateDamage(100 - targetAgent.evasion, 50f);
                        }
                        catch
                        {
                            Debug.LogWarning("No agent at target position");
                            OnActionEnd();
                            return;
                        }
                        
                        AttackAction(mousePositionOnGrid, damage);
                        NetworkManager.instance.SendAttackAction(mousePositionOnGrid, damage);
                        if (damage > 0) manaPoints += 1;
                        break;
                    
                    case SelectingTarget.Ability:
                        
                        if (!selectedNode.enemyVisual.activeSelf)
                        {
                            print("Target does not have an enemy");
                            return;
                        }
                        
                        print("Using ability in direction: " + currentAttackDirection);
                        AbilityAction(currentAbility.abilityName, currentAttackDirection);
                        NetworkManager.instance.SendAbilityAction(currentAbility.abilityName, currentAttackDirection);
                        break;
                }
            }
        }
        
        public void OnCameraMovePressed(InputAction.CallbackContext context)
        {
            if (playerCamera == null) return;
            Debug.Log("Camera move pressed");
            
            if (context.performed)
            {
                playerCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.m_MaxSpeed = 200;
            }
            else if (context.canceled)
            {
                playerCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.m_MaxSpeed = 0;
            }
        }
        
        private void SelectSelf()
        {
            var selectedNode = grid.GetNode(grid.GetPositionFromWorldPoint(transform.position));
            selectedNode.SetActionable(false, PathfindingNode.VisualType.Selected);
            lastSelectedNode = selectedNode;
            uiManager.playerActionsUI.SetActive(true);
        }
        
        private void UnselectAction()
        {
            lastSelectedNode?.SetActionable(false);
            uiManager.playerActionsUI.SetActive(false);
            foreach (var node in reachableNodes)
            {
                node.SetActionable(false);
            }
            foreach (var node in attackableNodes)
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
            
            SelectSelf();
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
            SelectSelf();
        }

        private void HandleItemDropped(string itemKey, string slotName)
        {
            if (itemKey == "gun")
            {
                gunEquipped = slotName == "WeaponSlot";
            }
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
            if (movementPoints <= 0)
            {
                Debug.LogWarning("Not enough movement points");
                return;
            }
            
            reachableNodes = GetReachableNodes(movementPoints);
            reachableNodes.RemoveAll(x => x.Occupant != null);
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
            if (actionPoints <= 0)
            {
                Debug.LogWarning("Not enough action points");
                return;
            }
            
            reachableNodes = GetReachableNodesWithoutNeighborCheck(attackRange);
            reachableNodes.RemoveAll(x => x.Occupant == null);
            reachableNodes.RemoveAll(x => x.Occupant == this.gameObject);
            
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
        
        public void AbilityButton(Ability ability, List<Ability> allAbilities = null)
        {
            if (allAbilities != null) attacks = allAbilities;
            if (selectingTarget == SelectingTarget.Ability)
            {
                UnselectAction();
                return;
            }
            
            if (actionPoints <= 0)
            {
                Debug.LogWarning("Not enough action points");
                return;
            }
            
            if (manaPoints <= 0)
            {
                Debug.LogWarning("Not enough mana points");
                return;
            }
            
            if (ability == null)
            {
                Debug.LogWarning("No attack with name: " + ability.abilityName);
                return;
            }
            currentAbility = ability;

            attackableNodesNorth = GetAllAttackNodes(ability, AttackDirection.North);
            attackableNodesEast = GetAllAttackNodes(ability, AttackDirection.East);
            attackableNodesSouth = GetAllAttackNodes(ability, AttackDirection.South);
            attackableNodesWest = GetAllAttackNodes(ability, AttackDirection.West);
            
            attackableNodes = new List<PathfindingNode>();
            attackableNodes.AddRange(attackableNodesNorth);
            attackableNodes.AddRange(attackableNodesEast);
            attackableNodes.AddRange(attackableNodesSouth);
            attackableNodes.AddRange(attackableNodesWest);

            foreach (var node in attackableNodes)
            {
                node.SetActionable(false, PathfindingNode.VisualType.Outline);
            }
            
            if (attackableNodes.Count > 0)
            {
                selectingTarget = SelectingTarget.Ability;
            }
            else
            {
                Debug.LogWarning("No spaces to use ability");
                UnselectAction();
            }
        }
        
        // public void ActivateDragonsBreath()
        // {
        //     AbilityButton("Dragons Breath");
        // }

        public async void SetCameraFocus(int delay, Transform focus = null)
        {
            await Task.Delay(delay);
            if (focus == null)
            {
                playerCamera.Follow = transform;
                playerCamera.LookAt = transform;
            }
            else
            {
                playerCamera.Follow = focus;
                playerCamera.LookAt = focus;
            }
        }
    }
}