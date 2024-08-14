using System.Threading.Tasks;
using UnityEngine;

namespace Ekkam
{
    public class EnemySpawner : MonoBehaviour
    {
        public Enemy enemyPrefab;
        TurnSystem turnSystem;
        PathfindingGrid pathfindingGrid;
        
        private void Start()
        {
            turnSystem = FindObjectOfType<TurnSystem>();
            pathfindingGrid = FindObjectOfType<PathfindingGrid>();
        }
        
        public async void Spawn(Vector3 position)
        {
            Instantiate(enemyPrefab, position, Quaternion.identity);
            Player player = FindObjectOfType<Player>();
            var lookAtPlayer = player.transform.position - position;
            var lookAtPlayerXZ = new Vector3(lookAtPlayer.x, transform.position.y, lookAtPlayer.z);
            transform.rotation = Quaternion.LookRotation(lookAtPlayerXZ);
            
            turnSystem.hostileCount++;
            await Task.Delay(500);
            pathfindingGrid.UpdateBlockedNodes();
        }
    }
}