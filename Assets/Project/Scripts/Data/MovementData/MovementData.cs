using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Yudiz.VRAwarenessExperience.Interactions;

namespace Yudiz.VRAwarenessExperience.Data
{
    public class MovementData : ScriptableObject
    {
        [Header("Movement Type")]
        [HorizontalLine(color: EColor.Green)]
        public PuzzleMovementType movementType;
    }
}

