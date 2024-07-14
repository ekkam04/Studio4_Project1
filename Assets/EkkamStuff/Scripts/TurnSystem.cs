﻿using System;
using UnityEngine;

namespace Ekkam
{
    public class TurnSystem : MonoBehaviour
    {
        public int friendlyCount;
        public int friendlyTurnsCompleted;
        
        public int hostileCount;
        public int hostileTurnsCompleted;
        
        public Agent.AgentType currentTurn;
        
        private void OnEnable()
        {
            Agent.onTurnEnd += OnTurnEnd;
            foreach (var agent in FindObjectsOfType<Agent>())
            {
                if (agent.agentType == Agent.AgentType.Friendly)
                {
                    friendlyCount++;
                }
                else if (agent.agentType == Agent.AgentType.Hostile)
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
                    foreach (var agent in FindObjectsOfType<Agent>())
                    {
                        if (agent.agentType == Agent.AgentType.Hostile)
                        {
                            agent.StartTurn();
                        }
                    }
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