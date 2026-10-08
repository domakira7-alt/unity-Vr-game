using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurSettings : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Standard Deviation (Spread) of the Gaussian blur")]
    public ClampedFloatParameter stenght = new ClampedFloatParameter(0f, 0f, 15f);
    
    public bool IsActive()
    {
        return (stenght.value > 0f) && active;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
