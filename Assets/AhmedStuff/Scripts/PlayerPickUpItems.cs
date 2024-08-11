using System;
using System.Collections;
using System.Collections.Generic;
using Ekkam;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerPickUpItems : MonoBehaviour
{
    public InventoryItem inventoryItem;
    public InventoryManager inventoryManager;
    [HideInInspector] public bool pickedUp;
    
    private NetworkComponent networkComponent;
    
    void Start()
    {
        networkComponent = GetComponent<NetworkComponent>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!networkComponent.IsMine()) return;
        Item item = other.GetComponent<Item>();
        if (item != null)
        {
            bool result = inventoryManager.AddItem(item.item);
            if (result == true)
            {
                inventoryItem.InitializeItem(item.item);
                NetworkManager.instance.SendItemPacket(item.item.itemKey);
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
