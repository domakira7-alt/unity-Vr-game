using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Yudiz.VRAwarenessExperience.Data
{
    public class WobbleTransitionData : TransitionData
    {
        [Header("Wobble")]
        [HorizontalLine(color: EColor.Red)]
        public float wobbleAmplitude;
        public Vector2 wobbleSpeed;
        [Range(0, 1)]public float blend;
        public float noiseScale;
    }
}

