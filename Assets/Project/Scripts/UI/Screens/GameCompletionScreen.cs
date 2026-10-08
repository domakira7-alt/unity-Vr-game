using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yudiz.VRAwarenessExperience.Manager;
using Yudiz.VRAwarenessExperience.Utilities;
using Yudiz.VRAwarenessExperience.XR;
using Yudiz.VRAwarenessExperience.Core;

namespace UISystem
{
    public class GameCompletionScreen : Screen
    {
        [Header("Game Outcome")]
        [HorizontalLine(color: EColor.Green)]
        [SerializeField] private Image gameOutcomeImage;
        [SerializeField] private Sprite wonSprite;
        [SerializeField] private Sprite lostSprite;
        [SerializeField] private TMP_Text gameOutcomeText;
        [SerializeField] private TMP_Text continueButtonText;

        [Header("Buttons")]
        [HorizontalLine(color: EColor.Blue)]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button exitButton;
       

        public override void Show()
        {
            base.Show();
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        public void SetData(bool hasWon ,int wrongAttempts, bool isPillLevel = false, PillType  pillType= PillType.None)
        {
            gameOutcomeImage.sprite = hasWon ? wonSprite : lostSprite;
            //gameOutcomeText.text = hasWon ? StringConstants.TEXT_STRING_YOU_WON : StringConstants.TEXT_STRING_YOU_LOST;
            //gameOutcomeText.text = hasWon ? (isPillLevel ? StringConstants.TEXT_STRING_YOU_WON_PILL_LEVEL : StringConstants.TEXT_STRING_YOU_WON) : (wrongAttempts >= 3 ? StringConstants.TEXT_STRING_EXCEEDED_WRONG_ATTEMPTS : StringConstants.TEXT_STRING_YOU_LOST);

            if (hasWon)
            {
                if (isPillLevel)
                {
                    switch (pillType)
                    {
                        case PillType.Slow:
                            gameOutcomeText.text = StringConstants.TEXT_STRING_YOU_WON_SLOW_PILL;
                            break;
                        case PillType.Fast:
                            gameOutcomeText.text = StringConstants.TEXT_STRING_YOU_WON_FAST_PILL;
                            break;
                        case PillType.Drunk:
                            gameOutcomeText.text = StringConstants.TEXT_STRING_YOU_WON_DRUNK_PILL;
                            break;
                        default:
                            gameOutcomeText.text = StringConstants.TEXT_STRING_YOU_WON;
                            break;
                    }
                }
                else
                {
                    gameOutcomeText.text = StringConstants.TEXT_STRING_YOU_WON;
                }
            }
            else
            {
                gameOutcomeText.text = wrongAttempts >= 3
                    ? StringConstants.TEXT_STRING_EXCEEDED_WRONG_ATTEMPTS
                    : StringConstants.TEXT_STRING_YOU_LOST;
            }


            continueButtonText.text = hasWon ? StringConstants.TEXT_STRING_CONTINUE : StringConstants.TEXT_STRING_RETRY;
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(hasWon ? OnContinueButtonClicked : OnRetryButtonClicked);

            //if (hasWon)
            //{
            //    continueButton.onClick.AddListener(OnContinueButtonClicked);
            //}
            //else
            //{
            //    continueButton.onClick.AddListener(OnRetryButtonClicked);
            //}
        }

        public void OnContinueButtonClicked()
        {
            SoundManager.instance.PlaySound(SoundType.UIButtonClick);
            LevelManager.instance.LoadNextLevel();
            ViewController.instance.ChangeView(ScreenName.GameplayScreen);
        }

        private void OnRetryButtonClicked()
        {
            SoundManager.instance.PlaySound(SoundType.UIButtonClick);
            LevelManager.instance.ExitGameLevel();
            LevelManager.instance.LoadNextLevel();
            ViewController.instance.ChangeView(ScreenName.GameplayScreen);
        }

        public void OnExitButtonClicked()
        {
            SoundManager.instance.PlaySound(SoundType.UIButtonClick);
            LevelManager.instance.ExitGameLevel();
            ViewController.instance.ChangeView(ScreenName.MainMenuScreen);
        }

        public override void Hide()
        {
            base.Hide();
            continueButton.onClick.RemoveAllListeners();
        }
    }
}

