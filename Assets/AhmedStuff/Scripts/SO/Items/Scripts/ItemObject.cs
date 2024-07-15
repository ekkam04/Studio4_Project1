using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemType
{
   Ore,
   Herb,
   Crystal,
   Wheat
}
public class ItemObject : ScriptableObject
{
    public GameObject prefab;
    public ItemType itemType;
    public bool stackble;
    [TextArea(10, 10)] public string description;
}
