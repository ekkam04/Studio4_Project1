using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialChest : MonoBehaviour, IInteractable
{
    public ItemObject[] possibleItems;
    public GameObject selectionUIPrefab;
    private GameObject selectionUIInstance;

    public void Interact()
    {
        if (selectionUIInstance == null)
        {
            ShowItemSelectionUI();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void ShowItemSelectionUI()
    {
        
        selectionUIInstance = Instantiate(selectionUIPrefab, transform.position, Quaternion.identity);
        
        TutorialItemSelectionUI selectionUI = selectionUIInstance.GetComponent<TutorialItemSelectionUI>();
        selectionUI.Initialize(possibleItems, OnItemSelected);
    }

    private void OnItemSelected(ItemObject selectedItem)
    {
        
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        inventoryManager.AddItem(selectedItem);
        
        Destroy(selectionUIInstance);
    }
}

