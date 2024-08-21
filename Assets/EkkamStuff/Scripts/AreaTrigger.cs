using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{
    public class AreaTrigger : MonoBehaviour
    {
        public bool isTriggered;
        
        public enum ActionType
        {
            None,
            SpawnEnemy,
            SpawnItem
        }
        public ActionType action;
        
        [Header("Spawn Enemy")]
        public List<EnemySpawner> enemySpawners = new List<EnemySpawner>();
        public int enemiesToSpawn;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isTriggered)
            {
                switch (action)
                {
                    case ActionType.SpawnEnemy:
                        StartCoroutine(SpawnEnemies());
                        break;
                }
                isTriggered = true;
                Debug.Log("Player triggered the area trigger");
            }
        }
        
        IEnumerator SpawnEnemies()
        {
            List<EnemySpawner> spawners = enemySpawners;
            // for (int i = 0; i < enemiesToSpawn; i++)
            // {
            //     if (i >= spawners.Count || i <= 0) continue;
            //     spawners[i - 1].Spawn(spawners[i - 1].transform.position);
            //     yield return new WaitForSeconds(0.2f);
            // }
            
            // spawning alternate
            enemiesToSpawn = enemySpawners.Count;
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                if (i >= spawners.Count || i <= 0) continue;
                
                // ez diversity
                if (i == 1) continue;
                if (i == 3) continue;
                if (i == 5) continue;
                spawners[i - 1].Spawn(spawners[i - 1].transform.position);
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}