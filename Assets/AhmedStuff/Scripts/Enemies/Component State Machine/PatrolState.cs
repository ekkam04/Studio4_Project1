using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PatrolState : State
{
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private float rotationSpeed;
    private int currentWayPoint = 0;
    
    public override void Enter()
    {
        base.Enter();
    }

    public override void Do()
    {
        Vector3 direction = (wayPoints[currentWayPoint].position - enemy.transform.position).normalized;
        if (Vector3.Distance(enemy.transform.position, wayPoints[currentWayPoint].position) < .1f)
        {
            currentWayPoint++;
        }

        if (currentWayPoint >= wayPoints.Length)
        {
            currentWayPoint = 0;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        enemy.transform.rotation =
            Quaternion.Slerp(enemy.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        enemyBody.velocity = direction * _enemySo.speed;
        Debug.Log("Patroling");
    }

    public override void Exit()
    {
        base.Exit();
    }
}
