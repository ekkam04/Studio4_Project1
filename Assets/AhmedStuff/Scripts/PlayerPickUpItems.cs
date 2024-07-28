using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerPickUpItems : MonoBehaviour
{
    public InventoryItem inventoryItem;
    public InventoryManager inventoryManager;
    [HideInInspector] public bool pickedUp;

    private void OnTriggerEnter(Collider other)
    {
        Item item = other.GetComponent<Item>();
        if (item != null)
        {
            bool result = inventoryManager.AddItem(item.item);
            if (result == true)
            {
                inventoryItem.InitializeItem(item.item);
                Destroy(other.gameObject); 
            }
            else
            {
                Debug.Log("Inventory Full");
            }
            
        }

        pickedUp = true;
    }
   /* void OnApplicationQuit()
    {
        playerInventory.inventorySlots.Clear();
    }*/
 
}
