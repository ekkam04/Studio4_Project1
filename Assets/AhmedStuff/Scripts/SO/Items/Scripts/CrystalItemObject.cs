using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Crystal Item",menuName = "Inventory System/Items/Crystal")]
public class CrystalItemObject : ItemObject
{
    public CrystalType CrystalType;
}

public enum CrystalType
{
    Fire,
    Lightning,
    Ice,
    Earth,
    Holy,
    Nature,
    Air,
    Doom
}
