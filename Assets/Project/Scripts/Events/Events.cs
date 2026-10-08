using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yudiz.VRAwarenessExperience.Events
{
    public static class EventManager
    {
        public delegate void OnLevelStartedDelegate();
        public static event OnLevelStartedDelegate OnLevelStarted;

        public delegate void OnLevelCompletedDelegate();
        public static event OnLevelCompletedDelegate OnLevelCompleted;

        public delegate void OnLevelFailedDelegate();
        public static event OnLevelFailedDelegate OnLevelFailed;

        public static void LevelStarted()
        {
            OnLevelStarted?.Invoke();
        }

        public static void LevelCompleted()
        {
            OnLevelCompleted?.Invoke();
        }
        

        public static void LevelFailed()
        {
            OnLevelFailed?.Invoke();
        }
    }
}

