using UnityEngine;
using UnityEngine.UI;
using Yudiz.VRAwarenessExperience.Manager;
using Yudiz.VRAwarenessExperience.XR;

namespace UISystem
{
    public class MainMenuScreen : Screen
    {
        [SerializeField] private Button playButton;

        public override void Show()
        {
            base.Show();
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        private void OnPlayButtonClicked()
        {
            SoundManager.instance.PlaySound(SoundType.UIButtonClick);
            LevelManager.instance.LoadNextLevel();
            ViewController.instance.ChangeView(ScreenName.GameplayScreen);
        }

        public override void Hide()
        {
            base.Hide();
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
        }

    }
}
