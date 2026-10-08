using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurRenderPass : ScriptableRenderPass
{
    private Material material;
    private BlurSettings blurSettings;

    public Shader blurShader;

    RTHandle _tempRT;
    RTHandle _cameraColor;

    static readonly string kTag = "Blur Post Process";

    public bool Setup(ScriptableRenderer renderer)
    {
        blurSettings   = VolumeManager.instance.stack.GetComponent<BlurSettings>();
        renderPassEvent = RenderPassEvent.AfterRendering; // or AfterRenderingPostProcessing

        if (blurSettings == null || !blurSettings.IsActive())
            return false;

        Shader shaderToUse = blurShader != null ? blurShader : Shader.Find("PostProcessing/GaussianBlur");
        if (shaderToUse == null)
        {
            Debug.LogError("BlurRenderPass: shader missing (PostProcessing/GaussianBlur).");
            return false;
        }
        if (material == null || material.shader != shaderToUse)
            material = CoreUtils.CreateEngineMaterial(shaderToUse);

        // Request camera color input for URP to ensure a readable color texture
        ConfigureInput(ScriptableRenderPassInput.Color);
        return true;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        _cameraColor = renderingData.cameraData.renderer.cameraColorTargetHandle;

        // Allocate a non-MSAA temp RT (forces resolve before sampling)
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        desc.msaaSamples = 1; // *** critical on Quest ***
        RenderingUtils.ReAllocateIfNeeded(ref _tempRT, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_BlurTemp");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
{
    if (blurSettings == null || !blurSettings.IsActive() || material == null) return;
    if (renderingData.cameraData.cameraType == CameraType.Preview) return;

    var cmd = CommandBufferPool.Get("Blur Post Process");

    // params
    int gridSize = Mathf.CeilToInt(blurSettings.stenght.value * 6f);
    if ((gridSize & 1) == 0) gridSize++;
    material.SetInt("_GridSize", gridSize);
    material.SetFloat("_Spread", Mathf.Max(0.0001f, blurSettings.stenght.value));

    // 1) cameraColor -> temp   (binds _BlitTexture & resolves MSAA)
    Blitter.BlitCameraTexture(cmd, _cameraColor, _tempRT);

    // 2) Horizontal: temp -> cameraColor   (material pass 0)
    Blitter.BlitCameraTexture(cmd, _tempRT, _cameraColor, material, 0);

    // 3) Rebind cameraColor -> temp (no material), then temp -> cameraColor (pass 1)
    Blitter.BlitCameraTexture(cmd, _cameraColor, _tempRT);
    Blitter.BlitCameraTexture(cmd, _tempRT, _cameraColor, material, 1);

    context.ExecuteCommandBuffer(cmd);
    CommandBufferPool.Release(cmd);
}


    public override void OnCameraCleanup(CommandBuffer cmd) { }

    public void Dispose()
    {
        _tempRT?.Release();
        CoreUtils.Destroy(material);
    }
}
