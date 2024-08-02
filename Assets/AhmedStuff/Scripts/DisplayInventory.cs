using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/*
public class DisplayInventory : MonoBehaviour
{
    public List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
    public InventoryManager inventoryManager;
    public PlayerPickUpItems player;
    

    private void Update()
    {
        if (player.pickedUp)
        {
            int count = player.inventoryManager.playerInventory.Length;
            for (int i = 0; i < texts.Count; i++)
            {
                if (i < count)
                {
                    switch(player.inventoryManager.playerInventory[i].item.itemType)
                    {
                        case ItemType.Default:
                            texts[0].text = "Amount : " + player.inventoryManager.playerInventory[i].amount;
                            break;
                        case ItemType.Crystal:
                            texts[1].text = "Amount : " + player.inventoryManager.playerInventory[i].amount;
                            break;
                        case ItemType.Food:
                            texts[2].text = "Amount : " + player.inventoryManager.playerInventory[i].amount;
                            break;
                        case ItemType.Equipment:
                            texts[3].text = "Amount : " + player.inventoryManager.playerInventory[i].amount;
                            break;
                    }
                }
                else
                {
                    texts[i].text = texts[i].text;
                }
            }
            player.pickedUp = false;
        }
    }
}*/