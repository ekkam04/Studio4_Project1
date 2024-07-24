using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemType
{
   Defualt,
   Food,
   Crystal,
   Equipment
}
public class ItemObject : ScriptableObject
{
    public GameObject prefab;
    public ItemType itemType;
    public string itemKey;
    public bool stackble;
    [TextArea(10, 10)] public string description;
}
