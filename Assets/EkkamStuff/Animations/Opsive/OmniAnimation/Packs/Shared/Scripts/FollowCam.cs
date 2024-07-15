/// ---------------------------------------------
/// Omni Animation Packs
/// Copyright (c) Opsive. All Rights Reserved.
/// https://omnianimation.ai
/// ---------------------------------------------

namespace Opsive.OmniAnimation.Packs.Shared
{
    using UnityEngine;

    /// <summary>
    /// Moves the camera with the target.
    /// </summary>
    public class FollowCam : MonoBehaviour
    {
        [Tooltip("The target to follow.")]
        [SerializeField] protected Transform m_Target;
        [Tooltip("The offset of the camera relative to the character.")]
        [SerializeField] protected Vector3 m_Offset = new Vector3(-1, 1, -1.2f);
#if !ENABLE_INPUT_SYSTEM
        [Tooltip("The speed to rotate the camera when the right mouse button is pressed.")]
        [SerializeField] protected float m_RotationSpeed = 20;
        [Tooltip("The maximum pitch angle (in degrees).")]
        [SerializeField] protected float m_MaxPitch;
#endif

        public Transform Target { set { m_Target = value; Initialize(); } }

        private Transform m_Transform;
        private float m_StartPitch;
        private float m_Pitch;
        private float m_Yaw;

#if !ENABLE_INPUT_SYSTEM
        private Vector3 m_MousePosition;
#endif

        /// <summary>
        /// Cache the component references.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;

            Initialize();
        }

#if !ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Starts the component.
        /// </summary>
        private void Start()
        {
            m_MousePosition = Input.mousePosition;
        }
#endif

        /// <summary>
        /// Initializes the target.
        /// </summary>
        private void Initialize()
        {
            enabled = m_Target != null;

            if (!enabled || m_Transform == null) {
                return;
            }

            m_StartPitch = m_Pitch = m_Transform.eulerAngles.x;
            m_Yaw = m_Transform.eulerAngles.y;
        }

        /// <summary>
        /// Updates the camera position.
        /// </summary>
        private void LateUpdate()
        {
#if !ENABLE_INPUT_SYSTEM
            if (m_RotationSpeed > 0 && Input.GetMouseButton(1)) {
                var deltaMovement = m_MousePosition - Input.mousePosition;
                if (m_MaxPitch > m_StartPitch) {
                    m_Pitch = Mathf.Clamp(m_Pitch + deltaMovement.y * m_RotationSpeed * Time.deltaTime, m_StartPitch, m_MaxPitch);
                }
                m_Yaw = m_Yaw + deltaMovement.x * m_RotationSpeed * Time.deltaTime;
            }
            m_MousePosition = Input.mousePosition;
#endif
            var rotation = Quaternion.Euler(m_Pitch, m_Yaw, 0);
            // The camera will have an offset. Don't get too close to the offset since it's not directly over the target.
            var amount = 1 - (m_Pitch / 90f);
            var offset = new Vector3(m_Offset.x * amount, m_Offset.y, m_Offset.z * amount);
            var position = m_Target.position + TransformDirection(offset, Quaternion.Euler(0, m_Yaw, 0)) + rotation * Vector3.forward * offset.z;
            m_Transform.SetPositionAndRotation(position, rotation);
        }

        /// <summary>
        /// Transforms the direction from local space to world space. This is similar to Transform.TransformDirection but does not require a Transform.
        /// </summary>
        /// <param name="direction">The direction to transform from local space to world space.</param>
        /// <param name="rotation">The world rotation of the object.</param>
        /// <returns>The world space direction.</returns>
        public static Vector3 TransformDirection(Vector3 direction, Quaternion rotation)
        {
            return rotation * direction;
        }
    }
}