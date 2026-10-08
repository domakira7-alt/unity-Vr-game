using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Yudiz.VRAwarenessExperience.Data
{
    [CreateAssetMenu(fileName = "SlowMovementData", menuName = "MovementData/SlowMovementData")]
    public class SlowMovementData : MovementData
    {
        [Header("Slow Movement (Interpolation) Data")]
        [HorizontalLine(color: EColor.Green)]
        public float lerpSpeed;
        public float delay;
    }
}

