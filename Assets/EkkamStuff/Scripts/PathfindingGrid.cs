using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using QFSW.QC;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace Ekkam
{
    public class PathfindingGrid : MonoBehaviour
    {
        public PathfindingNode[] nodes;
        public int gridCellCountX = 100;
        public int gridCellCountZ = 100;
        private int gridCellCount;
        public Vector3 startingPosition;
        
        public Grid grid;

        private float timer;
        
        public float blockCheckHeightOffset = 0.5f;
        
        public float edgeOffset = 1f;
        public float edgeCheckSize = 0.7f;
        public float edgeCheckWidth = 0.1f;
        
        public float centerCheckSize = 0.2f;
        
        void Start()
        {
            nodes = GetComponentsInChildren<PathfindingNode>();
            // gridCellCountX = Mathf.RoundToInt(Mathf.Sqrt(nodes.Length));
            // gridCellCountZ = gridCellCountX;
            // gridCellCount = nodes.Length;
            foreach (var node in nodes)
            {
                var nodePosition = grid.WorldToCell(node.transform.position);
                node.gridPosition = new Vector2Int(nodePosition.x, nodePosition.y);
            }
            
            UpdateBlockedNodes();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                UpdateBlockedNodes();
            }
        }

        public PathfindingNode GetNode(Vector2Int gridPosition)
        {
            foreach (var node in nodes)
            {
                if (node.gridPosition == gridPosition)
                {
                    return node;
                }
            }
            return null;
        }
        
        public async void UpdateBlockedNodes()
        {
            print("Updating blocked nodes...");
            await Task.Delay(100);
            if (nodes == null) return;
            
            // for (int i = 0; i < nodes.Length; i++)
            // {
            //     var node = nodes[i];
            //     if (node == null) continue;
            //     int mask = ~LayerMask.GetMask(LayerMask.LayerToName(6), LayerMask.LayerToName(7), LayerMask.LayerToName(8), LayerMask.LayerToName(9)); // Player, Enemy, Item, Environment layers
            //     bool isBlocked = Physics.CheckBox(node.transform.position + new Vector3(0, 0.5f, 0), new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, mask);
            //     node.isBlocked = isBlocked;
            //     if (isBlocked)
            //     {
            //         node.SetColor(new Color(0f, 0f, 0f, 0));
            //     }
            //     else
            //     {
            //         node.ResetColor();
            //     }
            // }
            
            int mask = ~LayerMask.GetMask(LayerMask.LayerToName(6), LayerMask.LayerToName(7), LayerMask.LayerToName(8), LayerMask.LayerToName(9)); // Player, Enemy, Item, Environment layers
            int edgeMask = 1 << 10; // Only detect objects on layer 10 for edge detection
            
            foreach (var node in nodes)
            {
                if (node == null) continue;
        
                // Check if the entire node is blocked
                bool isBlocked = Physics.CheckBox(node.transform.position + new Vector3(0, blockCheckHeightOffset, 0), new Vector3(centerCheckSize, centerCheckSize, centerCheckSize), Quaternion.identity, mask);
                node.isBlocked = isBlocked;
                if (isBlocked)
                {
                    node.SetColor(new Color(0f, 0f, 0f, 0));
                    node.isBlockedFromTopEdge = true;
                    node.isBlockedFromBottomEdge = true;
                    node.isBlockedFromLeftEdge = true;
                    node.isBlockedFromRightEdge = true;
                    continue;
                }
                else
                {
                    node.ResetColor();
                }
                
                // Top Edge (Forward in world space)
                node.isBlockedFromTopEdge = Physics.CheckBox(node.transform.position + new Vector3(0, blockCheckHeightOffset, edgeOffset), new Vector3(0.5f, 0.5f, edgeCheckWidth), Quaternion.identity, mask);

                // Bottom Edge (Backward in world space)
                node.isBlockedFromBottomEdge = Physics.CheckBox(node.transform.position + new Vector3(0, blockCheckHeightOffset, -edgeOffset), new Vector3(edgeCheckSize, edgeCheckSize, edgeCheckWidth), Quaternion.identity, mask);

                // Left Edge (Left in world space)
                node.isBlockedFromLeftEdge = Physics.CheckBox(node.transform.position + new Vector3(-edgeOffset, blockCheckHeightOffset, 0), new Vector3(edgeCheckWidth, edgeCheckSize, edgeCheckSize), Quaternion.identity, mask);

                // Right Edge (Right in world space)
                node.isBlockedFromRightEdge = Physics.CheckBox(node.transform.position + new Vector3(edgeOffset, blockCheckHeightOffset, 0), new Vector3(edgeCheckWidth, edgeCheckSize, edgeCheckSize), Quaternion.identity, mask);
                
                int edgeCount = 0;
                if (node.isBlockedFromTopEdge) edgeCount++;
                if (node.isBlockedFromBottomEdge) edgeCount++;
                if (node.isBlockedFromLeftEdge) edgeCount++;
                if (node.isBlockedFromRightEdge) edgeCount++;
                
                if (edgeCount >= 2)
                {
                    node.cover = PathfindingNode.CoverType.Full;
                }
                else if (edgeCount == 1)
                {
                    node.cover = PathfindingNode.CoverType.Half;
                }
                else
                {
                    node.cover = PathfindingNode.CoverType.None;
                }
            }
        }

        public Vector2Int GetPositionFromWorldPoint(Vector3 worldPosition)
        {
            int x = grid.WorldToCell(worldPosition).x;
            int y = grid.WorldToCell(worldPosition).y;
            return new Vector2Int(x, y);
        }
        
        public int2[] GetBlockedPositions()
        {
            List<int2> blockedPositions = new List<int2>();
            foreach (var node in nodes)
            {
                if (node.isBlocked)
                {
                    blockedPositions.Add(new int2(node.gridPosition.x, node.gridPosition.y));
                }
            }
            return blockedPositions.ToArray();
        }

        private void OnDrawGizmos()
        {
            if (nodes == null) return;
            foreach (var node in nodes)
            {
                // draw gizmos for each edge box cast, red if blocked, green if not
                Gizmos.color = node.isBlockedFromTopEdge ? Color.red : Color.green;
                Gizmos.DrawWireCube(node.transform.position + new Vector3(0, blockCheckHeightOffset, edgeOffset), new Vector3(edgeCheckSize, edgeCheckSize, edgeCheckWidth));
                
                Gizmos.color = node.isBlockedFromBottomEdge ? Color.red : Color.green;
                Gizmos.DrawWireCube(node.transform.position + new Vector3(0, blockCheckHeightOffset, -edgeOffset), new Vector3(edgeCheckSize, edgeCheckSize, edgeCheckWidth));
                
                Gizmos.color = node.isBlockedFromLeftEdge ? Color.red : Color.green;
                Gizmos.DrawWireCube(node.transform.position + new Vector3(-edgeOffset, blockCheckHeightOffset, 0), new Vector3(edgeCheckWidth, edgeCheckSize, edgeCheckSize));
                
                Gizmos.color = node.isBlockedFromRightEdge ? Color.red : Color.green;
                Gizmos.DrawWireCube(node.transform.position + new Vector3(edgeOffset, blockCheckHeightOffset, 0), new Vector3(edgeCheckWidth, edgeCheckSize, edgeCheckSize));
                
                // draw gizmo for the center boxcast
                Gizmos.color = node.isBlocked ? Color.red : Color.green;
                Gizmos.DrawWireCube(node.transform.position + new Vector3(0, blockCheckHeightOffset, 0), new Vector3(centerCheckSize, centerCheckSize, centerCheckSize));
            }
        }
    }
}
