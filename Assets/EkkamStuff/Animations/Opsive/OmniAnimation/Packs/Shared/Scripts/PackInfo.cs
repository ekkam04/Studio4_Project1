/// ---------------------------------------------
/// Omni Animation Packs
/// Copyright (c) Opsive. All Rights Reserved.
/// https://omnianimation.ai
/// ---------------------------------------------

namespace Opsive.OmniAnimation.Packs.Shared
{
    using System;
    using UnityEngine;

    /// <summary>
    /// A ScriptableObject containing the information about the animation pack.
    /// </summary>
    public class PackInfo : ScriptableObject
    {
        /// <summary>
        /// Specifies the type of action that should be taken on the item.
        /// </summary>
        public enum ItemAction
        {
            None,
            Equip,
            Unequip
        }

        /// <summary>
        /// Information about the animation clip.
        /// </summary>
        [Serializable]
        public struct AnimationInfo
        {
            [Tooltip("A reference to the animation clip.")]
            public AnimationClip Clip;
            [Tooltip("The display friendly name of the clip.")]
            public string Name;
            [Tooltip("Specifies the item action that should be taken.")]
            public ItemAction ItemAction;
            [Tooltip("If an ItemAction is specified, specifies the duration until that action occurs.")]
            public float ItemActionDuration;
        }

        [Tooltip("The name of the asset pack.")]
        [SerializeField] protected string m_PackName;
        [Tooltip("The version of the asset pack.")]
        [SerializeField] protected string m_Version;
        [Tooltip("A reference to the animations contained within the pack.")]
        [SerializeField] protected AnimationInfo[] m_Animations;

        public string PackName { get => m_PackName; }
        public string Version { get => m_Version; }
        public AnimationInfo[] Animations { get => m_Animations; set => m_Animations = value; }
    }
}