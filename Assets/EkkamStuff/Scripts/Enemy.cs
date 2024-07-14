using System.Collections;
using UnityEngine;

namespace Ekkam
{
    public class Enemy : Agent
    {
        private new void Start()
        {
            base.Start();
        }
        
        private new void Update()
        {
            base.Update();
        }
        
        public override void StartTurn()
        {
            base.StartTurn();
            StartCoroutine(SimulateTurn());
        }
        
        IEnumerator SimulateTurn()
        {
            yield return new WaitForSeconds(1f);
            TeabagAction();
            yield return new WaitForSeconds(0.75f);
            TeabagAction();
        }
    }
}