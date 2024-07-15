using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;
using System.Threading.Tasks;
using QFSW.QC;
using Unity.VisualScripting;

namespace Ekkam
{
    public class Agent : Damagable
    {
        [Header("--- Astar Settings ---")] // ---------------------------
        
        [SerializeField] public Vector2Int startNodePosition;
        [SerializeField] public Vector2Int endNodePosition;
        
        public PathfindingGrid grid;
        
        [SerializeField] Color startNodeColor = new Color(0, 0.5f, 0, 1);
        [SerializeField] Color endNodeColor = new Color(0.5f, 0, 0, 1);
        [SerializeField] Color pathNodeColor = new Color(0, 0, 0.5f, 1);

        public enum PathfindingState { Idle, Waiting, Running, Success, Failure }
        public PathfindingState state;
        
        public List<PathfindingNode> openNodes = new List<PathfindingNode>();
        public List<PathfindingNode> closedNodes = new List<PathfindingNode>();
        public List<PathfindingNode> pathNodes = new List<PathfindingNode>();
        private List<PathfindingNode> pathNodesColored = new List<PathfindingNode>();
        private PathfindingNode[] allNodes;

        public bool findPath;
        
        [Header("--- Agent Settings ---")] // ---------------------------
        
        private Animator anim;
        public bool isTakingAction;
        
        public enum AgentType { Neutral, Friendly, Hostile }
        public AgentType agentType;
        
        public delegate void OnTurnEnd(AgentType agentType);
        public static OnTurnEnd onTurnEnd;

        public int movementPoints = 6;
        private int maxMovementPoints;
        public int actionPoints = 2;
        private int maxActionPoints;
        public int shootRange = 4;

        protected void Start()
        {
            grid = FindObjectOfType<PathfindingGrid>();
            anim = GetComponent<Animator>();
            
            maxMovementPoints = movementPoints;
            maxActionPoints = actionPoints;
        }

        protected void Update()
        {
            if (findPath) FindPath();
        }

        // --- Pathfinding ---------------------------------------------------
        
        void FindPath()
        {
            if (pathNodes.Count > 0)
            {
                Debug.LogWarning("Path already found");
                findPath = false;
                OnActionEnd();
                state = PathfindingState.Success;
                return;
            }

            if (openNodes.Count < 1)
            {
                Debug.LogWarning("No path found");
                findPath = false;
                OnActionEnd();
                state = PathfindingState.Failure;
                return;
            }
            var currentNode = openNodes[0];
            foreach (var node in openNodes)
            {
                if (node.FCost < currentNode.FCost)
                {
                    currentNode = node;
                }
            }
            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);
            
            if (currentNode == grid.GetNode(endNodePosition))
            {
                print("Path found");
                findPath = false;
                SetPathNodes();
                StartCoroutine(FollowPath());
                state = PathfindingState.Success;
                return;
            }
            
