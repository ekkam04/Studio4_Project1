using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Equipment Item",menuName = "Inventory System/Items/Equipment")]
public class EquipmentItemObject : ItemObject
{
    public EquipmentType equipmentType;
    public ItemObject ability;
    public int attackValue;
    public int armorValue;
    public int speedValue;
    public int evasionValue;
    public int movementPoints;
}

public enum EquipmentType
{
    Weapon,
    Armor,
    Shield
}
