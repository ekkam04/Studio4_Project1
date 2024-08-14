using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialItemSelectionUI : MonoBehaviour
{
   private TutorialChest chest;
   public InventoryManager inventoryManager;

   public void AddAssaultRifle()
   {
      chest = FindObjectOfType<TutorialChest>();
      inventoryManager.AddItem(chest.weapons[0]);
      Destroy(chest.gameObject);
   }
   public void AddSniper()
   {
      chest = FindObjectOfType<TutorialChest>();
      inventoryManager.AddItem(chest.weapons[1]);
      Destroy(chest.gameObject);
   }
   public void AddShotgun()
   {
      chest = FindObjectOfType<TutorialChest>();
      inventoryManager.AddItem(chest.weapons[2]);
      Destroy(chest.gameObject);
   }
}