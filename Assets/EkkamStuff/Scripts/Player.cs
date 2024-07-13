using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{
    public class Player : Agent
    {
        [Header("--- Player Settings ---")]
        
        public GameObject mousePosition3DPrefab;
        private MousePosition3D mousePosition3D;

        private void Start()
        {
            base.Start();
            
            mousePosition3D = Instantiate(mousePosition3DPrefab).GetComponent<MousePosition3D>();
        }

        private void Update()
        {
            base.Update();
            
            Vector2Int mousePositionOnGrid = grid.GetPositionFromWorldPoint(mousePosition3D.transform.position); ;
            if (Input.GetMouseButtonDown(0) && grid.GetNode(mousePositionOnGrid) != null)
            {
                UpdateTargetPosition(mousePositionOnGrid);
                findPath = true;
            }
            
            if (Input.GetMouseButtonDown(1))
            {
                StartCoroutine(FollowPath());
            }
        }
    }
}