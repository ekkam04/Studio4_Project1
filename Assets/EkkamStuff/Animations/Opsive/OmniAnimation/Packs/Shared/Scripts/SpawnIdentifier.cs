/// ---------------------------------------------
/// Omni Animation Packs
/// Copyright (c) Opsive. All Rights Reserved.
/// https://omnianimation.ai
/// ---------------------------------------------

namespace Opsive.OmniAnimation.Packs.Shared
{
    using UnityEngine;

    /// <summary>
    /// identifying component for a spawn parent.
    /// </summary>
    public class SpawnIdentifier : MonoBehaviour
    {
        [Tooltip("The unique ID of the spawn parent.")]
        [SerializeField] protected int m_ID;

        public int ID => m_ID;
    }
}