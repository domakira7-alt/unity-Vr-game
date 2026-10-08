using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Post-processing/Kawase Blur")]
public class KawaseBlurSettings : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Number of blur iterations (1–8). Each iteration is one Kawase pass.")]
    public ClampedIntParameter iterations = new ClampedIntParameter(3, 0, 8);

    [Tooltip("Downsample steps: 0 = full res, 1 = 1/2, 2 = 1/4.")]
    public ClampedIntParameter downsample = new ClampedIntParameter(1, 0, 2);

    [Tooltip("Kawase offset (tap radius in pixels of the working RT). 0.5–3 typically.")]
    public ClampedFloatParameter offset = new ClampedFloatParameter(1.5f, 0.25f, 4.0f);

    public bool IsActive() => active && iterations.value > 0;
    public bool IsTileCompatible() => false;
}
