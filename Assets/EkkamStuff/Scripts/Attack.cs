using UnityEngine;

namespace Ekkam
{
    [CreateAssetMenu(fileName = "New Attack", menuName = "Attack", order = 0)]
    public class Attack : ScriptableObject
    {
        [Header("Attack Settings")]
        public string name;
        public float damage;
        public string animationToPlay;
        
        [Header("Forward Attack Pattern")]
        public bool[] frontLeft = new bool[] {};
        public bool[] frontMiddle = new bool[] {};
        public bool[] frontRight = new bool[] {};
        
        [Header("Mirror")]
        public bool mirrorToRight = true;
        public bool mirrorToLeft = true;
        public bool mirrorToBack = true;
    }
}