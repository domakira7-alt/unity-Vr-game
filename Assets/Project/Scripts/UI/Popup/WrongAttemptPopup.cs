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
    public class WrongAttemptPopup : Screen
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;

        public void SetData(string title, string description, bool isTimerBased, float timerDuration)
        {
            titleText.text = title;
            descriptionText.text = description;

            if (isTimerBased)
            {
                Debug.Log("TimeDuration" + timerDuration);
              
                this.DelayedInvoke(() => ViewController.instance.ClosePopup(ScreenName.WrongAttemptPopup), timerDuration);
            }
        }

        public override void Hide()
        {
            base.Hide();
        }
    }
}
