using System.Collections;
using System.Collections.Generic;
using CommanTickManager;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using Yudiz.VRAwarenessExperience.Events;
using Yudiz.VRAwarenessExperience.Manager;
using Yudiz.VRAwarenessExperience.XR;

namespace UISystem
{
    public class PausePopup : Screen, ITick
    {
        [Header("Buttons")]
        [HorizontalLine(color: EColor.Blue)]
        [SerializeField] private Button noButton;
        [SerializeField] private Button yesButton;

        [Header("UI Settings")]
        [HorizontalLine(color: EColor.Green)]
        [SerializeField] private float uiZOffset = 0.8f;
        [SerializeField] private float alignmentSpeed = 1.1f;

        private Transform cameraTransform;


        public override void Show()
        {
            base.Show();
            cameraTransform = Camera.main.transform;
            SetCanvasPosition();
            noButton.onClick.AddListener(OnNoButtonClicked);
            yesButton.onClick.AddListener(OnYesButtonClicked);
            ProcessingUpdate.Instance.Add(this);
        }

        public void Tick()
        {
            SetCanvasPosition();
        }

        public void OnNoButtonClicked()
        {
            SoundManager.instance.PlaySound(SoundType.UIButtonClick);
            ViewController.instance.ClosePopup(ScreenName.PausePopup);
            ViewController.instance.ChangeView(ScreenName.GameplayScreen);
        }

        private void SetCanvasPosition()
        {
            Vector3 uiDirection = cameraTransform.forward;
            Vector3 newPosition = cameraTransform.position + uiDirection * uiZOffset;
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * alignmentSpeed);

            Vector3 direction = transform.position - cameraTransform.position;
            Vector3 lookAtPoint = transform.position + direction;

            transform.LookAt(lookAtPoint);
        }

        public void OnYesButtonClicked()
        {
            LevelManager.instance.ExitGameLevel();
            SoundManager.instance.PlaySound(SoundType.UIButtonClick);
            SoundManager.instance.StopClockTickSound();
            SoundManager.instance.ResetSoundPitch();
            EventManager.LevelFailed();
            XRManager.instance.ToggleControls(XRControlsSwitcher.InteractorType.RayInteraction);
            ViewController.instance.ClosePopup(ScreenName.PausePopup);
            ViewController.instance.ChangeView(ScreenName.MainMenuScreen);
        }

        public override void Hide()
        {
            base.Hide();
            noButton.onClick.RemoveListener(OnNoButtonClicked);
            yesButton.onClick.RemoveListener(OnYesButtonClicked);
            ProcessingUpdate.Instance.Remove(this);
        }
    }
}

