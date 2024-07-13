/// ---------------------------------------------
/// Omni Animation Packs
/// Copyright (c) Opsive. All Rights Reserved.
/// https://omnianimation.ai
/// ---------------------------------------------

namespace Opsive.OmniAnimation.Packs.Shared
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the individual animation list and playback.
    /// </summary>
    public class IndividualAnimationDisplay : MonoBehaviour
    {
        [Tooltip("A reference to the PackInfo ScriptableObject.")]
        [SerializeField] protected PackInfo m_PackInfo;
        [Tooltip("The prefab that represents each animation within the scroll view.")]
        [SerializeField] protected GameObject m_ScrollViewElementPrefab;
        [Tooltip("Specifies if the animations should be automatically scrolled.")]
        [SerializeField] protected bool m_AutoScroll = true;

        [Header("UI")]
        [Tooltip("The panel that contains the individual animation display elements.")]
        [SerializeField] protected GameObject m_UIParent;
        [Tooltip("A reference to the title text.")]
        [SerializeField] protected TMPro.TMP_Text m_TitleText;
        [Tooltip("A reference to the ScrollView content.")]
        [SerializeField] protected ScrollRect m_ScrollRect;
        [Tooltip("A reference to the animation name text.")]
        [SerializeField] protected TMPro.TMP_Text m_AnimationText;
        [Tooltip("The autoscroll toggle.")]
        [SerializeField] protected Toggle m_AutoScrollToggle;

        [Tooltip("The character that will be playing the animations.")]
        protected GameObject m_Character;
        [Tooltip("Specifies the character item (can be null).")]
        protected GameObject m_Item;

        public GameObject Item { get => m_Item; set => m_Item = value; }
        public PackInfo PackInfo => m_PackInfo;
        public ScrollRect ScrollRect => m_ScrollRect;

        private Transform m_CharacterTransform;
        private Animator m_Animator;
        private AnimatorOverrideController m_OverrideController;
        private Vector3 m_CharacterPosition;
        private Quaternion m_CharacterRotation;

        private Element[] m_Elements;
        private int m_Index;
        private int m_DeselectedIndex = -1;
        private float m_ElementHeight;
        private Coroutine m_AutoScrollCoroutine;
        private float m_StartTime;
        private float m_ClipLength;
        private Coroutine m_EventCoroutine;

        private AnimationChangeEvent m_OnAnimationChangeEvent = new AnimationChangeEvent();
        public AnimationChangeEvent OnAnimationChangeEvent { get => m_OnAnimationChangeEvent; set => m_OnAnimationChangeEvent = value; }

        private static int s_ClipHash = Animator.StringToHash("Clip");

        /// <summary>
        /// Initialize the object.
        /// </summary>
        private void Start()
        {
            var spawnedObjects = GetComponent<Spawner>().SpawnObjects();
            m_Character = spawnedObjects.Item1;
            m_Item = spawnedObjects.Item2;

            var followCam = FindObjectOfType<FollowCam>();
            followCam.Target = m_Character.transform;

            m_TitleText.text = m_PackInfo.PackName;
            m_AutoScrollToggle.SetIsOnWithoutNotify(m_AutoScroll);

            m_Elements = new Element[m_PackInfo.Animations.Length];
            for (int i = 0; i < m_PackInfo.Animations.Length; ++i) {
                var instance = Object.Instantiate(m_ScrollViewElementPrefab);
                m_Elements[i] = instance.GetComponent<Element>();
                m_Elements[i].Initialize(this, i);
            }
            m_ElementHeight = m_ScrollViewElementPrefab.GetComponent<RectTransform>().sizeDelta.y;
            m_ScrollRect.content.sizeDelta = new Vector2(m_ScrollRect.content.sizeDelta.x, m_PackInfo.Animations.Length * m_ElementHeight);

            // AutoScroll should be disabled if the scrollbar is pressed.
            m_ScrollRect.verticalScrollbar.onValueChanged.AddListener((float v) =>
            {
                if (EventSystem.current.currentSelectedGameObject == m_ScrollRect.verticalScrollbar.gameObject) {
                    UpdateAutoScroll(false);
                }
            });

            m_CharacterTransform = m_Character.transform;
            m_CharacterPosition = m_CharacterTransform.position;
            m_CharacterRotation = m_CharacterTransform.rotation;

            m_Animator = m_Character.GetComponent<Animator>();
            m_OverrideController = new AnimatorOverrideController(m_Animator.runtimeAnimatorController);
            m_Animator.runtimeAnimatorController = m_OverrideController;

            m_Elements[0].ProceduralSelect();
        }

        /// <summary>
        /// Plays the animation at the specified index.
        /// </summary>
        /// <param name="index">The index of the animation.</param>
        /// <param name="manualSelection">Is the animation played because of a button press?</param>
        public void PlayAnimation(int index, bool manualSelection = false)
        {
            if (m_EventCoroutine != null) {
                StopCoroutine(m_EventCoroutine);
                m_EventCoroutine = null;
            }

            m_Index = index;
            m_DeselectedIndex = -1;
            m_AnimationText.text = m_PackInfo.Animations[index].Name;
            m_OverrideController["Clip"] = m_PackInfo.Animations[index].Clip;
            m_Animator.Play(s_ClipHash, 0, 0);
            m_CharacterTransform.SetPositionAndRotation(m_CharacterPosition, m_CharacterRotation);
            m_ClipLength = m_PackInfo.Animations[index].Clip.length;
            m_StartTime = Time.time;

            // An item action may need to be taken.
            if (m_Item != null) {
                m_Item.SetActive(true);
                if (m_PackInfo.Animations[index].ItemAction != PackInfo.ItemAction.None) {
                    m_EventCoroutine = StartCoroutine("ActivateItem",
                                        new object[] { 
                                            m_PackInfo.Animations[index].ItemAction == PackInfo.ItemAction.Equip, 
                                            m_PackInfo.Animations[index].ItemActionDuration
                                        });
                }
            }

            // The animation should always be visible within the scrollview.
            var targetLocation = index * m_ElementHeight;
            if (targetLocation < m_ScrollRect.content.localPosition.y) {
                m_ScrollRect.content.localPosition = new Vector3(0, targetLocation, 0);
            } else if (targetLocation >= m_ScrollRect.content.localPosition.y + m_ScrollRect.viewport.rect.height) {
                m_ScrollRect.content.localPosition = new Vector3(0, targetLocation - m_ScrollRect.viewport.rect.height + m_ElementHeight, 0);
            }

            if (m_OnAnimationChangeEvent != null) {
                m_OnAnimationChangeEvent.Invoke(m_Character, m_Item, m_PackInfo.Animations[index].Clip);
            }

            if (m_AutoScroll) {
                if (manualSelection) {
                    // Disable autoscroll and let the user continue to select the animations.
                    UpdateAutoScroll(false);
                } else {
                    m_AutoScrollCoroutine = StartCoroutine("NextAnimation");
                }
            }
        }

        /// <summary>
        /// Activates or deactivates the item.
        /// </summary>
        /// <param name="data">The parameter values. Element 0 is if the item should be active, element 1 is the duration.</param>
        private System.Collections.IEnumerator ActivateItem(object[] data)
        {
            m_Item.SetActive(!(bool)data[0]);
            
            yield return new WaitForSeconds((float)data[1]);

            m_Item.SetActive((bool)data[0]);
        }

        /// <summary>
        /// Plays the next animation after the current animation has completed.
        /// </summary>
        private System.Collections.IEnumerator NextAnimation()
        {
            yield return new WaitForSeconds(m_ClipLength - (Time.time - m_StartTime));

            var index = (m_Index + 1) % m_PackInfo.Animations.Length;
            m_Elements[index].ProceduralSelect();
        }

#if !ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Rotates the character with the input manager.
        /// </summary>
        public void Update()
        {
            // Play the same animation again if it is already selected.
            if (Input.GetMouseButtonDown(0)) {
                var elementRectTransform = m_Elements[m_Index].GetComponent<RectTransform>();
                var localMousePosition = elementRectTransform.InverseTransformPoint(Input.mousePosition);
                if (elementRectTransform.rect.Contains(localMousePosition) && m_StartTime != Time.time) {
                    PlayAnimation(m_Index, true);
                }
            } else if (Input.GetKeyDown(KeyCode.Space)) {
                PlayAnimation(m_Index, true);
            }
        }
#endif

        /// <summary>
        /// Updates if the animations should be automatically scrolled.
        /// </summary>
        /// <param name="autoScroll">True if the animations should be automatically scrolled.</param>
        public void UpdateAutoScroll(bool autoScroll)
        {
            m_AutoScroll = autoScroll;
            if (m_AutoScroll) {
                m_AutoScrollCoroutine = StartCoroutine("NextAnimation");
            } else {
                StopCoroutine(m_AutoScrollCoroutine);
            }
            m_AutoScrollToggle.SetIsOnWithoutNotify(m_AutoScroll);
        }

        /// <summary>
        /// The specified element has been deselected.
        /// </summary>
        /// <param name="element">The element that was deselected.</param>
        public void Deselect(Element element)
        {
            if (m_Elements[m_Index] == element) {
                m_DeselectedIndex = m_Index;
            }
        }

        /// <summary>
        /// An element may have been deselected with no elements active. Ensure at least one element is active.
        /// </summary>
        public void LateUpdate()
        {
            if (m_DeselectedIndex != -1) {
                m_Elements[m_DeselectedIndex].ProceduralSelect(false);
                m_DeselectedIndex = -1;
            }
        }
    }
}