using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    private State state;
    public EnemySO _enemySo;
    public State patrolState;
    public State rangedAttack;
    public State ChaseState;
    [SerializeField] Rigidbody rb;
    
    [SerializeField] private Transform player;

    private void Start()
    {
        patrolState.SetUp(this,rb,_enemySo,player);
        rangedAttack.SetUp(this,rb,_enemySo,player);
    }

    private void Update()
    {
        SelectState();
    }

    void SelectState()
    {
        float playerDistance = Vector3.Distance(transform.position, player.position);
        if (playerDistance < 12)
        {
            state = rangedAttack;
        }
        else
        {
            state = patrolState;
        }
        state.Do();
    }

}
