using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;


namespace Yudiz.VRAwarenessExperience.Data
{
    [CreateAssetMenu(fileName = "PillEffectData", menuName = "Effects/WobbleEffects/PillEffectData")]
    public class PillTransitionData : WobbleTransitionData
    {
        [Header("PillEffect")]
        [HorizontalLine(color: EColor.Green)]
        [ColorUsage(false, true)]
        public Color pillColor;
    }
}

