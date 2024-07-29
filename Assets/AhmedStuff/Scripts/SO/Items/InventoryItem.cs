using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class InventoryItem : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler
{
    
    public Image image;
    public TextMeshProUGUI countText;
    
    [HideInInspector] public ItemObject item;
    [HideInInspector] public int count = 1;
    [HideInInspector] public Transform parentAfterDrag;

   
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
}
