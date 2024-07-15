/// ---------------------------------------------
/// Omni Animation Packs
/// Copyright (c) Opsive. All Rights Reserved.
/// https://omnianimation.ai
/// ---------------------------------------------

namespace Opsive.OmniAnimation.Packs.Shared
{
    using UnityEngine;
    using UnityEngine.Events;
    
    /// <summary>
    /// Extends the UnityEvent for a three parameter event.
    /// </summary>
    public class AnimationChangeEvent : UnityEvent<GameObject, GameObject, AnimationClip>
    {
    }
}