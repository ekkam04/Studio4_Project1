using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttackState : State
{
    private bool isFiring;
    [SerializeField] private Transform firePoint;
    public override void Do()
    {
        enemy.transform.LookAt(player);
        if (!isFiring)
        {
            StartCoroutine(FireProjectile());
        }
    }

    IEnumerator FireProjectile()
    {

        isFiring = true;
        GameObject projectile = Instantiate(_enemySo.projectile, firePoint.position, Quaternion.identity);
        Rigidbody ProjectileRb = projectile.GetComponent<Rigidbody>();

        if (ProjectileRb != null)
        {
            Vector3 direction = (player.position - projectile.transform.position).normalized;
            ProjectileRb.velocity = direction * _enemySo.projectileSpeed;
        }

        yield return new WaitForSeconds(_enemySo.rateOfFire);
        isFiring = false;

    }
}
