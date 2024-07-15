using UnityEngine;

namespace Ekkam
{
    public class Damagable : MonoBehaviour
    {
        [Header("--- Damagable Settings ---")]
        public float health = 100;
        public float armor;
        public float evasion;
        
        public void TakeDamage(float damage)
        {
            health -= damage;
            if (health <= 0) Die();
        }
        
        public void Die()
        {
            Destroy(gameObject);
        }
        
        public void Heal(float amount)
        {
            health += amount;
        }
    }
}