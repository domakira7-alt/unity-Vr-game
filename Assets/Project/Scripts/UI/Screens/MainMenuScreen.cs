using UnityEngine;
using UnityEngine.UI;
using Yudiz.VRAwarenessExperience.Manager;
using Yudiz.VRAwarenessExperience.XR;

namespace UISystem
{
    public class MainMenuScreen : Screen
    {
        [SerializeField] private Button playButton;
        [SerializeField] private GameObject registrationPrefab;
        private PlayerDetailsPopup playerDetailsPopup;

        public override void Show()
        {
            base.Show();
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        private void OnPlayButtonClicked()
        {
            SoundManager.instance.PlaySound(SoundType.UIButtonClick);
            if (playerDetailsPopup == null)
            {
                playerDetailsPopup = new PlayerDetailsPopup(content.transform, StartGameplay, registrationPrefab);
            }
            playerDetailsPopup.Show();
        }

        private void StartGameplay()
        {
            LevelManager.instance.LoadNextLevel();
            ViewController.instance.ChangeView(ScreenName.GameplayScreen);
        }

        public override void Hide()
        {
            base.Hide();
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
            playerDetailsPopup?.Hide();
        }

    }
}
