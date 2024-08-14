using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ekkam;

public class AgentStats : MonoBehaviour
{
    public Damagable damagable;
    public void AddStats(int healthBonus, int armorBounus, int evasionBounus)
    {
        damagable.health += healthBonus;
        damagable.armor += armorBounus;
        damagable.evasion += evasionBounus;
    }

    public void RemoveStats(int healthBonus, int armorBounus, int evasionBounus)
    {
        damagable.health -= healthBonus;
        damagable.armor -= armorBounus;
        damagable.evasion -= evasionBounus;
    }

    public void RestoreHealth(int healthValue)
    {
        damagable.health += healthValue;
    }
}
