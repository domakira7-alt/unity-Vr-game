using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Yudiz.VRAwarenessExperience.Data
{
    [CreateAssetMenu(fileName = "FastMovementData", menuName = "MovementData/FastMovementData")]
    public class FastMovementData : MovementData
    {
        [Header("Fast Movement (Harmonic Motion) Data")]
        [HorizontalLine(color: EColor.Green)]
        public float springConstant;
        public float dampingRatio;
        public float overshootFactor;
        public float maxVelocity;
    }
}

