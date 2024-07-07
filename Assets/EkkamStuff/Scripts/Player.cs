using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{
    public class Player : MonoBehaviour
    {
        public GameObject mousePosition3DPrefab;
        private MousePosition3D mousePosition3D;
        private PathfindingGrid grid;
        private Astar astar;

        private void Start()
        {
            mousePosition3D = Instantiate(mousePosition3DPrefab).GetComponent<MousePosition3D>();
            grid = FindObjectOfType<PathfindingGrid>();
            astar = GetComponent<Astar>();
        }

        private void Update()
        {
            Vector2Int mousePositionOnGrid = grid.GetPositionFromWorldPoint(mousePosition3D.transform.position); ;
            if (Input.GetMouseButtonDown(0) && grid.GetNode(mousePositionOnGrid) != null)
            {
                // print("Grid position of mouse: " + mousePositionOnGrid);
                astar.UpdateTargetPosition(mousePositionOnGrid);
                astar.findPath = true;
            }
            
            if (Input.GetMouseButtonDown(1))
            {
                StartCoroutine(FollowPathCoroutine());
            }
        }
        
        IEnumerator FollowPathCoroutine()
        {
            grid.GetNode(astar.startNodePosition).Occupant = null;
            for (int i = astar.pathNodes.Count - 1; i >= 0; i--)
            {
                astar.pathNodes[i].Occupant = this.gameObject;
                Vector3 targetPosition = new Vector3(astar.pathNodes[i].transform.position.x, transform.position.y, astar.pathNodes[i].transform.position.z);
                while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, 5 * Time.deltaTime);
                    yield return null;
                }
                transform.position = targetPosition;
                astar.pathNodes[i].Occupant = null;
            }
            grid.GetNode(astar.endNodePosition).Occupant = this.gameObject;
        }
    }
}