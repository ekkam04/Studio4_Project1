using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{
    public class EnemyManager : MonoBehaviour
    {
        public bool isMasterClient;
        
        public void StartEnemyTurn()
        {
            if (!isMasterClient) return;
            
            List<Enemy> enemies = new List<Enemy>();
            foreach (var agent in FindObjectsOfType<Agent>())
            {
                if (agent.agentType == Agent.AgentType.Hostile && agent.GetComponent<Enemy>())
                {
                    enemies.Add(agent.GetComponent<Enemy>());
                }
            }
            
            enemies.Sort((a, b) =>
            {
                if (a.enemyRank == b.enemyRank)
                {
                    return UnityEngine.Random.Range(-1, 1);
                }
                return a.enemyRank.CompareTo(b.enemyRank);
            });
            
            StartCoroutine(ExecuteEnemyTurns(enemies));
        }
        
        IEnumerator ExecuteEnemyTurns(List<Enemy> enemies)
        {
            foreach (var enemy in enemies)
            {
                enemy.StartTurn();
                yield return new WaitUntil(() => enemy.isTakingTurn == false);
            }
        }
    }
}