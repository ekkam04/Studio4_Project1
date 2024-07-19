/// ---------------------------------------------
/// Omni Animation Packs
/// Copyright (c) Opsive. All Rights Reserved.
/// https://omnianimation.ai
/// ---------------------------------------------

namespace Opsive.OmniAnimation.Packs.Shared
{
    using UnityEngine;

    /// <summary>
    /// Uses the root motion data to move the object.
    /// </summary>
    public class MotionController : MonoBehaviour
    {
        [Tooltip("Should the object use root motion for the position?")]
        [SerializeField] protected bool m_UseRootMotionPosition = true;
        [Tooltip("Should the object use root motion for the rotation?")]
        [SerializeField] protected bool m_UseRootMotionRotation = true;

        public bool UseRootMotionPosition { get => m_UseRootMotionPosition; set => m_UseRootMotionPosition = value; }
        public bool UseRootMotionRotation { get => m_UseRootMotionRotation; set => m_UseRootMotionRotation = value; }

        private Transform m_Transform;
        private Animator m_Animator;

        private Vector3 m_RootMotionPosition;
        private Quaternion m_RootMotionRotation = Quaternion.identity;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
            m_Animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Moves the character according to the root motion data.
        /// </summary>
        private void Update()
        {
            m_Transform.SetPositionAndRotation(m_Transform.position + m_RootMotionPosition, m_Transform.rotation * m_RootMotionRotation);

            m_RootMotionPosition = Vector3.zero;
            m_RootMotionRotation = Quaternion.identity;
        }

        /// <summary>
        /// The Animator has updated.
        /// </summary>
        public void OnAnimatorMove()
        {
            if (m_UseRootMotionPosition) {
                m_RootMotionPosition += m_Animator.deltaPosition;
            } else {
                // Vertical displacement will always occur.
                m_RootMotionPosition.y += m_Animator.deltaPosition.y;
            }

            if (m_UseRootMotionRotation) {
                m_RootMotionRotation *= m_Animator.deltaRotation;
            }
        }
    }
}