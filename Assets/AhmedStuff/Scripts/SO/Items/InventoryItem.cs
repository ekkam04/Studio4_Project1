using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class InventoryItem : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler,IPointerMoveHandler,IPointerClickHandler
{
    
    public Image image;
    public TextMeshProUGUI countText;
    
    [HideInInspector] public ItemObject item;
    [HideInInspector] public int count = 1;
    [HideInInspector] public Transform parentAfterDrag;

    public ItemDescriptionUI itemDescriptionUI;
    private InventoryManager inventoryManager;

    private void Start()
    {
        itemDescriptionUI = FindObjectOfType<ItemDescriptionUI>().GetComponent<ItemDescriptionUI>();
        inventoryManager = FindObjectOfType<InventoryManager>();
    }

    public void InitializeItem(ItemObject item)
    {
        this.item = item;
        image.sprite = item.sprite;
        RefreshCount();
    }

    public void RefreshCount()
    {
        countText.text = count.ToString();
        bool textActive = count > 1;
        countText.gameObject.SetActive(textActive);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
        countText.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        //transform.SetAsFirstSibling();
        image.raycastTarget = true;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        itemDescriptionUI.ShowDescription(item.description);
        Vector3 position = Input.mousePosition + new Vector3(-80,80,0);
        itemDescriptionUI.UpdatePosition(position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        itemDescriptionUI.HideDescription();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        Vector3 position = Input.mousePosition + new Vector3(-80,80,0);
        itemDescriptionUI.UpdatePosition(position);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (item.itemType == ItemType.Food)
            {
                UseItem();
            }
        }
    }

    private void UseItem()
    {
        
        Debug.Log("Using item: " + item.name);
        count--;
        if (count <= 0)
        {
            itemDescriptionUI.HideDescription();
            Destroy(gameObject);
        }
        else
        {
            RefreshCount();
        }
        
        inventoryManager.ApplyItemEffects(item);
    }
}
