using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AbilitySlot : InventorySlot
{
    public KeyCode keyCode;

    public override void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            GameObject dropped = eventData.pointerDrag;
            InventoryItem inventoryItem = dropped.GetComponent<InventoryItem>();
            CallOnItemDropped(inventoryItem.item.itemKey, transform.name);
            if (inventoryItem.item.itemType == ItemType.Ability)
            {
                inventoryItem.parentAfterDrag = transform;
            }
        }
    }

    public InventoryItem GetAbilityItem()
    {
        if (transform.childCount > 0)
        {
            return transform.GetChild(0).GetComponent<InventoryItem>();
        }
        return null;
    }
}
