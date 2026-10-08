using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yudiz.VRAwarenessExperience.Manager;

namespace Yudiz.VRAwarenessExperience.Effect
{
    [Serializable]
    public class DistortionEffect : EffectBase
    {
        private DistortionEffectData distortionEffectData;

        public DistortionEffect(EffectData effectData) : base(effectData)
        {
            distortionEffectData = (DistortionEffectData)effectData;
        }

        protected override void OnEffectStarted()
        {
            base.OnEffectStarted();
            RenderPipelineManager.instance.UpdateFullScreenRenderFeature(RenderFeatureName.FullScreenPassRendererFeature, true, distortionEffectData.effectMaterial);
        }

        protected override void PlayEffectTween(float t)
        {
            Material material = distortionEffectData.effectMaterial;
            material.SetFloat("_Amplitude", Mathf.Lerp(distortionEffectData.fromDistortionData.distortionAmplitude, distortionEffectData.toDistortionData.distortionAmplitude, t));
            material.SetVector("_PanSpeed", Vector2.Lerp(distortionEffectData.fromDistortionData.distortionPanSpeed, distortionEffectData.toDistortionData.distortionPanSpeed, t));
            material.SetFloat("_Blend", Mathf.Lerp(distortionEffectData.fromDistortionData.blend, distortionEffectData.toDistortionData.blend, t));
            material.SetFloat("_NoiseScale", Mathf.Lerp(distortionEffectData.fromDistortionData.noiseScale, distortionEffectData.toDistortionData.noiseScale, t));
            material.SetFloat("_AbbreivationStrenght", Mathf.Lerp(distortionEffectData.fromDistortionData.chromaticAbberationStrenght, distortionEffectData.toDistortionData.chromaticAbberationStrenght, t));
            material.SetVector("_OffsetDir", Vector2.Lerp(distortionEffectData.fromDistortionData.chromaticAbberationDirection, distortionEffectData.toDistortionData.chromaticAbberationDirection, t));
            material.SetFloat("_AbbreivationIntensity", Mathf.Lerp(distortionEffectData.fromDistortionData.chromaticAbberationIntensity, distortionEffectData.toDistortionData.chromaticAbberationIntensity, t));
        }

        protected override void StopEffectTween(float t)
        {
            Material material = distortionEffectData.effectMaterial;
            material.SetFloat("_Amplitude", Mathf.Lerp(distortionEffectData.toDistortionData.distortionAmplitude, distortionEffectData.fromDistortionData.distortionAmplitude, t));
            material.SetVector("_PanSpeed", Vector2.Lerp(distortionEffectData.toDistortionData.distortionPanSpeed, distortionEffectData.fromDistortionData.distortionPanSpeed, t));
            material.SetFloat("_Blend", Mathf.Lerp(distortionEffectData.toDistortionData.blend, distortionEffectData.fromDistortionData.blend, t));
            material.SetFloat("_NoiseScale", Mathf.Lerp(distortionEffectData.toDistortionData.noiseScale, distortionEffectData.fromDistortionData.noiseScale, t));
            material.SetFloat("_AbbreivationStrenght", Mathf.Lerp(distortionEffectData.toDistortionData.chromaticAbberationStrenght, distortionEffectData.fromDistortionData.chromaticAbberationStrenght, t));
            material.SetVector("_OffsetDir", Vector2.Lerp(distortionEffectData.toDistortionData.chromaticAbberationDirection, distortionEffectData.fromDistortionData.chromaticAbberationDirection, t));
            material.SetFloat("_AbbreivationIntensity", Mathf.Lerp(distortionEffectData.toDistortionData.chromaticAbberationIntensity, distortionEffectData.fromDistortionData.chromaticAbberationIntensity, t));
        }

        protected override void OnEffectCompleted()
        {
            RenderPipelineManager.instance.UpdateFullScreenRenderFeature(RenderFeatureName.FullScreenPassRendererFeature, false, null);
            base.OnEffectCompleted();
            distortionEffectData = null;
        }

        protected override void OnEffectForceCompleted()
        {
            ForceResetDistortion(distortionEffectData.effectMaterial);
            RenderPipelineManager.instance.UpdateFullScreenRenderFeature(RenderFeatureName.FullScreenPassRendererFeature, false, null);
            base.OnEffectForceCompleted();
            distortionEffectData = null;
        }

        protected void ForceResetDistortion(Material material)
        {
            material.SetFloat("_Amplitude", distortionEffectData.fromDistortionData.distortionAmplitude);
            material.SetVector("_PanSpeed", distortionEffectData.fromDistortionData.distortionPanSpeed);
            material.SetFloat("_Blend", distortionEffectData.fromDistortionData.blend);
            material.SetFloat("_NoiseScale", distortionEffectData.fromDistortionData.noiseScale);
            material.SetFloat("_AbbreivationStrenght", distortionEffectData.fromDistortionData.chromaticAbberationStrenght);
            material.SetVector("_OffsetDir", distortionEffectData.fromDistortionData.chromaticAbberationDirection);
            material.SetFloat("_AbbreivationIntensity", distortionEffectData.fromDistortionData.chromaticAbberationIntensity);
        }
    }
}
