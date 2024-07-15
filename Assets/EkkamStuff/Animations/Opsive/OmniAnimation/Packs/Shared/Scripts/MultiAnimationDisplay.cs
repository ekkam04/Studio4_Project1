/// ---------------------------------------------
/// Omni Animation Packs
/// Copyright (c) Opsive. All Rights Reserved.
/// https://omnianimation.ai
/// ---------------------------------------------

namespace Opsive.OmniAnimation.Packs.Shared
{
    using UnityEngine;

    /// <summary>
    /// Spawns all of the animations in a grid of characters.
    /// </summary>
    public class MultiAnimationDisplay : MonoBehaviour
    {
        [Tooltip("A reference to the PackInfo ScriptableObject.")]
        [SerializeField] protected PackInfo m_PackInfo;
        [Tooltip("The number of characters per row.")]
        [SerializeField] protected int m_CharactersPerRow = 20;
        [Tooltip("The spacing between characters.")]
        [SerializeField] protected int m_CharacterSpacing = 4;

        [Header("UI")]
        [Tooltip("The UI canvas.")]
        [SerializeField] protected GameObject m_Canvas;

        [SerializeField] protected AnimationChangeEvent m_OnAnimationChangeEvent = new AnimationChangeEvent();
        public AnimationChangeEvent OnAnimationChangeEvent { get => m_OnAnimationChangeEvent; set => m_OnAnimationChangeEvent = value; }

        private static int s_ClipHash = Animator.StringToHash("LoopingClip");

        /// <summary>
        /// Spawns the characters.
        /// </summary>
        public void Start()
        {
            m_Canvas.SetActive(false);

            var parent = new GameObject("Animations").transform;

            for (int i = 0; i < m_PackInfo.Animations.Length; ++i) {
                var spawnedObjects = GetComponent<Spawner>().SpawnObjects();
                var character = spawnedObjects.Item1;
                character.name = m_PackInfo.Animations[i].Clip.name;
                character.transform.position = new Vector3((i % m_CharactersPerRow) * m_CharacterSpacing, 0, (i / m_CharactersPerRow) * m_CharacterSpacing);
                character.transform.parent = parent;

                var animator = character.GetComponent<Animator>();
                var overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
                animator.runtimeAnimatorController = overrideController;

                var motionController = character.GetComponent<MotionController>();
                motionController.UseRootMotionPosition = false;

                if (m_OnAnimationChangeEvent != null) {
                    m_OnAnimationChangeEvent.Invoke(character, spawnedObjects.Item2, m_PackInfo.Animations[i].Clip);
                }

                overrideController["Clip"] = m_PackInfo.Animations[i].Clip;
                animator.Play(s_ClipHash, 0, 0);
            }
        }
    }
}