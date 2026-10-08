using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using StarterKit;
using UnityEngine;
using Yudiz.VRAwarenessExperience.Effect;

namespace Yudiz.VRAwarenessExperience.Manager
{
    public enum EffectsType
    {
        KawaseBlur,
        Distortion,
    }

    [System.Serializable]
    public class EffectContainer
    {
        public EffectsType effectsType;
        public EffectBase effect;
    }


    public class EffectsManager : Singleton<EffectsManager>
    {
        private List<EffectContainer> activeEffectContainers = new List<EffectContainer>();


        public void PlayEffect(EffectsType effectsType, EffectData effectData, Action onComplete = null)
        {
            EffectBase effect = null;
            switch (effectsType)
            {
                case EffectsType.KawaseBlur:
                    effect = new KawaseBlurEffect(effectData);
                    break;
                case EffectsType.Distortion:
                    effect = new DistortionEffect(effectData);
                    break;
                default:
                    break;
            }

            if (effect != null)
            {
                effect.StartEffect(onComplete);
                activeEffectContainers.Add(new EffectContainer { effectsType = effectsType, effect = effect });
            }
        }

        public async Task StopEffect(EffectsType effectsType)
        {
            EffectContainer effectContainer = activeEffectContainers.Find(x => x.effectsType == effectsType);
            if (effectContainer != null)
            {
                await effectContainer.effect.StopCurrentEffect();
                activeEffectContainers.Remove(effectContainer);
            }
        }

        public void ForceStopAllEffects()
        {
            foreach (EffectContainer effectContainer in activeEffectContainers)
            {
                effectContainer.effect.ForceStopCurrentEffect();
            }
            activeEffectContainers.Clear();
        }

    }
}

