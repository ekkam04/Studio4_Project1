using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    public bool isComplete { get; protected set; }

    protected EnemyBehavior enemy;
    protected Transform player;
    protected Rigidbody enemyBody;
    protected EnemySO _enemySo;
    

    public virtual void Enter(){}
    public virtual void Do(){}
    public virtual void Exit(){}

    public void SetUp(EnemyBehavior enemy, Rigidbody enemyBody,EnemySO enemySo, Transform player)
    {
        this.enemy = enemy;
        this.enemyBody = enemyBody;
        _enemySo = enemySo;
        this.player = player;
    }
}
