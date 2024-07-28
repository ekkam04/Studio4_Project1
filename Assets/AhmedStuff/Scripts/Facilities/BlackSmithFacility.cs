using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackSmithFacility : Facility
{
    [SerializeField] int reward;
    [SerializeField] private int nextRewardIncrement;


    public override void CheckForReward()
    {
        foreach (InventorySlot slot in playerInventory.inventorySlots)
        {
            if (slot.item.itemType == itemType && slot.amount >= reward)
            {
                GiveReward();
                reward += nextRewardIncrement;
                return;
            }
        }
        Debug.Log("No reward for " + facilityType);
    }
}
