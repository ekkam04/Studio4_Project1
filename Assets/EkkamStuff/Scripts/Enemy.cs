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
        
        public bool isTakingTurn;
        
        private new void Start()
        {
            base.Start();
            agentData = new AgentData(gameObject.name, gameObject.name);
        }
        
        private new void Update()
        {
            base.Update();
        }
        
        public override void StartTurn()
        {
            base.StartTurn();
            isTakingTurn = true;
            StartCoroutine(SimulateTurn());
        }
        
        IEnumerator SimulateTurn()
        {
            yield return new WaitForSeconds(1f);
            AttackAction(Vector2Int.zero);
            NetworkManager.instance.SendAttackAction(Vector2Int.zero, agentData);
            yield return new WaitForSeconds(1f);
            
            isTakingTurn = false;
            EndTurn();
            NetworkManager.instance.SendEndTurn(agentData);
        }
    }
}