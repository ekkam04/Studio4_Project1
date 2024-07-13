using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Create Enemy/Creat New Enemy")]
public class EnemySO : ScriptableObject
{
    public float hitPoints;
    public float speed;
    public float projectileSpeed;
    public float rateOfFire;
    public float damage;
    public Animator anim;
    public enum Type
    {
        Fire,
        Ice,
        Poison,
        Electric
    }

    public Type type;

    public GameObject enemyPrefab;
    public GameObject projectile;
    public GameObject enemyDrop;


}
