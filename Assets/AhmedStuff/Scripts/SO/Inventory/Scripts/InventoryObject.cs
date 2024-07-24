using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "new inventory object", menuName = "Inventory System/Inventroy")]
public class InventoryObject : ScriptableObject
{
    public List<InventorySlot> container = new List<InventorySlot>();

    public void AddItem(ItemObject _item, int _amount)
    {
        bool hasItem = false;
        for (int i = 0; i < container.Count; i++)
        {
            if (container[i].item == _item)
            {
                if (container[i].item.stackble)
                {
                    container[i].AddAmount(_amount);
                }
                hasItem = true;
                break;
            }
        }

        if (_item.stackble == false || !hasItem)
        {
            container.Add(new InventorySlot(_item, _amount));
        }
    }
    
}
