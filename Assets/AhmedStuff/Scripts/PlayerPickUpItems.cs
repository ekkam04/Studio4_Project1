using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerPickUpItems : MonoBehaviour
{
    public InventoryObject playerInventory;
    [HideInInspector] public bool pickedUp;

    private void OnTriggerEnter(Collider other)
    {
        Item item = other.GetComponent<Item>();
        if (item != null)
        {
            playerInventory.AddItem(item.item,1);
            Destroy(other.gameObject);
        }

        pickedUp = true;
    }
    void OnApplicationQuit()
    {
        playerInventory.container.Clear();
    }
 
}
