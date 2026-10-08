using UnityEngine;
using UnityEngine.Rendering.Universal;

public class KawaseBlurRenderFeature : ScriptableRendererFeature
{
    [SerializeField] Shader kawaseShader;

    KawaseBlurRenderPass _pass;

    public override void Create()
    {
        _pass = new KawaseBlurRenderPass();
        _pass.kawaseShader = kawaseShader;
        name = "KawaseBlur";
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_pass != null && _pass.Setup(renderer))
            renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
    }
}
