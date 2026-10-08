using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yudiz.VRAwarenessExperience.Data;

namespace Yudiz.VRAwarenessExperience.Effect
{
    [CreateAssetMenu(fileName = "KawaseBlurEffectData", menuName = "Effect/KawaseBlurEffectData")]
    public class KawaseBlurEffectData : EffectData
    {
        public KawaseBlurData fromKawaseBlurData;
        public KawaseBlurData toKawaseBlurData;
    }
}
