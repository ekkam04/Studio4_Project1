using System;
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
            hostileCount = CalculateHostileCount();
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
                    // foreach (var agent in FindObjectsOfType<Agent>())
                    // {
                    //     if (agent.agentType == Agent.AgentType.Friendly)
                    //     {
                    //         agent.StartTurn();
                    //     }
                    // }
                    StartFriendlyTurn();
                }
            }
        }
        
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
        
        private int CalculateHostileCount()
        {
            int count = 0;
            foreach (var agent in FindObjectsOfType<Agent>())
            {
                if (agent.agentType == Agent.AgentType.Hostile)
                {
                    count++;
                }
            }
            return count;
        }

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