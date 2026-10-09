using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yudiz.VRAwarenessExperience.Utilities;
using Yudiz.VRAwarenessExperience.Manager;
using Yudiz.VRAwarenessExperience.Core;
using DG.Tweening;
using NaughtyAttributes;
using System.Threading;
using TMPro;
using StarterKit.Utilities;

namespace UISystem
{
    public class GameplayScreen : Screen
    {
        [Header("Precision Slider")]
        [HorizontalLine(color: EColor.Green)]
        [SerializeField] private float lerpDuration = 0.5f;
        [SerializeField] private Image precisionBarImage;

        [Header("Timer")]
        [HorizontalLine(color: EColor.Blue)]
        [SerializeField] private Image timeBarImage;
        [SerializeField] private TMP_Text timerText;

        [Header("Wrong Attempts Data")]
        [HorizontalLine(color: EColor.Yellow)]
        [SerializeField] private TMP_Text wrongAttemptsText;
        [SerializeField] private Animatable wrongAttemptsContentAnimatable;

        [Header("Debug")]
        [SerializeField] private bool isDebugMode = false;

        private Tween precisionTween;

        public override void Show()
        {
            base.Show();
            precisionBarImage.fillAmount = 0f;
            timerText.text = "0";
            Level.OnPrecisionValueChanged += OnPrecisionValueChanged;
            Level.OnWrongAttempt += OnWrongAttemptChanged;
            TimerManager.onNewTimerAdded += OnNewTimerAdded;
            TimerManager.onTimerRemoved += OnTimerRemoved;
            TimerManager.instance.RegisterTimerUpdate(StringConstants.GAMEPLAY_TIMER_ID, OnTimerUpdate);
            Debug.Log("Playing sound");
        }

        private void OnTimerUpdate(float time, float duration)
        {
            float normalizedTimeValue = time / duration;
            timeBarImage.fillAmount = normalizedTimeValue;
            timerText.text = Utilities.FormatTime(time);
        }

        private void OnNewTimerAdded(string timerId)
        {
            if (timerId == StringConstants.GAMEPLAY_TIMER_ID)
            {
                TimerManager.instance.RegisterTimerUpdate(StringConstants.GAMEPLAY_TIMER_ID, OnTimerUpdate);
            }
        }

        private void OnTimerRemoved(string timerId)
        {
            Debug.Log("GameplayScreen -> OnTimerRemoved -> Called! Timer ID: " + timerId);
            if (timerId == StringConstants.GAMEPLAY_TIMER_ID)
            {
                TimerManager.instance.UnregisterTimerUpdate(StringConstants.GAMEPLAY_TIMER_ID, OnTimerUpdate);
                precisionBarImage.fillAmount = 0f;
                timerText.text = Utilities.FormatTime(0f);
            }
        }

        private void OnPrecisionValueChanged(float value)   
        {
            precisionTween?.Kill();
            
            precisionTween = DOTween.To(() => precisionBarImage.fillAmount, x => {
                precisionBarImage.fillAmount = x;
            }, value, lerpDuration)
            .SetEase(Ease.Linear);
        }

        private void OnWrongAttemptChanged(int currentWrongAttempts, int wrongAttemptsLimit)
        {
            wrongAttemptsContentAnimatable.StartAnimate();
            wrongAttemptsText.text = $"{currentWrongAttempts} / {wrongAttemptsLimit}";
        }

        [EnableIf("isDebugMode")]
        [Button("Test Wrong Attempt UI")]
        public void TestWrongAttemptUI()
        {
            if (!isDebugMode) return;
            int currentWrongAttempt = 0;
            currentWrongAttempt++;
            OnWrongAttemptChanged(currentWrongAttempt, 3);
            this.DelayedInvoke(() => {
                currentWrongAttempt = 0;
                OnWrongAttemptChanged(currentWrongAttempt, 3);
            }, 0.5f);
        }

        public override void Hide()
        {
            base.Hide();
            Level.OnPrecisionValueChanged -= OnPrecisionValueChanged;
            TimerManager.onNewTimerAdded -= OnNewTimerAdded;
            TimerManager.onTimerRemoved -= OnTimerRemoved;
            Level.OnWrongAttempt -= OnWrongAttemptChanged;
            TimerManager.instance.UnregisterTimerUpdate(StringConstants.GAMEPLAY_TIMER_ID, OnTimerUpdate);
            timerText.text = Utilities.FormatTime(0f);
            precisionTween?.Kill();
        }
    }
}

