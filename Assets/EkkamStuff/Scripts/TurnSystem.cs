using System;
using UnityEngine;

namespace Ekkam
{
    public class TurnSystem : MonoBehaviour
    {
        public int friendlyCount = Int32.MaxValue;
        public int friendlyTurnsCompleted;
        
        public int hostileCount;
        public int hostileTurnsCompleted;
        
        public Agent.AgentType currentTurn;
        
        EnemyManager enemyManager;
        
        private void OnEnable()
        {
            Agent.onTurnEnd += OnTurnEnd;
            enemyManager = FindObjectOfType<EnemyManager>();
            foreach (var agent in FindObjectsOfType<Agent>())
            {
                if (agent.agentType == Agent.AgentType.Hostile)
                {
                    hostileCount++;
                }
            }
        }
        
        private void OnDisable()
        {
            Agent.onTurnEnd -= OnTurnEnd;
        }
        
        private void OnTurnEnd(Agent.AgentType agentType)
        {
            Debug.Log("Turn ended");
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
    }
}