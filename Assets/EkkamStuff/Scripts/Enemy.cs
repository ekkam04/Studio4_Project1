using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

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
        
        // public static int enemyID = 0;

        private new void Start()
        {
            // enemyID++;
            // gameObject.name = "Enemy_" + enemyID;
            
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
            var reachableNodes = GetReachableNodesWithoutNeighborCheck(attackRange);
            reachableNodes.RemoveAll(x => x.Occupant == null);
            reachableNodes.RemoveAll(x => x.Occupant != null && x.Occupant.GetComponent<Agent>().agentType == AgentType.Hostile);
            reachableNodes.RemoveAll(x => x.Occupant == this.gameObject);
            
            if (reachableNodes.Count > 0)
            {
                foreach (var node in reachableNodes)
                {
                    node.SetActionable(true, PathfindingNode.VisualType.Enemy);
                }
                
                var targetNode = reachableNodes[Random.Range(0, reachableNodes.Count)];
                var damage = targetNode.Occupant.GetComponent<Agent>().CalculateDamage(100 - targetNode.Occupant.GetComponent<Agent>().evasion, 25f);
                AttackAction(targetNode.gridPosition, damage);
                NetworkManager.instance.SendAttackAction(targetNode.gridPosition, damage, agentData);
                yield return new WaitForSeconds(3f);
                
                foreach (var node in reachableNodes)
                {
                    node.SetActionable(false);
                }
            }
            else
            {
                Debug.Log("No reachable nodes for attack. Moving to closest friendly node.");
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
                        if (targetNode.Occupant != null)
                        {
                            Debug.LogWarning("Target node is occupied");
                            EndTurn();
                            NetworkManager.instance.SendEndTurn(agentData);
                            yield break;
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