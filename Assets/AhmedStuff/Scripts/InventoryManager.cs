using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public int maxStackItems = 4;
    public GameObject inventroyItemPrefab;
    public InventorySlot[] playerInventory;
    public InventorySlot[] playerHotBar;
    public InventorySlot[] playerEquipmentInventory;
    // Start is called before the first frame update
    public bool AddItem(ItemObject item)
    {
        //check for slot with same item lower than max
        for (int i = 0; i < playerInventory.Length; i++)
        {
            InventorySlot slot = playerInventory[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null&&
                itemInSlot.item == item&&
                itemInSlot.count < maxStackItems&&
                itemInSlot.item.stackble == true)
            {
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return true;
            }
        }
        // Check for empty slot
        for (int i = 0; i < playerInventory.Length; i++)
        {
            InventorySlot slot = playerInventory[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null)
            {
                SpawnNewItem(item, slot);
                return true;
            }
        }

        return false;
    }

    void SpawnNewItem(ItemObject item, InventorySlot slot)
    {
        GameObject go = Instantiate(inventroyItemPrefab, slot.transform);
        InventoryItem inventoryItem = go.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item);
    }

}
