using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentSlot : InventorySlot
{
    public InventoryManager inventoryManager;

    public override void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0 )
        {
            GameObject dropped = eventData.pointerDrag;
            InventoryItem inventoryItem = dropped.GetComponent<InventoryItem>();
            CallOnItemDropped(inventoryItem.item.itemKey, transform.name);
            if (inventoryItem.item.itemType == ItemType.Equipment)
            {
                inventoryItem.parentAfterDrag = transform;
                // inventoryManager.UpdatePlayerStats();
            }
        }
    }
}