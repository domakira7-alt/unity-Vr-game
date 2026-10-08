using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using StarterKit;
using UISystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Yudiz.VRAwarenessExperience.Events;
using Yudiz.XRStarter;

namespace Yudiz.VRAwarenessExperience.XR
{
    public class XRManager : StarterKit.Singleton<XRManager>
    {
        [Header("Interactors")]
        [HorizontalLine(color: EColor.Green)]
        [SerializeField] private XRDirectInteractor leftHandInteractor;
        [SerializeField] private XRDirectInteractor rightHandInteractor;

        [Header("Hand Models")]
        [HorizontalLine(color: EColor.Blue)]
        [SerializeField] private GameObject leftHandModel;
        [SerializeField] private GameObject rightHandModel;

        [Header("Controls Handler")]
        [HorizontalLine(color: EColor.Yellow)]
        [SerializeField] private XRControlsHandler xrControlsHandler;

        [Header("PauseInput")]
        [SerializeField] private InputActionReference pauseInput;


        private void OnEnable()
        {
            EventManager.OnLevelStarted += OnLevelStarted;
            EventManager.OnLevelCompleted += OnLevelCompleted;
            EventManager.OnLevelFailed += OnLevelFailed;
        }

        public override void OnAwake()
        {
            Debug.Log("XRManager -> OnAwake Called!");
            ToggleControls(XRControlsSwitcher.InteractorType.RayInteraction);
        }

        private void OnLevelStarted()
        {
            pauseInput.action.Enable();
            pauseInput.action.performed += OnPauseInputPerformed;
            ToggleControls(XRControlsSwitcher.InteractorType.DirectInteraction);
        }

        private void OnLevelCompleted()
        {
            pauseInput.action.performed -= OnPauseInputPerformed;
            pauseInput.action.Disable();
            ToggleControls(XRControlsSwitcher.InteractorType.RayInteraction);
        }

        private void OnLevelFailed()
        {
            pauseInput.action.performed -= OnPauseInputPerformed;
            pauseInput.action.Disable();
            ToggleControls(XRControlsSwitcher.InteractorType.RayInteraction);
        }

        private void OnPauseInputPerformed(InputAction.CallbackContext context)
        {
            ToggleControls(XRControlsSwitcher.InteractorType.RayInteraction);
            ViewController.instance.OpenPopup(ScreenName.PausePopup);
        }

       

        public void ToggleHandModel(bool isActive, HandSide handSide)
        {
            if (handSide == HandSide.Left)
            {
                leftHandModel.SetActive(isActive);
            }
            else
            {
                rightHandModel.SetActive(isActive);
            }
        }

        public void ToggleControls(XRControlsSwitcher.InteractorType interactionType)
        {
           xrControlsHandler.ChangeInteraction(HandSide.Left, interactionType);
           xrControlsHandler.ChangeInteraction(HandSide.Right, interactionType);
        }

        [ContextMenu("ShowPauseScreen")]
        public void ShowPauseScreen()
        {
            ViewController.instance.OpenPopup(ScreenName.PausePopup);
        }
        
        private void OnDisable()
        {
            EventManager.OnLevelStarted -= OnLevelStarted;
            EventManager.OnLevelCompleted -= OnLevelCompleted;
            EventManager.OnLevelFailed -= OnLevelFailed;
        }
    }
}

