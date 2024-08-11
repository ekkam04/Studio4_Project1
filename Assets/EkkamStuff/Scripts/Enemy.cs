using System.Collections;
using UnityEngine;

namespace Ekkam
{
    public class Enemy : Agent
    {
        public AgentData agentData;
        public enum EnemyRank
        {
            Regular,
            Elite,
            Boss
        }
        public EnemyRank enemyRank;
        
        // public bool isTakingTurn;
        
        private new void Start()
        {
            base.Start();
            agentData = new AgentData(gameObject.name, gameObject.name);
            nameText.text = gameObject.name;
            UpdateStartPosition(grid.GetPositionFromWorldPoint(transform.position));
        }
        
        private new void Update()
        {
            base.Update();
        }
        
        public override void StartTurn()
        {
            base.StartTurn();
            // isTakingTurn = true;
            StartCoroutine(SimulateTurn());
        }
        
        IEnumerator SimulateTurn()
        {
            yield return new WaitForSeconds(1f);
            var reachableNodes = GetReachableNodes(attackRange, true, new AgentType[] {AgentType.Friendly});
            if (reachableNodes.Count > 0)
            {
                var targetNode = reachableNodes[Random.Range(0, reachableNodes.Count)];
                AttackAction(targetNode.gridPosition, 50f);
                NetworkManager.instance.SendAttackAction(targetNode.gridPosition, 50f, agentData);
                yield return new WaitForSeconds(3f);
            }
            else
            {
                PathfindingNode closestFriendlyNode = null;
                float closestDistance = Mathf.Infinity;
                foreach (var node in grid.nodes)
                {
                    if (node.Occupant != null && node.Occupant.GetComponent<Agent>().agentType == AgentType.Friendly)
                    {
                        float distance = Vector2Int.Distance(node.gridPosition, grid.GetPositionFromWorldPoint(transform.position));
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestFriendlyNode = node;
                        }
                    }
                }
                if (closestFriendlyNode != null)
                {
                    reachableNodes = GetReachableNodes(movementPoints, false);
                    if (reachableNodes.Count > 0)
                    {
                        PathfindingNode targetNode = reachableNodes[0];
                        foreach (var node in reachableNodes)
                        {
                            if (Vector2Int.Distance(node.gridPosition, closestFriendlyNode.gridPosition) <
                                Vector2Int.Distance(targetNode.gridPosition, closestFriendlyNode.gridPosition))
                            {
                                if (node.Occupant != null) continue;
                                targetNode = node;
                            }
                        }

                        MoveAction(targetNode.gridPosition);
                        NetworkManager.instance.SendMoveAction(targetNode.gridPosition, agentData);
                        yield return new WaitForSeconds(3f);
                    }
                }
            }
            
            // isTakingTurn = false;
            EndTurn();
            NetworkManager.instance.SendEndTurn(agentData);
        }
    }
}