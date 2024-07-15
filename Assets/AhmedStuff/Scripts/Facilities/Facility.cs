using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum FacilityType
{
    BlackSmith,
    Apothecary,
    CultistTower,
    Farmer,
    Builder,
    HexGate
}
public abstract class Facility : MonoBehaviour
{
    public FacilityType facilityType;
    public ItemType itemType;
    public InventoryObject playerInventory;

    public abstract void CheckForReward();

    protected void GiveReward()
    {
        Debug.Log("Item has been upgraded");
    }
    
}
