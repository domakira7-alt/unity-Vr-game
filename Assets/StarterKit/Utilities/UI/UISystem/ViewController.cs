using System.Collections;
using System.Collections.Generic;
using StarterKit;
using UnityEngine;
using UnityEngine.InputSystem;
namespace UISystem
{

    public class ViewController : Singleton<ViewController>
    {
        Screen currentView;
        Screen previousView;
        ScreenName currentScreenName;
        [SerializeField] ScreenName initScreen;

        [Divider]
        [SerializeField] List<ScreenView> screens = new List<ScreenView>();
        [Divider]
        [SerializeField] ToastMessage toastPrefab;

        private Dictionary<ScreenName, List<ScreenName>> activePopups = new Dictionary<ScreenName, List<ScreenName>>();

        // Toast cooldown system
        private string lastToastMessage = "";
        private float lastToastTime = 0f;
        private const float TOAST_COOLDOWN_DURATION = 2f; // 2 seconds cooldown

        [System.Serializable]
        public struct ScreenView
        {
            public Screen screen;
            public ScreenName screenName;
        }

        void Start() => Init();

        // Helper method to check if toast can be shown
        private bool CanShowToast(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;
                
            // If it's the same message and within cooldown period, don't show
            if (message == lastToastMessage && Time.time - lastToastTime < TOAST_COOLDOWN_DURATION)
            {
                return false;
            }
            
            return true;
        }

        // Helper method to update toast tracking
        private void UpdateToastTracking(string message)
        {
            lastToastMessage = message;
            lastToastTime = Time.time;
        }

        // Original methods (keeping for backward compatibility)
        public void ShowToast(Vector2 anchoredOffset, string message, float duration = 1f)
        {
            if (!CanShowToast(message))
                return;
                
            var toast = Instantiate(toastPrefab, transform);
            toast.SetToastAnchoredPosition(anchoredOffset, message, duration);
            UpdateToastTracking(message);
        }

        public void ShowToastFade(Vector2 rectPosition, string message, float duration = 1f)
        {
            if (!CanShowToast(message))
                return;
                
            var toast = Instantiate(toastPrefab, transform);
            toast.SetToastFadeWithOffset(rectPosition, message, duration);
            UpdateToastTracking(message);
        }

        public void ShowToastAtPosition(Vector2 screenOffset, string message, float duration = 1f, Color? textColor = null)
        {
            if (!CanShowToast(message))
                return;
                
            var toast = Instantiate(toastPrefab, transform);
            toast.SetToastAnchoredPosition(screenOffset, message, duration, textColor);
            UpdateToastTracking(message);
        }
        // Predefined positions for common use cases
        public void ShowToastTop(string message, float duration = 1f,Color? textColor = null)
        {
            Vector2 topOffset = new Vector2(0, 200); // 200 units above center
            ShowToastAtPosition(topOffset, message, duration,textColor);
        }

        public void ShowToastBottom(string message, float duration = 1f, Color? textColor = null)
        {
            Vector2 bottomOffset = new Vector2(0, -200); // 200 units below center
            ShowToastAtPosition(bottomOffset, message, duration, textColor);
        }

        public void ShowToastCenter(string message, float duration = 1f,Color? textColor = null)
        {
            Vector2 centerOffset = Vector2.zero; // Exactly at center
            ShowToastAtPosition(centerOffset, message, duration, textColor);
        }

        // Custom offset method
        public void ShowToastWithCustomOffset(float xOffset, float yOffset, string message, float duration = 1f)
        {
            Vector2 customOffset = new Vector2(xOffset, yOffset);
            ShowToastAtPosition(customOffset, message, duration);
        }

        public void ChangeView(ScreenName screen)
        {
            if(screen == currentScreenName || screen == ScreenName.None)
            {
                return;
            }
            if (currentView != null)
            {
                previousView = currentView;
                previousView.Hide();
                ClosePopupsLinkedWithScreen(currentScreenName);
                currentView = screens[GetScreenIndex(screen)].screen;
                currentScreenName = screen;
                currentView.Show();
            }
            else
            {
                currentView = screens[GetScreenIndex(screen)].screen;
                currentScreenName = screen;
                currentView.Show();
            }
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                // Check if any popups are open; if not, show exit popup
                if (activePopups.Count == 0)
                {
                    // TODO : Add ExitScreen
                    // OpenPopup(ScreenName.ExitScreen); // Replace with your actual ScreenName for ExitScreen
                }
                else
                {
                    // Close the topmost popup (optional)
                    foreach (var screenType in activePopups.Keys)
                    {
                        if (activePopups[screenType].Count > 0)
                        {
                            var lastPopup = activePopups[screenType][activePopups[screenType].Count - 1];
                            ClosePopup(lastPopup);
                            return;
                        }
                    }
                }
            }
        }
        public Screen GetCurrentScreen()
        {
            return currentView;
        }

        // Add this property to get current screen name
        public ScreenName CurrentScreenName => currentScreenName;

        int GetScreenIndex(ScreenName screen)
        {
            return screens.FindIndex(
            delegate (ScreenView screenView)
            {
                return screenView.screenName.Equals(screen);
            });
        }

        public void RedrawView() => currentView.Redraw();

        private void Init()
        {
            for (int indexOfScreen = 0; indexOfScreen < screens.Count; indexOfScreen++)
            {
                screens[indexOfScreen].screen.Disable();
            }

            if (initScreen != ScreenName.None)
            {
                ChangeView(initScreen);
            }
        }

        public void OpenPopup(ScreenName popupScreenType, ScreenName LinkedScreen = ScreenName.None)
        {
            if (!activePopups.ContainsKey(LinkedScreen))
            {
                activePopups[LinkedScreen] = new List<ScreenName>();
            }

            GetScreen<Screen>(popupScreenType).Show();
            activePopups[LinkedScreen].Add(popupScreenType);
        }

        public void ClosePopup(ScreenName popupScreenType)
        {
            foreach (var screenType in activePopups.Keys)
            {

                foreach (var popup in activePopups[screenType])
                {

                    if (popup == popupScreenType)
                    {
                        GetScreen<Screen>(popupScreenType).Hide();
                        activePopups[screenType].Remove(popupScreenType);

                        if (activePopups[screenType].Count == 0)
                        {
                            activePopups.Remove(screenType);
                        }

                        return;
                    }
                }
            }
        }

        private void ClosePopupsLinkedWithScreen(ScreenName screenType)
        {
            if (activePopups.ContainsKey(screenType))
            {
                foreach (var popup in activePopups[screenType])
                {
                    GetScreen<Screen>(popup).Hide();
                }
                activePopups.Remove(screenType);
            }
        }

        public ScreenName GetCurrentScreenName()
        {
            return currentScreenName;
        }


        public T GetScreen<T>(ScreenName sName) => (T)screens[GetScreenIndex(sName)].screen.GetComponent<T>();
    }
}
