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
    public InventorySlot[] playerEquipmentSlots;
    public CraftingSlot[] craftingSlots;
    public InventorySlot rewardSlot;
    public ItemObject craftedObject;
    public CraftingRecipe[] craftingRecipes;
    public AgentStats agentStats;
    
    // Start is called before the first frame update
    public bool AddItem(ItemObject item)
    {
        //check for slot with same item lower than max
        for (int i = 0; i < playerInventory.Length; i++)
        {
            InventorySlot slot = playerInventory[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null &&
                itemInSlot.item == item &&
                itemInSlot.count < maxStackItems &&
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
    
    public void CheckCrafting()
    {
        InventoryItem item1 = craftingSlots[0].GetComponentInChildren<InventoryItem>();
        InventoryItem item2 = craftingSlots[1].GetComponentInChildren<InventoryItem>();

        if (item1 != null && item2 != null)
        {
            foreach (CraftingRecipe recipe in craftingRecipes)
            {
                if ((item1.item == recipe.item1 && item2.item == recipe.item2) ||
                    (item1.item == recipe.item2 && item2.item == recipe.item1))
                {
                    SpawnNewItem(recipe.result, rewardSlot);

                    Destroy(item1.gameObject);
                    Destroy(item2.gameObject);

                    break;
                }
            }
        }
    }

    public void UpdatePlayerStats()
    {
        foreach (InventorySlot slot in playerEquipmentSlots)
        {
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.item.itemType == ItemType.Equipment)
            {
                EquipmentItemObject equipmentItem = itemInSlot.item as EquipmentItemObject;
                 agentStats.AddStats(equipmentItem.attackValue, equipmentItem.armorValue, equipmentItem.evasionValue);
            }
        }
    }
}
