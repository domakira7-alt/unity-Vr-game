using System.Collections;
using System.Collections.Generic;
using StarterKit.Utilities;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Yudiz.VRAwarenessExperience.Utilities;
using Yudiz.VRAwarenessExperience.Manager;

namespace UISystem
{
    public class SplashScreen : Screen
    {
        [SerializeField] private Image progressBar;
        [SerializeField] private TMP_Text progressText;
        public override void Show()
        {
            base.Show();
            progressBar.fillAmount = 0f;
            progressText.text = StringConstants.TEXT_STRING_DEFAULT_LOADING;
            SoundManager.instance.PlaySound(SoundType.BackgroundMusicSound);
            ShowProgressBar();
        }

        private void ShowProgressBar()
        {
            progressBar.fillAmount = 0f;
            progressBar.DOFillAmount(1f, 5f).OnUpdate(() => {
                progressText.text = $"Loading...{(int)(progressBar.fillAmount * 100)}%";
            }).SetEase(Ease.Linear).OnComplete(() => 
            {
                ViewController.instance.ChangeView(ScreenName.MainMenuScreen);
                if (FPSCounter.instance != null)
                    FPSCounter.instance.EnableFPS(true);
            });
        }
    }
}
