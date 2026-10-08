using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NaughtyAttributes;
using StarterKit.Utilities;
using UnityEngine;
using Yudiz.VRAwarenessExperience.Data;
using Yudiz.VRAwarenessExperience.Manager;

namespace Yudiz.VRAwarenessExperience.Transition
{
    [Serializable]
    public class TransitionBase : MonoBehaviour
    {
        [Header("Transition")]
        [HorizontalLine(color: EColor.Green)]
        [SerializeField] protected float transitionDuration;
        [SerializeField] protected TransitionType transitionType;
        private TransitionData fromTransitionData;
        private TransitionData toTransitionData;
        protected CancellationTokenSource transitionCancellationTokenSource;
        protected bool isActive;

        private Action onTransitionCompleted = null;

        public void StartTransition(TransitionData fromTransitionData, TransitionData toTransitionData, Action onComplete = null)
        {
            onTransitionCompleted = onComplete;
            InitTransition(fromTransitionData, toTransitionData);
            PlayTransition();
        }

        protected virtual void InitTransition(TransitionData fromTransitionData, TransitionData toTransitionData) 
        {
            this.fromTransitionData = fromTransitionData;
            this.toTransitionData = toTransitionData;
        }

        protected async virtual void PlayTransition()
        {
            if (isActive) 
            {
                Debug.Log("Effect is already active " + transitionType);
                return;
            }

            SoundManager.instance.PlaySound(SoundType.DizzinessSound);
            isActive = true;
            OnTransitionStarted();
            transitionCancellationTokenSource = new CancellationTokenSource();
            await PlayTransitionAsync(transitionCancellationTokenSource.Token,  () => 
            {
                StopTransition();
            });
        }

        private async Task PlayTransitionAsync(CancellationToken cancellationToken, Action onComplete)
        {
            float elapsedTime = 0f;
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;
                PlayTransitionTween(t);
                await Task.Yield();
                if (cancellationToken.IsCancellationRequested) break;
            }
            onComplete?.Invoke();
        }

        protected virtual void PlayTransitionTween(float t)
        {

        }

        protected virtual void StopTransitionTween(float t)
        {

        }

        public void StopTransition()
        {
            if (!isActive) return;

            isActive = false;
            transitionCancellationTokenSource = new CancellationTokenSource();
            StopTransitionAsync(transitionCancellationTokenSource.Token, () => 
            {
                OnTransitionStopped();
                onTransitionCompleted?.Invoke();
                ResetTransition();
            });
        }

        private async void StopTransitionAsync(CancellationToken cancellationToken, Action onComplete)
        {
            float elapsedTime = 0f;
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;
                StopTransitionTween(t);
                await Task.Yield();
                if (cancellationToken.IsCancellationRequested) break;
            }
            onComplete?.Invoke();
        }

        public virtual void ForceStopTransition()
        {
            if (!isActive) return;
            isActive = false;
            onTransitionCompleted = null;
            if (transitionCancellationTokenSource != null)
            {
                transitionCancellationTokenSource.Cancel();
                transitionCancellationTokenSource.Dispose();
                transitionCancellationTokenSource = null;
            }
        }

        public virtual void ResetTransition()
        {
            isActive = false;
            if (transitionCancellationTokenSource != null)
            {
                transitionCancellationTokenSource.Dispose();
                transitionCancellationTokenSource = null;
            }
        }

        protected virtual void OnTransitionStarted()
        {

        }


        protected virtual void OnTransitionStopped()
        {

        }
    }
}

