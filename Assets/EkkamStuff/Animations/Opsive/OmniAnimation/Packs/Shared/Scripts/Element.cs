/// ---------------------------------------------
/// Omni Animation Packs
/// Copyright (c) Opsive. All Rights Reserved.
/// https://omnianimation.ai
/// ---------------------------------------------

namespace Opsive.OmniAnimation.Packs.Shared
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    /// <summary>
    /// A Element is the selectable item within the ScrollRect.
    /// </summary>
    public class Element : Selectable
    {
        [Tooltip("A reference to the element text.")]
        [SerializeField] protected TMPro.TMP_Text m_Text;
        [Tooltip("The background image of the element.")]
        [SerializeField] protected Image m_BackgroundImage;

        private IndividualAnimationDisplay m_AnimationDisplay;
        private int m_Index;
        private bool m_ManualSelection = true;
        private bool m_PlayAnimation = true;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        /// <param name="animationDisplay">A reference to the IndividualAnimationDisplay component.</param>
        /// <param name="index">The index of the animation.</param>
        public void Initialize(IndividualAnimationDisplay animationDisplay, int index)
        {
            m_AnimationDisplay = animationDisplay;
            m_Index = index;

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.position = new Vector3(0, -(index * rectTransform.sizeDelta.y) - rectTransform.sizeDelta.y / 2);
            rectTransform.SetParent(animationDisplay.ScrollRect.content, false);

            m_Text.text = animationDisplay.PackInfo.Animations[index].Name;

            if (index == 0) {
                m_BackgroundImage.color = colors.selectedColor;
            }
        }

        /// <summary>
        /// Selects the element from script.
        /// </summary>
        /// <param name="">Should the animation be played?</param>
        public void ProceduralSelect(bool playAnimation = true)
        {
            m_ManualSelection = false;
            m_PlayAnimation = playAnimation;
            Select();
            m_ManualSelection = true;
            m_PlayAnimation = true;
        }

        /// <summary>
        /// The pointer has entered the element.
        /// </summary>
        /// <param name="eventData">The event that triggered the event.</param>
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            m_BackgroundImage.color = currentSelectionState == SelectionState.Selected ? colors.selectedColor : colors.normalColor;
        }

        /// <summary>
        /// The element has been selected.
        /// </summary>
        /// <param name="eventData">The event that triggered the event.</param>
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            m_BackgroundImage.color = colors.selectedColor;
            if (m_PlayAnimation) {
                m_AnimationDisplay.PlayAnimation(m_Index, m_ManualSelection);
            }
        }

        /// <summary>
        /// The element has been deselected.
        /// </summary>
        /// <param name="eventData">The event that triggered the event.</param>
        public override void OnDeselect(BaseEventData eventData)
        {
            m_AnimationDisplay.Deselect(this);

            base.OnDeselect(eventData);

            m_BackgroundImage.color = colors.normalColor;
        }
    }
}