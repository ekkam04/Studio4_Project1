using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability Item",menuName = "Inventory System/Items/Ability")]
public class AbilityItemObject : ItemObject
{
    public string abilityName;
    public float manaCost;
    public float actionCost;
    public GameObject abilityEffect;
}
