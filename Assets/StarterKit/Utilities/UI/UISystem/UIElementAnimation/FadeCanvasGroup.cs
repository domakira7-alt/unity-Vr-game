using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UISystem
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeCanvasGroup : Animatable
    {
        public float fromAlpha;
        public float toAlpha;
        public float finalAlpha;
        CanvasGroup canvasGroup;
        public override void Awake()
        {
            base.Awake();
            canvasGroup = GetComponent<CanvasGroup>();
        }
        public override void OnAnimationStarted()
        {
            base.OnAnimationStarted();
            canvasGroup.alpha = fromAlpha;
        }
        public override void OnAnimationRunning(float animPerc)
        {
            base.OnAnimationRunning(animPerc);
            canvasGroup.alpha = Mathf.LerpUnclamped(fromAlpha, toAlpha, animPerc);
        }
        public override void OnAnimationEnded()
        {
            base.OnAnimationEnded();
            canvasGroup.alpha = finalAlpha;
        }
    }
}

