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
            List<EnemySpawner> spawners = new List<EnemySpawner>(enemySpawners);
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                int randomIndex = Random.Range(0, spawners.Count);
                spawners[randomIndex].Spawn(spawners[randomIndex].transform.position);
                spawners.RemoveAt(randomIndex);
                if (spawners.Count == 0) break;
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}