using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private Facility currentFacility;

    private void OnTriggerEnter(Collider other)
    {
        Facility facility = other.GetComponent<Facility>();
        if (facility != null)
        {
            currentFacility = facility;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Facility facility = other.GetComponent<Facility>();
        if (facility != null && facility == currentFacility)
        {
            currentFacility = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentFacility != null)
        {
            currentFacility.CheckForReward();
        }
    }
}
