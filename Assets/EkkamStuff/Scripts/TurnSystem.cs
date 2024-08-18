﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{
    public class TurnSystem : MonoBehaviour
    {
        public bool isMasterClient;
        
        public int friendlyCount = Int32.MaxValue;
        public int friendlyTurnsCompleted;
        
        public int hostileCount = Int32.MaxValue;
        public int hostileTurnsCompleted;
        
        public Agent.AgentType currentTurn;
        public float hostilityRange = 15;
        
        private void OnEnable()
        {
            Agent.onTurnEnd += OnTurnEnd;
            Agent.onEliminated += OnEliminated;
        }
        
        private void OnDisable()
        {
            Agent.onTurnEnd -= OnTurnEnd;
            Agent.onEliminated -= OnEliminated;
        }
        
        private void OnTurnEnd(Agent.AgentType agentType)
        {
            Debug.Log("Turn ended");
            hostileCount = GetEnemiesInRange().Count;
            CheckForEndGame();
            
            if (agentType == Agent.AgentType.Friendly)
            {
                friendlyTurnsCompleted++;
                if (friendlyTurnsCompleted == friendlyCount)
                {
                    friendlyTurnsCompleted = 0;
                    currentTurn = Agent.AgentType.Hostile;
                    StartEnemyTurn();
                }
            }
            else if (agentType == Agent.AgentType.Hostile)
            {
                hostileTurnsCompleted++;
                if (hostileTurnsCompleted == hostileCount)
                {
                    hostileTurnsCompleted = 0;
                    currentTurn = Agent.AgentType.Friendly;
                    StartFriendlyTurn();
                }
            }
        }
        
        public void StartEnemyTurn()
        {
            if (!isMasterClient) return;
            
            // Get all enemies
            List<Enemy> enemies = GetEnemiesInRange();
            
            // If there are no enemies left, end the turn
            if (enemies.Count == 0)
            {
                Debug.LogWarning("No enemies to take turn.");
                hostileCount = 0;
                OnTurnEnd(Agent.AgentType.Hostile);
                return;
            }
            
            // Sort enemies by rank
            enemies.Sort((a, b) =>
            {
                if (a.enemyRank == b.enemyRank)
                {
                    return UnityEngine.Random.Range(-1, 1);
                }
                return a.enemyRank.CompareTo(b.enemyRank);
            });
            
            // Convert enemies to agents and start their turns
            List<Agent> enemyAgents = enemies.ConvertAll(x => (Agent)x);
            StopCoroutine(ExecuteTurns());
            StartCoroutine(ExecuteTurns(enemyAgents));
        }
        
        public void StartFriendlyTurn()
        {
            if (!isMasterClient) return;
            
            List<Agent> friendlyAgents = new List<Agent>();
            foreach (var agent in FindObjectsOfType<Agent>())
            {
                if (agent.agentType == Agent.AgentType.Friendly)
                {
                    friendlyAgents.Add(agent);
                }
            }
            StopCoroutine(ExecuteTurns());
            StartCoroutine(ExecuteTurns(friendlyAgents));
        }
        
        IEnumerator ExecuteTurns(List<Agent> agents = null)
        {
            foreach (var agent in agents)
            {
                if (agent.GetComponent<Player>())
                {
                    string agentID = agent.GetComponent<NetworkComponent>().ownerID;
                    NetworkManager.instance.SendStartTurn(agentID);
                }
                
                agent.StartTurn();
                
                yield return new WaitUntil(() => agent.isTakingTurn == false);
            }
        }
        
        public List<Enemy> GetEnemiesInRange()
        {
            List<Enemy> enemies = new List<Enemy>();
            foreach (var agent in FindObjectsOfType<Agent>())
            {
                if (agent.agentType == Agent.AgentType.Hostile && agent.GetComponent<Enemy>())
                {
                    enemies.Add(agent.GetComponent<Enemy>());
                }
            }
            
            List<Enemy> enemiesInRange = new List<Enemy>();
            foreach (var enemy in enemies)
            {
                bool playerInRange = false;
                foreach (var player in FindObjectsOfType<Player>())
                {
                    var distance = Vector3.Distance(enemy.transform.position, player.transform.position);
                    if (distance < hostilityRange)
                    {
                        playerInRange = true;
                        break;
                    }
                }
                if (playerInRange)
                {
                    enemiesInRange.Add(enemy);
                }
            }
            
            return enemiesInRange;
        }
        
        public List<Agent> GetHostilesInRange()
        {
            List<Agent> hostiles = new List<Agent>();
            foreach (var agent in FindObjectsOfType<Agent>())
            {
                if (agent.agentType == Agent.AgentType.Hostile)
                {
                    hostiles.Add(agent);
                }
            }
            
            List<Agent> hostilesInRange = new List<Agent>();
            foreach (var hostile in hostiles)
            {
                bool playerInRange = false;
                foreach (var player in FindObjectsOfType<Player>())
                {
                    var distance = Vector3.Distance(hostile.transform.position, player.transform.position);
                    if (distance < hostilityRange)
                    {
                        playerInRange = true;
                        break;
                    }
                }
                if (playerInRange)
                {
                    hostilesInRange.Add(hostile);
                }
            }
            
            return hostilesInRange;
        }
        
        // private int CalculateHostileCountNearMe()
        // {
        //     int count = 0;
        //     foreach (var agent in FindObjectsOfType<Agent>())
        //     {
        //         if (agent.agentType == Agent.AgentType.Hostile)
        //         {
        //             count++;
        //         }
        //     }
        //     return count;
        // }

        private void OnEliminated(Agent.AgentType agentType)
        {
            // Friendly agents are only recalculated if they are eliminated
            if (agentType == Agent.AgentType.Friendly)
            {
                friendlyCount--;
            }

            CheckForEndGame();
        }
        
        private void CheckForEndGame()
        {
            if (friendlyCount == 0)
            {
                Debug.LogWarning("Hostiles win!");
            }
            else if (hostileCount == 0)
            {
                Debug.LogWarning("Friendlies win!");
            }
        }
    }
}