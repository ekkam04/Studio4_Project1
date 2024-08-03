using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AbilityManager : MonoBehaviour
{
    public GameObject inventroyItemPrefab;
    public AbilitySlot[] abilityInventory;
    public AbilitySlot[] abilityBar;
    
    void Update()
    {
        foreach (var slot in abilityBar)
        {
            if (Input.GetKeyDown(slot.keyCode))
            {
                UseAbility(slot);
            }
        }
    }

    public void AddAbility(ItemObject item)
    {
        for (int i = 0; i < abilityInventory.Length; i++)
        { 
            Debug.Log("working");
            AbilitySlot slot = abilityInventory[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null)
            {
                SpawnAbility(item, slot);
                break;
            }
        }
    }

    void SpawnAbility(ItemObject item,AbilitySlot slot)
    {
        GameObject go = Instantiate(inventroyItemPrefab, slot.transform);
        InventoryItem inventoryItem = go.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item);
    }
    void UseAbility(AbilitySlot slot)
    {
        InventoryItem abilityItem = slot.GetAbilityItem();
        if (abilityItem != null)
        {
            AbilityItemObject ability = (AbilityItemObject)abilityItem.item;

            if (ability != null)
            {
                
                Debug.Log($"Using ability: {ability.abilityName}");
            }
        }
    }
}
