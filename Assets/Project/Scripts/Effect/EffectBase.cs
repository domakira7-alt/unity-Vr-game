using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Yudiz.VRAwarenessExperience.Manager;

namespace Yudiz.VRAwarenessExperience.Effect
{
    [Serializable]
    public class EffectBase
    {
        [Header("Effect")]
        [HorizontalLine(color: EColor.Green)]
        protected float effectDuration;
        protected EffectsType effectsType;

        protected EffectData effectData;

        protected Action onEffectStarted = null;

        protected CancellationTokenSource effectCancellationTokenSource;


        public EffectBase(EffectData effectData)
        {
            this.effectData = effectData;
            this.effectDuration = effectData.effectDuration;
        }
        
        public async void StartEffect(Action onComplete = null)
        {
            onEffectStarted = onComplete;
            OnEffectStarted();
            await PlayEffect();
        }

        protected async Task PlayEffect()
        {
            float elapsedTime = 0f;
            while (elapsedTime < effectDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / effectDuration;
                PlayEffectTween(t);
                await Task.Yield();
                if (effectCancellationTokenSource.IsCancellationRequested)
                {
                    OnEffectForceCompleted();
                    break;
                }
            }

            onEffectStarted?.Invoke();
        }

        public async Task StopCurrentEffect()
        {
            await StopEffect();
        }

        public void ForceStopCurrentEffect()
        {
            if (effectCancellationTokenSource != null)
            {
                effectCancellationTokenSource.Cancel();
                effectCancellationTokenSource.Dispose();
                effectCancellationTokenSource = null;
            }
        }

        protected async Task StopEffect()
        {
            float elapsedTime = 0f;
            while (elapsedTime < effectDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / effectDuration;
                StopEffectTween(t);
                await Task.Yield();
                if (effectCancellationTokenSource.IsCancellationRequested)
                {
                    OnEffectForceCompleted();
                    break;
                }
            }
            OnEffectCompleted();
        }

        protected virtual void PlayEffectTween(float t)
        {

        }

        protected virtual void StopEffectTween(float t)
        {

        }
        
        protected virtual void OnEffectStarted()
        {
            effectCancellationTokenSource = new CancellationTokenSource();
        }

        protected virtual void OnEffectCompleted()
        {
            Debug.Log("EffectBase - OnEffectCompleted");
            ResetEffect();
        }

        protected virtual void OnEffectForceCompleted()
        {
            Debug.Log("EffectBase - OnEffectForceCompleted");
            ResetEffect();
        }

        private void ResetEffect()
        {
            Debug.Log("EffectBase - ResetEffect: " + effectsType);
            if (effectCancellationTokenSource != null)
            {
                effectCancellationTokenSource.Dispose();
                effectCancellationTokenSource = null;
            }
            onEffectStarted = null;
            effectData = null;
        }
    }
}

