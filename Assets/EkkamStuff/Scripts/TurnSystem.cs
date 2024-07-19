using System;
using UnityEngine;

namespace Ekkam
{
    public class TurnSystem : MonoBehaviour
    {
        public int friendlyCount = Int32.MaxValue;
        public int friendlyTurnsCompleted;
        
        public int hostileCount = Int32.MaxValue;
        public int hostileTurnsCompleted;
        
        public Agent.AgentType currentTurn;
        
        EnemyManager enemyManager;
        
        private void OnEnable()
        {
            Agent.onTurnEnd += OnTurnEnd;
            Agent.onEliminated += OnEliminated;
            
            enemyManager = FindObjectOfType<EnemyManager>();
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
                    enemyManager.StartEnemyTurn();
                }
            }
            else if (agentType == Agent.AgentType.Hostile)
            {
                hostileTurnsCompleted++;
                if (hostileTurnsCompleted == hostileCount)
                {
                    hostileTurnsCompleted = 0;
                    currentTurn = Agent.AgentType.Friendly;
                    foreach (var agent in FindObjectsOfType<Agent>())
                    {
                        if (agent.agentType == Agent.AgentType.Friendly)
                        {
                            agent.StartTurn();
                        }
                    }
                }
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