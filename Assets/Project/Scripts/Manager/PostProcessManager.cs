using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using StarterKit;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Yudiz.VRAwarenessExperience.Manager
{
    public class PostProcessManager : Singleton<PostProcessManager>
    {
           [SerializeField] private Volume volume;

            public async void SetMotionBlur(bool isActive, float intensity, float duration)
            {
                Debug.Log("PostProcessManager -> SetMotionBlur -> Called!");
                MotionBlur motionBlur = volume.profile.components.Find(x => x.GetType() == typeof(MotionBlur)) as MotionBlur;
                if (motionBlur != null)
                {
                    motionBlur.active = isActive;
                    if (isActive)
                        await SetMotionBlurIntensityAsync(motionBlur, intensity, duration);
                    else
                        await SetMotionBlurIntensityAsync(motionBlur, 0, duration);
                }
            }

            private async Task SetMotionBlurIntensityAsync(MotionBlur motionBlur, float intensity, float duration)
            {
                float startIntensity = motionBlur.intensity.value;
                float time = 0;
                while (time < duration)
                {
                    motionBlur.intensity.value = Mathf.Lerp(startIntensity, intensity, time / duration);
                    time += Time.deltaTime;
                    await Task.Yield();
                }
                motionBlur.intensity.value = intensity;
                Debug.Log("PostProcessManager -> SetMotionBlurIntensityAsync -> Value Set Completed!");
            }

            public void ManageKawaseBlur(bool isActive)
            {
                KawaseBlurSettings kawaseBlurSettings = volume.profile.components.Find(x => x.GetType() == typeof(KawaseBlurSettings)) as KawaseBlurSettings;
                if (kawaseBlurSettings != null)
                {
                    kawaseBlurSettings.active = isActive;
                }
            }

            public void SetKawaseBlur(int iterations, int downsample, float offset)
            {
                KawaseBlurSettings kawaseBlurSettings = volume.profile.components.Find(x => x.GetType() == typeof(KawaseBlurSettings)) as KawaseBlurSettings;
                if (kawaseBlurSettings != null)
                {
                    kawaseBlurSettings.iterations.value = iterations;
                    kawaseBlurSettings.downsample.value = downsample;
                    kawaseBlurSettings.offset.value = offset;
                }
            }
    }
}
