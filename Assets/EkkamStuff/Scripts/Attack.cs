using UnityEngine;

namespace Ekkam
{
    [CreateAssetMenu(fileName = "New Attack", menuName = "Attack", order = 0)]
    public class Attack : ScriptableObject
    {
        [Header("Attack Settings")]
        public string attackName;
        public float damage;
        public string animationToPlay;
        public GameObject vfxPrefab;
        public float damageDelay;
        public float vfxDelay = 1f;
        
        [Header("Forward Attack Pattern")]
        public bool[] frontLeft = new bool[] {};
        public bool[] frontMiddle = new bool[] {};
        public bool[] frontRight = new bool[] {};
    }
}