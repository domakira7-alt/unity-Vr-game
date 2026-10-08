using System.Collections.Generic;
using UnityEngine;
using Yudiz.VRAwarenessExperience.Data;
using Yudiz.VRAwarenessExperience.Transition;
using StarterKit;
using System;

namespace Yudiz.VRAwarenessExperience.Manager
{
    [System.Serializable]
    public class TransitionDataContainer
    {
        public TransitionType transitionType;
        public TransitionBase transition;
    }

    public class TransitionsManager : Singleton<TransitionsManager>
    {
        [SerializeField] private List<TransitionDataContainer> transitionDataContainers = new List<TransitionDataContainer>();
        private List<TransitionDataContainer> activeTransitionDataContainers = new List<TransitionDataContainer>();

        public void PlayTransition(TransitionType transitionType, TransitionData fromEffectData, TransitionData toEffectData, bool isAutoReset = false, Action onComplete = null)
        {
            TransitionDataContainer transitionDataContainer = transitionDataContainers.Find(x => x.transitionType == transitionType);
            if (transitionDataContainer != null)
            {
                activeTransitionDataContainers.Add(transitionDataContainer);
                transitionDataContainer.transition.StartTransition(fromEffectData, toEffectData, onComplete);
            }
        }

        public void ForceStopAllTransitions()
        {
            foreach (TransitionDataContainer transitionDataContainer in activeTransitionDataContainers)
            {
                transitionDataContainer.transition.ForceStopTransition();
            }
            activeTransitionDataContainers.Clear();
        }

        public T GetTransition<T>(TransitionType transitionType) where T : TransitionBase
        {
            TransitionDataContainer transitionDataContainer = transitionDataContainers.Find(x => x.transitionType == transitionType);
            if (transitionDataContainer != null)
            {
                return transitionDataContainer.transition.GetComponent<T>();
            }

            return null;
        }
    }
}
