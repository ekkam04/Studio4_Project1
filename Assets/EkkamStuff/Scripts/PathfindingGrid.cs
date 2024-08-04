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
        public int gridCellCountX;
        public int gridCellCountZ;
        private int gridCellCount;
        public Vector3 startingPosition;
        
        public Grid grid;

        private float timer;
        
        void Start()
        {
            nodes = GetComponentsInChildren<PathfindingNode>();
            gridCellCountX = Mathf.RoundToInt(Mathf.Sqrt(nodes.Length));
            gridCellCountZ = gridCellCountX;
            gridCellCount = nodes.Length;
            foreach (var node in nodes)
            {
                var nodePosition = grid.WorldToCell(node.transform.position);
                node.gridPosition = new Vector2Int(nodePosition.x, nodePosition.y);
            }
            
            UpdateBlockedNodes();
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
        
        async void UpdateBlockedNodes()
        {
            print("Updating blocked nodes...");
            await Task.Delay(100);
            if (nodes == null) return;
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node == null) continue;
                int mask = ~LayerMask.GetMask(LayerMask.LayerToName(6), LayerMask.LayerToName(7), LayerMask.LayerToName(8)); // Player, Enemy and Item layers
                bool isBlocked = Physics.CheckBox(node.transform.position + new Vector3(0, 0.5f, 0), new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, mask);
                node.isBlocked = isBlocked;
                if (isBlocked)
                {
                    node.SetColor(new Color(0f, 0f, 0f, 0));
                }
                else
                {
                    node.ResetColor();
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
    }
}
