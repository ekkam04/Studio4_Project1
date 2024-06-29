using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Default Item Object", menuName = "Inventory System/Items/Default")]
public class DefaultItemObject : ItemObject
{
    private void Awake()
    {
        itemType = ItemType.Default;
    }
}
