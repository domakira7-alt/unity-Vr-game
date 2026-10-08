using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class KawaseBlurRenderPass : ScriptableRenderPass
{
    static readonly string kTag = "Kawase Blur (URP)";

    public Shader kawaseShader; // assigned from feature
    Material _mat;

    KawaseBlurSettings _settings;

    RTHandle _cameraColor;
    RTHandle _rtA;
    RTHandle _rtB;

    public bool Setup(ScriptableRenderer renderer)
    {
        _settings = VolumeManager.instance.stack.GetComponent<KawaseBlurSettings>();
        renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing; // or BeforeRenderingPostProcessing

        if (_settings == null || !_settings.IsActive())
            return false;

        if (kawaseShader == null)
            kawaseShader = Shader.Find("PostProcessing/KawaseBlur");

        if (kawaseShader == null)
        {
            Debug.LogError("KawaseBlurRenderPass: Shader not found (PostProcessing/KawaseBlur).");
            return false;
        }

        if (_mat == null || _mat.shader != kawaseShader)
            _mat = CoreUtils.CreateEngineMaterial(kawaseShader);

        ConfigureInput(ScriptableRenderPassInput.Color); // ensure readable color target
        return true;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        _cameraColor = renderingData.cameraData.renderer.cameraColorTargetHandle;

        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        desc.msaaSamples = 1; // resolve for post

        int down = Mathf.Clamp(_settings.downsample.value, 0, 2);
        int div  = 1 << down;
        desc.width  = Mathf.Max(1, desc.width  / div);
        desc.height = Mathf.Max(1, desc.height / div);

        RenderingUtils.ReAllocateIfNeeded(ref _rtA, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_KawaseA");
        RenderingUtils.ReAllocateIfNeeded(ref _rtB, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_KawaseB");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_mat == null || _settings == null || !_settings.IsActive())
            return;

        if (renderingData.cameraData.cameraType == CameraType.Preview)
            return;

        var cmd = CommandBufferPool.Get(kTag);

        // Parameters
        int iters = Mathf.Clamp(_settings.iterations.value, 1, 8);
        _mat.SetFloat("_Offset", _settings.offset.value);

        // Downsample from camera to A (binds _BlitTexture & resolves MSAA)
        Blitter.BlitCameraTexture(cmd, _cameraColor, _rtA);

        // Kawase iterations (ping-pong A <-> B)
        bool ping = true;
        for (int i = 0; i < iters; i++)
        {
            // Optional: slightly grow offset each iteration for stronger blur
            _mat.SetFloat("_Offset", _settings.offset.value + 0.25f * i);

            if (ping) Blitter.BlitCameraTexture(cmd, _rtA, _rtB, _mat, 0);
            else      Blitter.BlitCameraTexture(cmd, _rtB, _rtA, _mat, 0);

            ping = !ping;
        }

        // Upsample back to camera
        var src = ping ? _rtA : _rtB; // last written is the opposite of current 'ping'
        Blitter.BlitCameraTexture(cmd, src, _cameraColor);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Dispose()
    {
        _rtA?.Release();
        _rtB?.Release();
        CoreUtils.Destroy(_mat);
    }
}
