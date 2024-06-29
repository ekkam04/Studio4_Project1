using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemType
{
    Food,
    Equipment,
    Default
}
public class ItemObject : ScriptableObject
{
    public GameObject prefab;
    public ItemType itemType;
    [TextArea(10, 10)] public string description;
}
