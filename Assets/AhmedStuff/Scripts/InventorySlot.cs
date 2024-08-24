using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public bool isRewardSlot = false;
    
    // Ekkam: Added events for item dropped in inventory
    public delegate void OnItemDropped(string itemKey, string slotName);
    public static event OnItemDropped onItemDropped;
    
    public virtual void OnDrop(PointerEventData eventData)
    {
        if(isRewardSlot)return;
        if (transform.childCount == 0)
        {
            GameObject dropped = eventData.pointerDrag;
            InventoryItem inventoryItem = dropped.GetComponent<InventoryItem>();
            CallOnItemDropped(inventoryItem.item.itemKey, transform.name);
            inventoryItem.parentAfterDrag = transform; 
        }
      
    }
    
    // Ekkam: Added method to call event when item is dropped from this class and classes that inherit from this one
    public void CallOnItemDropped(string itemKey, string slotName)
    {
        onItemDropped?.Invoke(itemKey, slotName);
    }
}
