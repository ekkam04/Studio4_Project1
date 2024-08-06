using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Ekkam
{
    public class Damagable : MonoBehaviour
    {
        [Header("--- Damagable Settings ---")]
        public Slider healthSlider;
        [Range(0f, 100f)] public float health = 100;
        [Range(0f, 100f)] public float armor = 0;
        [Range(0f, 100f)] public float evasion;
        
        public Texture2D[] coverTextures; // 0 = no cover, 1 = half cover, 2 = full cover
        public RawImage coverImage;
        
        public SkinnedMeshRenderer skinnedMeshRenderer;

        public float CalculateDamage(float accuracy, float damage)
        {
            float damageTaken = 0;
            if (Random.Range(0, 100) < evasion) return 0;
            damageTaken = damage * (1 - armor / 100);
            return damageTaken;
        }
        
        public async void TakeDamage(float damage)
        {
            print(gameObject.name + " took " + damage + " damage");
            health -= damage;
            healthSlider.value = health;
            StartCoroutine(PulseRed());

            await Task.Delay(300);
            if (health <= 0) Eliminate();
        }
        
        private IEnumerator PulseRed()
        {
            skinnedMeshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            skinnedMeshRenderer.material.color = Color.white;
        }
        
        public void Eliminate()
        {
            StopCoroutine(PulseRed());
            
            // To do: Add ragdoll effect
            Destroy(gameObject);
        }
        
        public void Heal(float amount)
        {
            health += amount;
            if (health > 100) health = 100;
            healthSlider.value = health;
        }
    }
}