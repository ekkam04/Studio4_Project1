using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "new inventory object", menuName = "Inventory System/Inventroy")]
public class InventoryObject : ScriptableObject
{
    public GameObject inventroyItemPrefab;
    public InventorySlot[] inventorySlots;

    public void AddItem(ItemObject item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null)
            {
                SpawnNewItem(item, slot);
                return;
            }
        }
    }

    void SpawnNewItem(ItemObject item, InventorySlot slot)
    {
        GameObject go = Instantiate(inventroyItemPrefab, slot.transform);
        InventoryItem inventoryItem = go.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item);
    }
    
}
