using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yudiz.VRAwarenessExperience.Core
{
    public class ManageEatables : MonoBehaviour
    {
        public delegate void OnEatableEatenDelegate(IEatable eatable);
        public static event OnEatableEatenDelegate OnEatableEaten;
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IEatable>(out IEatable eatable))
            {
                Debug.Log("Pill is eaten");
                OnEatableEaten?.Invoke(eatable);
                eatable.Eat();                
            }
        }
    }
}

