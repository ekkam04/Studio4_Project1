using UnityEngine;

namespace Ekkam
{
    [CreateAssetMenu(fileName = "New Ability", menuName = "Ability", order = 0)]
    public class Ability : ScriptableObject
    {
        [Header("Ability Settings")]
        public string abilityName;
        public Texture2D icon;
        public int intelligenceCost;
        public float damage;
        public string animationToPlay;
        public GameObject vfxPrefab;
        public float vfxForwardOffset;
        public float vfxScale = 1f;
        public float damageDelay;
        public float vfxDelay = 1f;
        
        [Header("Forward Attack Pattern")]
        public bool[] frontLeft = new bool[] {};
        public bool[] frontMiddle = new bool[] {};
        public bool[] frontRight = new bool[] {};
    }
}