using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Yudiz.VRAwarenessExperience.Core;
using Yudiz.VRAwarenessExperience.Effect;
using Yudiz.VRAwarenessExperience.Manager;

namespace Yudiz.VRAwarenessExperience.Data
{
    [Serializable]
    public class LevelContainer
    {
        public LevelName levelName;
        public int levelTimeInSeconds;
        public bool isPillEffectRequired;
        public PillType pillType;
    }

    public enum LevelName
    {
        NormalMovementLevel = 0,
        SlowMovementLevel = 1,
        FastMovementLevel = 2,
        DrunkMovementLevel = 3,
    }

    public enum TransitionType
    {
        None,
        PillWobble,
    }


    [Serializable]
    public class PillEffectContainer
    {
        public PillType pillType;
        public MovementData movementData;
        public bool isPillRequired;
        public Pill pillPrefab;
        public Transform pillParent;
        public bool isTransitionRequired;
        public TransitionData fromEffectData;
        public TransitionData toEffectData;
        public bool isEffectRequired;
        public List<EffectDataContainer> effectData;
    }


    [Serializable]
    public class DistortionData
    {
        [Header("Distortion")]
        [HorizontalLine(color: EColor.Green)]
        public float distortionAmplitude;
        public Vector2 distortionPanSpeed;
        public float blend;
        public float noiseScale;

        [Header("Chromatic Abberation")]
        [HorizontalLine(color: EColor.Blue)]
        public float chromaticAbberationStrenght;
        public Vector2 chromaticAbberationDirection;
        public float chromaticAbberationIntensity;
    }

    
    [Serializable]
    public class KawaseBlurData
    {
        public int iterations;
        public int downsample;
        public int offset;
    }

    [Serializable]
    public class EffectDataContainer
    {
        public EffectsType effectsType;
        public EffectData effectData;
    }

    [Serializable]
    public class PillIndicatorData
    {
        public PillType pillType;
        public Transform pillIndicatorSpawnPoint;
    }
}
