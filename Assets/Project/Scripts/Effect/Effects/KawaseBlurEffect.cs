using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yudiz.VRAwarenessExperience.Manager;


namespace Yudiz.VRAwarenessExperience.Effect
{
    [Serializable]
    public class KawaseBlurEffect : EffectBase
    {
        private KawaseBlurEffectData kawaseBlurEffectData;

        public KawaseBlurEffect(EffectData effectData) : base(effectData)
        {
            kawaseBlurEffectData = (KawaseBlurEffectData)effectData;
        }

        protected override void OnEffectStarted()
        {
            base.OnEffectStarted();
            RenderPipelineManager.instance.UpdateRenderFeature(RenderFeatureName.KawaseBlur, true);
            PostProcessManager.instance.ManageKawaseBlur(true);
        }

        protected override void PlayEffectTween(float t)
        {
            int iterations = Mathf.RoundToInt(Mathf.Lerp(kawaseBlurEffectData.fromKawaseBlurData.iterations, kawaseBlurEffectData.toKawaseBlurData.iterations, t));
            int downsample = Mathf.RoundToInt(Mathf.Lerp(kawaseBlurEffectData.fromKawaseBlurData.downsample, kawaseBlurEffectData.toKawaseBlurData.downsample, t));
            float offset = Mathf.Lerp(kawaseBlurEffectData.fromKawaseBlurData.offset, kawaseBlurEffectData.toKawaseBlurData.offset, t);
            PostProcessManager.instance.SetKawaseBlur(iterations, downsample, offset);
        }

        protected override void StopEffectTween(float t)
        {
            int iterations = Mathf.RoundToInt(Mathf.Lerp(kawaseBlurEffectData.toKawaseBlurData.iterations, kawaseBlurEffectData.fromKawaseBlurData.iterations, t));
            int downsample = Mathf.RoundToInt(Mathf.Lerp(kawaseBlurEffectData.toKawaseBlurData.downsample, kawaseBlurEffectData.fromKawaseBlurData.downsample, t));
            float offset = Mathf.Lerp(kawaseBlurEffectData.toKawaseBlurData.offset, kawaseBlurEffectData.fromKawaseBlurData.offset, t);
            PostProcessManager.instance.SetKawaseBlur(iterations, downsample, offset);
        }

        protected override void OnEffectCompleted()
        {
            Debug.Log("KawaseBlurEffect Completed!");
            RenderPipelineManager.instance.UpdateRenderFeature(RenderFeatureName.KawaseBlur, false);
            PostProcessManager.instance.ManageKawaseBlur(false);
            base.OnEffectCompleted();
            kawaseBlurEffectData = null;
        }

        protected override void OnEffectForceCompleted()
        {
            PostProcessManager.instance.SetKawaseBlur(kawaseBlurEffectData.fromKawaseBlurData.iterations, kawaseBlurEffectData.fromKawaseBlurData.downsample, kawaseBlurEffectData.fromKawaseBlurData.offset);
            RenderPipelineManager.instance.UpdateRenderFeature(RenderFeatureName.KawaseBlur, false);
            PostProcessManager.instance.ManageKawaseBlur(false);
            base.OnEffectForceCompleted();
            kawaseBlurEffectData = null;
        }
    }
}

