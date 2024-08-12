using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialChest : MonoBehaviour, IInteractable
{
    public ItemObject[] weapons; // Array of weapons the player can choose from
    public GameObject weaponSelectionUI; // Reference to the selection UI GameObject in the canvas
    public Transform player;
    public float interactDistance;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            float distance = Vector3.Distance(player.position, this.transform.position);
            if (distance <= interactDistance)
            {
                Interact();
            }
        }
    }

    public void Interact()
    {
        weaponSelectionUI.SetActive(true);
    }
}

