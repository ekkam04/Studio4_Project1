using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.ShaderGraph;
using UnityEngine;
using System.Threading.Tasks;
using QFSW.QC;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Animations;

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
        private bool isPathfindingJobRunning;
        private bool startFollowingPath = false;
        
        [Header("--- Agent Settings ---")] // ---------------------------
        
        public bool isTakingTurn;
        
        public GameObject leftHand;
        public GameObject rightHand;
        private Animator anim;
        private PropManager propManager;
        private TurnSystem turnSystem;
        
        public enum AgentType { Neutral, Friendly, Hostile }
        public AgentType agentType;
        
        public bool isTakingAction;
        public delegate void OnTurnEnd(AgentType agentType);
        public static OnTurnEnd onTurnEnd;
        
        public delegate void OnEliminated(AgentType agentType);
        public static OnEliminated onEliminated;
        
        public GameObject agentStatsUI;
        public GameObject agentStatsUIPoints;
        public bool showPoints = true;
        public TMP_Text nameText;
        
        public TMP_Text movementPointsText;
        public int movementPoints = 6;
        protected int maxMovementPoints;
        
        public TMP_Text actionPointsText;
        public int actionPoints = 2;
        protected int maxActionPoints;
        
        public TMP_Text manaPointsText;
        public int manaPoints = 4;
        protected int maxManaPoints;
        
        public int attackRange = 4;
        public List<Attack> attacks = new List<Attack>();
        
        protected void Start()
        {
            grid = FindObjectOfType<PathfindingGrid>();
            anim = GetComponent<Animator>();
            propManager = FindObjectOfType<PropManager>();
            turnSystem = FindObjectOfType<TurnSystem>();
            
            var mainCamera = Camera.main;
            agentStatsUI.GetComponent<RotationConstraint>().AddSource(new ConstraintSource {sourceTransform = mainCamera.transform, weight = 1});
            
            maxMovementPoints = movementPoints;
            maxActionPoints = actionPoints;
        }

        protected void Update()
        {
            if (findPath && !isPathfindingJobRunning)
            {
                StartFindPathJob();
            }
            
            if (startFollowingPath)
            {
                StartCoroutine(FollowPath());
                startFollowingPath = false;
            }
            
            movementPointsText.text = movementPoints.ToString();
            actionPointsText.text = actionPoints.ToString();
            manaPointsText.text = manaPoints.ToString();
            agentStatsUIPoints.SetActive(showPoints);
            
            // If no hostiles, set movement points to 99
            if (turnSystem.hostileCount == 0 && movementPoints != 99)
            {
                movementPoints = 99;
            }
            else if (turnSystem.hostileCount > 0 && movementPoints == 99)
            {
                movementPoints = maxMovementPoints;
            }
        }

        // --- Pathfinding ---------------------------------------------------
        
        void StartFindPathJob()
        {
            if (isPathfindingJobRunning) return;

            isPathfindingJobRunning = true;
            JobManager.instance.EnqueueJob(new Job(FindPathJob, OnPathFindingComplete));
            Debug.Log("Pathfinding job queued");
        }
        
        void FindPathJob()
        {
            if (pathNodes.Count > 0 || openNodes.Count < 1)
            {
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
                SetPathNodes();
                return;
            }

            var currentNeighbours = GetNeighbours(currentNode, currentNode.gridPosition);
            foreach (var neighbour in currentNeighbours)
            {
                if (neighbour == null) continue;

                if (neighbour.isBlocked || closedNodes.Contains(neighbour))
                {
                    continue;
                }

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
        }
        
        void OnPathFindingComplete()
        {
            Debug.Log("Pathfinding job complete");
            isPathfindingJobRunning = false;
            
            if (pathNodes.Count > 0)
            {
                findPath = false;
                // StartCoroutine(FollowPath()); // this doesn't work anymore because I can't start a coroutine from a job callback for some reason
                startFollowingPath = true; // this is a good workaround
                state = PathfindingState.Success;
            }
            else if (openNodes.Count < 1)
            {
                findPath = false;
                OnActionEnd();
                state = PathfindingState.Failure;
            }
            else
            {
                state = PathfindingState.Running;
            }
        }

        public void UpdateStartPosition(Vector2Int newStartPosition)
        {
            // #if PATHFINDING_DEBUG
            //     if (grid.GetNode(startNodePosition) != null) grid.GetNode(startNodePosition).ResetColor();
            // #endif

            // #if PATHFINDING_DEBUG
            //     foreach (var node in pathNodesColored)
            //     {
            //         node.ResetColor();
            //     }
            //     pathNodesColored.Clear();
            // #endif
            
            pathNodes.Clear();

            startNodePosition = newStartPosition;
            openNodes.Clear();
            closedNodes.Clear();

            // #if PATHFINDING_DEBUG
            //     grid.GetNode(startNodePosition).SetColor(startNodeColor);
            // #endif

            PathfindingNode startingNode = grid.GetNode(startNodePosition);
            startingNode.Occupant = gameObject;
            openNodes.Add(startingNode);
        }
        
        public void UpdateTargetPosition(Vector2Int newTargetPosition)
        {
            // #if PATHFINDING_DEBUG
            //     if (grid.GetNode(endNodePosition) != null) grid.GetNode(endNodePosition).ResetColor();
            // #endif

            endNodePosition = newTargetPosition;
            openNodes.Clear();
            closedNodes.Clear();

            // #if PATHFINDING_DEBUG
            //     grid.GetNode(endNodePosition).SetColor(endNodeColor);
            // #endif

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
                    // #if PATHFINDING_DEBUG
                    //     currentNode.SetPathColor(pathNodeColor);
                    //     pathNodesColored.Add(currentNode);
                    // #endif
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
        
        public List<PathfindingNode> GetReachableNodes(int range, bool filterByType = false, AgentType[] agentTypes = null)
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
            if (filterByType)
            {
                // reachableNodes.RemoveAll(node => node.Occupant == null || node.Occupant.GetComponent<Agent>().agentType != agentType);
                reachableNodes.RemoveAll(node => node.Occupant == null || Array.IndexOf(agentTypes, node.Occupant.GetComponent<Agent>().agentType) == -1);
            }
            return reachableNodes;
        }
        
        // --- Agent ---------------------------------------------------

        private void OnDestroy()
        {
            onEliminated?.Invoke(agentType);
        }

        public void ShowProp(string propKey)
        {
            propManager.ShowProp(propKey, gameObject.name, leftHand.transform);
        }
        
        public void HideProp(string propKey)
        {
            propManager.HideProp(propKey, gameObject.name);
        }
        
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
                if (turnSystem.hostileCount > 0) movementPoints--;
            }
            
            anim.SetBool("isMoving", false);
            grid.GetNode(endNodePosition).Occupant = this.gameObject;
            
            UpdateStartPosition(grid.GetPositionFromWorldPoint(transform.position));
            OnActionEnd();
        }
        
        public IEnumerator Shoot(Vector2Int targetPosition, float damage)
        {
            var initialRotation = transform.rotation;
            var targetPosition3D = grid.GetNode(targetPosition).transform.position;
            var duration = 0.5f;
            var time = 0f;
            while (time < duration)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition3D - transform.position), time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            transform.rotation = Quaternion.LookRotation(targetPosition3D - transform.position);
            anim.SetTrigger("shootWatergun");
            
            yield return new WaitForSeconds(2f);
            
            try
            {
                Agent targetAgent = grid.GetNode(targetPosition).Occupant.GetComponent<Agent>();
                targetAgent.TakeDamage(damage);
            }
            catch
            {
                Debug.LogWarning("No agent at target position");
            }
            
            NetworkManager.instance.HideActionCam();
            yield return new WaitForSeconds(2f);
            
            time = 0f;
            while (time < duration)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            
            OnActionEnd();
        }
        
        // --- Actions ---------------------------------------------------
        
        public virtual void StartTurn()
        {
            isTakingTurn = true;
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
        
        public void AttackAction(Vector2Int targetPosition, float damage)
        {
            OnActionStart();
            actionPoints--;
            NetworkManager.instance.ShowActionCam(this);
            StartCoroutine(Shoot(targetPosition, damage));
        }
        
        public void EndTurn()
        {
            isTakingTurn = false;
            movementPoints = 0;
            actionPoints = 0;
            onTurnEnd?.Invoke(agentType);
        }
    }
    
    // [System.Serializable]
    // public class AttackRange
    // {
    //     public string key;
    //     
    //     [Header("Forward Attack Range")]
    //     public bool[] frontLeft = new bool[] {};
    //     public bool[] frontMiddle = new bool[] {true, true};
    //     public bool[] frontRight = new bool[] {};
    //     
    //     [Header("Mirror")]
    //     public bool mirrorToRight = true;
    //     public bool mirrorToLeft = true;
    //     public bool mirrorToBack = true;
    // }
}