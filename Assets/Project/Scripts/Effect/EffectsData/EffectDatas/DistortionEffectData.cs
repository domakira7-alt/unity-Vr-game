using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Yudiz.VRAwarenessExperience.Data;

namespace Yudiz.VRAwarenessExperience.Effect
{
    [CreateAssetMenu(fileName = "DistortionEffectData", menuName = "Effect/DistortionEffectData")]
    public class DistortionEffectData : EffectData
    {
        public DistortionData fromDistortionData;
        public DistortionData toDistortionData;
    }
        
}

