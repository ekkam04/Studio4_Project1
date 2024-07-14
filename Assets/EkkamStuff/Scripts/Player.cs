using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ekkam
{
    public class Player : Agent
    {
        [Header("--- Player Settings ---")]
        
        public GameObject mousePosition3DPrefab;
        public GameObject selfActionUI;
        public Button moveButton;
        public Button shootButton;
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
            selfActionUI = GameObject.Find("GameUI");
            moveButton = GameObject.Find("MoveButton").GetComponent<Button>();
            moveButton.onClick.AddListener(MoveButton);
            shootButton = GameObject.Find("ShootButton").GetComponent<Button>();
            shootButton.onClick.AddListener(ShootButton);
        }

        private new void Update()
        {
            base.Update();
            if (!networkComponent.IsMine()) return;
            
            Vector2Int mousePositionOnGrid = grid.GetPositionFromWorldPoint(mousePosition3D.transform.position);
            
            if (Input.GetMouseButtonDown(0) && grid.GetNode(mousePositionOnGrid) != null)
            {
                var selectedNode = grid.GetNode(mousePositionOnGrid);

                if (!selectingTarget)
                {
                    lastSelectedNode?.SetActionable(false);
                    selfActionUI.SetActive(false);
                    print("Occupant: " + selectedNode.occupantText.text);
                    if (selectedNode.Occupant == null) return;

                    if (selectedNode.Occupant == this.gameObject)
                    {
                        print("Selected self");
                        selectedNode.SetActionable(false, PathfindingNode.VisualType.Selected);
                        lastSelectedNode = selectedNode;
                        selfActionUI.SetActive(true);
                    }
                }
                else
                {
                    if (!selectedNode.pathVisual.activeSelf)
                    {
                        print("Target is unreachable");
                        return;
                    }

                    lastSelectedNode.SetActionable(false);
                    selfActionUI.SetActive(false);
                    foreach (var node in reachableNodes)
                    {
                        node.SetActionable(false);
                    }

                    // Take action
                    MoveAction(mousePositionOnGrid);
                    NetworkManager.instance.SendMoveAction(mousePositionOnGrid);
                    
                    selectingTarget = false;
                }
            }
        }

        public override void StartTurn()
        {
            base.StartTurn();
        }

        public void MoveButton()
        {
            reachableNodes = GetReachableNodes(moveRange);
            foreach (var node in reachableNodes)
            {
                node.SetActionable(true, PathfindingNode.VisualType.Path);
            }
            selectingTarget = true;
        }
        
        public void ShootButton()
        {
            TeabagAction(); // Placeholder
        }
    }
}