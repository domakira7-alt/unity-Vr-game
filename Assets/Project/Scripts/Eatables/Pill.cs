using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Yudiz.VRAwarenessExperience.Data;
using Yudiz.VRAwarenessExperience.Manager;

namespace Yudiz.VRAwarenessExperience.Core
{
    public class Pill : MonoBehaviour, IEatable
    {
        [SerializeField] private MeshRenderer meshRenderer;
        private XRGrabInteractable xRGrabInteractable;
        private Level level;
        private PillType pillType;

        private void OnEnable()
        {
            xRGrabInteractable = GetComponent<XRGrabInteractable>();
        }

        public void InitializePill(PillType pillType, Level level, bool isInteractable, Material material)
        {
            this.pillType = pillType;
            this.level = level;
            xRGrabInteractable.enabled =  isInteractable;
            if (isInteractable)
            {
                Material[] newMaterials = new Material[meshRenderer.materials.Length + 1];
                for (int counter = 0; counter < meshRenderer.materials.Length; counter++)
                {
                    newMaterials[counter] = meshRenderer.materials[counter];
                }
                newMaterials[newMaterials.Length - 1] = material;
                meshRenderer.materials = newMaterials;
            }
        }

        public void Eat()
        {
            if (level != null)
            {
                Debug.Log("Inside Eat method");
                SoundManager.instance.PlaySound(SoundType.PillEatSound);
                level.OnPillEaten(pillType);
                gameObject.SetActive(false);
            }
        }
    }

    public enum PillType
    {
        None,
        Slow,
        Fast,
        Drunk,
    }
}

