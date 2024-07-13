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

        private Animator anim;
        
        private PathfindingGrid grid;
        private Astar astar;

        private void Start()
        {
            mousePosition3D = Instantiate(mousePosition3DPrefab).GetComponent<MousePosition3D>();
            anim = GetComponent<Animator>();
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
                StartCoroutine(FollowPath());
            }
        }
        
        IEnumerator FollowPath()
        {
            grid.GetNode(astar.startNodePosition).Occupant = null;
            anim.SetBool("isMoving", true);
            
            for (int i = astar.pathNodes.Count - 1; i >= 0; i--)
            {
                Vector3 targetPosition = astar.pathNodes[i].transform.position;
                while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, 5f * Time.deltaTime);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), 10f * Time.deltaTime);
                    yield return null;
                }
            }
            
            anim.SetBool("isMoving", false);
            grid.GetNode(astar.endNodePosition).Occupant = this.gameObject;
        }
    }
}