            var currentNeighbours = GetNeighbours(currentNode, currentNode.gridPosition);
            foreach (var neighbour in currentNeighbours)
            {
                if (neighbour == null) continue;
                
                // check if it is in blocked positions or closed nodes
                if (neighbour.isBlocked || closedNodes.Contains(neighbour))
                {
                    continue;
                }
                // check if new path to neighbour is shorter or neighbour is not in openNodes
                var newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.GCost || !openNodes.Contains(neighbour))
                {
                    neighbour.GCost = newMovementCostToNeighbour;
                    neighbour.HCost = GetDistance(neighbour, grid.GetNode(endNodePosition));
                    neighbour.Parent = currentNode;
                    if (!openNodes.Contains(neighbour))
                    {
                        openNodes.Add(neighbour);
                    }
                }
            }
            state = PathfindingState.Running;
        }

        public void UpdateStartPosition(Vector2Int newStartPosition)
        {
            #if PATHFINDING_DEBUG
                grid.GetNode(startNodePosition).ResetColor();
            #endif

            #if PATHFINDING_DEBUG
                foreach (var node in pathNodesColored)
                {
                    node.ResetColor();
                }
                pathNodesColored.Clear();
            #endif
            
            pathNodes.Clear();

            startNodePosition = newStartPosition;
            openNodes.Clear();
            closedNodes.Clear();

            #if PATHFINDING_DEBUG
                grid.GetNode(startNodePosition).SetColor(startNodeColor);
            #endif

            PathfindingNode startingNode = grid.GetNode(startNodePosition);
            startingNode.Occupant = gameObject;
            openNodes.Add(startingNode);
        }
        
        public void UpdateTargetPosition(Vector2Int newTargetPosition)
        {
            #if PATHFINDING_DEBUG
                grid.GetNode(endNodePosition).ResetColor();
            #endif

            endNodePosition = newTargetPosition;
            openNodes.Clear();
            closedNodes.Clear();

            #if PATHFINDING_DEBUG
                grid.GetNode(endNodePosition).SetColor(endNodeColor);
            #endif

            UpdateStartPosition(grid.GetPositionFromWorldPoint(transform.position));

            if (grid.GetNode(endNodePosition).isBlocked)
            {
                print("End node is blocked");
                return;
            }
        }

        void SetPathNodes()
        {
            var currentNode = grid.GetNode(endNodePosition);
            pathNodes.Add(currentNode);
            while (currentNode != grid.GetNode(startNodePosition))
            {
                if (currentNode != grid.GetNode(endNodePosition)) {
                    pathNodes.Add(currentNode);
                    #if PATHFINDING_DEBUG
                        currentNode.SetPathColor(pathNodeColor);
                        pathNodesColored.Add(currentNode);
                    #endif
                }
                currentNode = currentNode.Parent;
            }
        }
        
        List<PathfindingNode> GetNeighbours(PathfindingNode node, Vector2Int nodePosition)
        {
            node.neighbours.Clear();
            Vector2Int rightNodePosition = new Vector2Int(nodePosition.x + 1, nodePosition.y);
            if (rightNodePosition.x < grid.gridCellCountX)
            {
                PathfindingNode rightNode = grid.GetNode(rightNodePosition);
                // rightNode.SetColor(new Color(0f, 0.25f, 0f , 1));
                node.neighbours.Add(rightNode);
            }
            Vector2Int leftNodePosition = new Vector2Int(nodePosition.x - 1, nodePosition.y);
            if (leftNodePosition.x >= -grid.gridCellCountX)
            {
                PathfindingNode leftNode = grid.GetNode(leftNodePosition);
                // leftNode.SetColor(new Color(0f, 0.25f, 0f , 1));
                node.neighbours.Add(leftNode);
            }
            Vector2Int upNodePosition = new Vector2Int(nodePosition.x, nodePosition.y + 1);
            if (upNodePosition.y < grid.gridCellCountZ)
            {
                PathfindingNode upNode = grid.GetNode(upNodePosition);
                // upNode.SetColor(new Color(0f, 0.25f, 0f , 1));
                node.neighbours.Add(upNode);
            }
            Vector2Int downNodePosition = new Vector2Int(nodePosition.x, nodePosition.y - 1);
            if (downNodePosition.y >= -grid.gridCellCountZ)
            {
                PathfindingNode downNode = grid.GetNode(downNodePosition);
                // downNode.SetColor(new Color(0f, 0.25f, 0f , 1));
                node.neighbours.Add(downNode);
            }

            return node.neighbours;
        }
        
        public int GetDistance(PathfindingNode nodeA, PathfindingNode nodeB)
        {
            int distanceX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
            int distanceY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);
            // return manhattan distance
            return distanceX + distanceY;
        }
        
        public List<PathfindingNode> GetReachableNodes(int range)
        {
            List<PathfindingNode> reachableNodes = new List<PathfindingNode>();
            List<PathfindingNode> openNodes = new List<PathfindingNode>();
            List<PathfindingNode> closedNodes = new List<PathfindingNode>();
            openNodes.Add(grid.GetNode(startNodePosition));
            int currentRange = 0;
            
            while (currentRange < range)
            {
                List<PathfindingNode> currentNodes = new List<PathfindingNode>(openNodes);
                openNodes.Clear();
                foreach (var node in currentNodes)
                {
                    foreach (var neighbour in GetNeighbours(node, node.gridPosition))
                    {
                        if (neighbour == null) continue;
                        if (neighbour.isBlocked || closedNodes.Contains(neighbour) || reachableNodes.Contains(neighbour))
                        {
                            continue;
                        }
                        openNodes.Add(neighbour);
                        reachableNodes.Add(neighbour);
                    }
                    closedNodes.Add(node);
                }
                currentRange++;
            }
            return reachableNodes;
        }
        
        // --- Agent ---------------------------------------------------
        
        public IEnumerator FollowPath()
        {
            grid.GetNode(startNodePosition).Occupant = null;
            anim.SetBool("isMoving", true);
            
            for (int i = pathNodes.Count - 1; i >= 0; i--)
            {
                Vector3 targetPosition = pathNodes[i].transform.position;
                while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, 5f * Time.deltaTime);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), 10f * Time.deltaTime);
                    yield return null;
                }
                movementPoints--;
            }
            
            anim.SetBool("isMoving", false);
            grid.GetNode(endNodePosition).Occupant = this.gameObject;
            
            UpdateStartPosition(grid.GetPositionFromWorldPoint(transform.position));
            OnActionEnd();
        }
        
        // --- Actions ---------------------------------------------------
        
        public virtual void StartTurn()
        {
            movementPoints = maxMovementPoints;
            actionPoints = maxActionPoints;
        }
        
        public virtual void OnActionStart()
        {
            isTakingAction = true;
        }
        
        public virtual void OnActionEnd()
        {
            isTakingAction = false;
        }

        public void MoveAction(Vector2Int targetPosition)
        {
            OnActionStart();
            UpdateTargetPosition(targetPosition);
            findPath = true; // finds path and starts following it if path is found
        }
        
        public async void AttackAction(Vector2Int targetPosition)
        {
            OnActionStart();
            actionPoints--;
            
            anim.SetTrigger("teabag");
            await Task.Delay(200);
            anim.SetTrigger("teabag");
            await Task.Delay(1000);
            
            OnActionEnd();
        }
        
        public void EndTurn()
        {
            movementPoints = 0;
            actionPoints = 0;
            onTurnEnd?.Invoke(agentType);
        }
    }
}