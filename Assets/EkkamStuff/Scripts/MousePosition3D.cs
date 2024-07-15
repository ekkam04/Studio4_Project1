using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{

    public class MousePosition3D : MonoBehaviour
    {
        private Camera mainCamera;
        public LayerMask layerMask;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, layerMask))
            {
                transform.position = hit.point;
            }
        }
    }
}
