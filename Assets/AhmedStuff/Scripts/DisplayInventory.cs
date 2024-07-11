using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayInventory : MonoBehaviour
{
    public List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
    public InventoryObject playerInventory;
    public PlayerPickUpItems player;
    

    private void Update()
    {
        if (player.pickedUp)
        {
            int count = player.playerInventory.container.Count;
            for (int i = 0; i < texts.Count; i++)
            {
                if (i < count)
                {
                    switch(player.playerInventory.container[i].item.itemType)
                    {
                        case ItemType.Ore:
                            texts[0].text = "Amount : " + player.playerInventory.container[i].amount;
                            break;
                        case ItemType.Herb:
                            texts[1].text = "Amount : " + player.playerInventory.container[i].amount;
                            break;
                        case ItemType.Crystal:
                            texts[2].text = "Amount : " + player.playerInventory.container[i].amount;
                            break;
                        case ItemType.Wheat:
                            texts[3].text = "Amount : " + player.playerInventory.container[i].amount;
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
}