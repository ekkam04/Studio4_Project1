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
    public AbilityManager abilityManager;
    public InventoryManager inventoryManager;
    [HideInInspector] public bool pickedUp;
    
    private NetworkComponent networkComponent;
    public delegate void OnItemPickedUp(string itemKey);
    public static event OnItemPickedUp onItemPickedUp;
    
    void Start()
    {
        networkComponent = FindObjectOfType<NetworkComponent>();
    }

    private void OnTriggerEnter(Collider other)
    {
       //if (!networkComponent.IsMine()) return;
        Item item = other.GetComponent<Item>();
        if (item != null)
        {
            if (item.item.itemType != ItemType.Ability&& item.item.itemType != ItemType.Equipment)
            {
                bool result = inventoryManager.AddItem(item.item);
                if (result)
                {
                    inventoryItem.InitializeItem(item.item);
                    NetworkManager.instance.SendItemPacket(item.item.itemKey);
                    Destroy(other.gameObject);

                }
            }

            if (item.item.itemType == ItemType.Ability)
            {
                abilityManager.AddAbility((AbilityItemObject)item.item);
                inventoryItem.InitializeItem(item.item);
                NetworkManager.instance.SendItemPacket(item.item.itemKey);
                onItemPickedUp?.Invoke(item.item.itemKey);
                Destroy(other.gameObject);
            }
            else if (item.item.itemType == ItemType.Equipment)
            {
                inventoryManager.AddItem(item.item);
                EquipmentItemObject equipmentItem = (EquipmentItemObject)item.item;
                if (equipmentItem.ability != null && equipmentItem.ability is AbilityItemObject)
                {
                    
                    abilityManager.AddAbility((AbilityItemObject)equipmentItem.ability);
                    inventoryItem.InitializeItem(item.item);
                    NetworkManager.instance.SendItemPacket(item.item.itemKey);
                    Destroy(other.gameObject);
                }
            }

            pickedUp = true;
        }
        else
        {
            Debug.Log("Inventory Full");
        }
        
    }
        /* void OnApplicationQuit()
         {
             playerInventory.inventorySlots.Clear();
         }*/
}
