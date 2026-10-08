using System.Collections;
using System.Collections.Generic;
using StarterKit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Yudiz.VRAwarenessExperience.Manager
{
    public class RenderPipelineManager : StarterKit.Singleton<RenderPipelineManager>
    {
        [SerializeField] private UniversalRendererData rendererData;

        public void UpdateFullScreenRenderFeature(RenderFeatureName renderFeatureName, bool isActive, Material material = null)
        {
            ScriptableRendererFeature renderFeature = rendererData.rendererFeatures.Find(feature => feature.name == renderFeatureName.ToString());
            if (renderFeature != null)
            {
                if (material != null)
                {
                    FullScreenPassRendererFeature fullScreenPassRendererFeature = renderFeature as FullScreenPassRendererFeature;
                    if (fullScreenPassRendererFeature != null)
                    {
                        fullScreenPassRendererFeature.passMaterial = material;
                    }
                }
                
                renderFeature.SetActive(isActive);
                rendererData.SetDirty();
            }

            
            else Debug.Log("<color=red>RenderPipelineManager -> UpdateRenderFeature -> Render Feature not found</color>");
        }

        public void UpdateRenderFeature(RenderFeatureName renderFeatureName, bool isActive)
        {
            ScriptableRendererFeature renderFeature = rendererData.rendererFeatures.Find(feature => feature.name == renderFeatureName.ToString());
            if (renderFeature != null)
            {
                renderFeature.SetActive(isActive);
                rendererData.SetDirty();
            }
        }

        public void OnDisable()
        {
            UpdateFullScreenRenderFeature(RenderFeatureName.FullScreenPassRendererFeature, false);
        }
    }

    public enum RenderFeatureName
    {
        FullScreenPassRendererFeature,
        KawaseBlur,
    }

}

