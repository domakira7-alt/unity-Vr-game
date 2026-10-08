using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Yudiz.VRAwarenessExperience.Data;
using Yudiz.VRAwarenessExperience.Manager;

namespace Yudiz.VRAwarenessExperience.Transition
{
    public class PillTransition : TransitionBase
    {
        [Header("PillEffect")]
        [HorizontalLine(color: EColor.Indigo)]
        [SerializeField] private Material pillMaterial;
        private PillTransitionData fromPillTransitionData;
        private PillTransitionData toPillTransitionData;

        protected override void InitTransition(TransitionData fromTransitionData, TransitionData toTransitionData)
        {
            base.InitTransition(fromTransitionData, toTransitionData);
            fromPillTransitionData = (PillTransitionData)fromTransitionData;
            toPillTransitionData = (PillTransitionData)toTransitionData;
        }

        protected override void OnTransitionStarted()
        {
            RenderPipelineManager.instance.UpdateFullScreenRenderFeature(RenderFeatureName.FullScreenPassRendererFeature, true, pillMaterial);
        }

        protected override void PlayTransitionTween(float t)
        {
            pillMaterial.SetVector("_PanSpeed", Vector2.Lerp(fromPillTransitionData.wobbleSpeed, toPillTransitionData.wobbleSpeed, t));
            pillMaterial.SetFloat("_Amplitude", Mathf.Lerp(fromPillTransitionData.wobbleAmplitude, toPillTransitionData.wobbleAmplitude, t));
            pillMaterial.SetFloat("_NoiseScale", Mathf.Lerp(fromPillTransitionData.noiseScale, toPillTransitionData.noiseScale, t));
            pillMaterial.SetFloat("_Blend", Mathf.Lerp(fromPillTransitionData.blend, toPillTransitionData.blend, t));
            pillMaterial.SetColor("_TintColor", Color.Lerp(fromPillTransitionData.pillColor, toPillTransitionData.pillColor, t));
        }

        protected override void StopTransitionTween(float t)
        {
            pillMaterial.SetVector("_PanSpeed", Vector2.Lerp(toPillTransitionData.wobbleSpeed, fromPillTransitionData.wobbleSpeed, t));
            pillMaterial.SetFloat("_Amplitude", Mathf.Lerp(toPillTransitionData.wobbleAmplitude, fromPillTransitionData.wobbleAmplitude, t));
            pillMaterial.SetFloat("_NoiseScale", Mathf.Lerp(toPillTransitionData.noiseScale, fromPillTransitionData.noiseScale, t));
            pillMaterial.SetFloat("_Blend", Mathf.Lerp(toPillTransitionData.blend, fromPillTransitionData.blend, t));
            pillMaterial.SetColor("_TintColor", Color.Lerp(toPillTransitionData.pillColor, fromPillTransitionData.pillColor, t));
        }

        protected override void OnTransitionStopped()
        {
            RenderPipelineManager.instance.UpdateFullScreenRenderFeature(RenderFeatureName.FullScreenPassRendererFeature, false, null);
            fromPillTransitionData = null;
            toPillTransitionData = null;
        }
    }
}
