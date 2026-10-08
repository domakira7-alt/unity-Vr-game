using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Yudiz.VRAwarenessExperience.Manager;

namespace Yudiz.VRAwarenessExperience.Utilities
{
    public static class Utilities 
    {
        public static Color SetPrecisionColor(float value)
        {
            return value switch
            {
                >= 1.0f => Color.green,
                >= 0.5f => Color.yellow,
                >= 0.25f => Color.red,
                _ => Color.green
            };
        }

        public static string FormatTime(float currentTimeInSeconds)
        {
            return string.Format("{0:00}", Mathf.FloorToInt(currentTimeInSeconds));
        }

        public async static Task WaitForSoundCompletion(AudioClip audioClip, Action onComplete)
        {
            float duration = audioClip.length;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }
            onComplete?.Invoke();
        }

        public static async Task LerpDissolveValue(List<Material> materials, float startValue, float endValue, float duration)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float currentValue = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
                
                foreach(Material material in materials)
                {
                    material.SetFloat("_CutoffHeight", currentValue);
                }
                
                await Task.Yield();
            }

            foreach(Material material in materials)
            {
                material.SetFloat("_CutoffHeight", endValue);
            }
        }
    }
}

