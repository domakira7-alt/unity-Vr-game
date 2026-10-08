using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BlurRenderFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader blurShader;
    private BlurRenderPass blurRenderPass;

    public override void Create()
    {
        blurRenderPass = new BlurRenderPass();
        blurRenderPass.blurShader = blurShader;
        name = "Blur";
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
       if(blurRenderPass.Setup(renderer))
       {
            renderer.EnqueuePass(blurRenderPass);
       }
    }
}
