using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UISystem
{
    public enum AnimationCycleType
    {
        Once,
        Continuous
    }
    public class Animatable : MonoBehaviour
    {
        public bool shouldAnimateWhenScreenIsShowed = true;
        public UIAnimationType animationType;
        public AnimationCycleType cycleType;
        public AnimationCurve curve;
        [HideInInspector]
        public bool isAnimationRunning = false;
        protected RectTransform rectTransform;
        public float delay = 0;
        public float duration = 0.4f;
        private IEnumerator animationRoutine;
        WaitForSeconds waitForSeconds;
        public List<AnimationEvent> animationEvents;
        public virtual void Awake()
        {
            CacheComponents();
        }
        void CacheComponents()
        {
            waitForSeconds = new WaitForSeconds(delay);
            rectTransform = GetComponent<RectTransform>();
            if (!shouldAnimateWhenScreenIsShowed)
                return;
            if (FindBaseUI(this.gameObject) != null)
            {
                FindBaseUI(this.gameObject).GetComponent<UIAnimator>().RegisterAnimatable(this, animationType);
            }
        }
        public void Validate()
        {
            waitForSeconds = new WaitForSeconds(delay);
        }
        public void StartAnimate()
        {
            if (gameObject.activeInHierarchy)
            {
                animationRoutine = AnimateShow();
                StartCoroutine(animationRoutine);
            }
        }

        public void StopAnimate()
        {
            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
                OnAnimationEnded();
            }
        }
        public IEnumerator AnimateShow()
        {
            float elapsed = 0;
            float perc;
            CheckForAnimationEvent();
            OnAnimationStarted();
            yield return waitForSeconds;
            while (elapsed <= duration)
            {
                perc = elapsed / duration;
                CheckForAnimationEvent();
                OnAnimationRunning(curve.Evaluate(perc));
                elapsed += Time.unscaledDeltaTime;
                if (cycleType == AnimationCycleType.Continuous && elapsed > duration)
                {
                    elapsed = 0;
                }
                yield return null;
            }
            CheckForAnimationEvent();
            OnAnimationEnded();
            yield return null;
        }
        public virtual void OnAnimationStarted()
        {
            isAnimationRunning = true;
        }
        public virtual void OnAnimationEnded()
        {
            isAnimationRunning = false;
        }
        public virtual void OnAnimationRunning(float percentage)
        {
            isAnimationRunning = true;
        }
        public GameObject FindBaseUI(GameObject childObject)
        {
            Transform t = childObject.transform;
            while (t.parent != null)
            {
                if (t.parent.GetComponent<UIAnimator>() != null)
                {
                    return t.parent.gameObject;
                }
                t = t.parent.transform;
            }
            return null;
        }
        int counter = 0;
        public void CheckForAnimationEvent()
        {
            for (counter = 0; counter < animationEvents.Count; counter++)
            {
                animationEvents[counter].Event.Invoke();
            }
        }
    }
}