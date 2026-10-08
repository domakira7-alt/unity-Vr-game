using System;
using System.Collections;
using System.Collections.Generic;
using CommanTickManager;
using NaughtyAttributes;
using StarterKit.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace UISystem
{
    public class GeneralPopup : Screen, ITick
    {
        [Header("UI Elements")]
        [HorizontalLine(color: EColor.Blue)]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
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
            ProcessingUpdate.Instance.Add(this);
        }

        public void SetData(string title, string description, bool isTimerBased, bool isYesButtonVisible, bool isNoButtonVisible, float timerDuration = 2f, Action onYesButtonClicked = null, Action onNoButtonClicked = null)
        {
            titleText.text = title;
            descriptionText.text = description;
            yesButton.gameObject.SetActive(isYesButtonVisible);
            noButton.gameObject.SetActive(isNoButtonVisible);
            yesButton.onClick.AddListener(() => onYesButtonClicked?.Invoke());
            noButton.onClick.AddListener(() => onNoButtonClicked?.Invoke());
            if (isTimerBased)
            {
                this.DelayedInvoke(() => ViewController.instance.ClosePopup(ScreenName.GeneralPopup), timerDuration);
            }
        }

        public void Tick()
        {
            SetCanvasPosition();
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

        public override void Hide()
        {
            base.Hide();
            ProcessingUpdate.Instance.Remove(this);
            yesButton.onClick.RemoveAllListeners();
            noButton.onClick.RemoveAllListeners();
        }
    }

}
