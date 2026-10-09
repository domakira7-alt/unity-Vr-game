using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    // Uses the menu's existing world-space canvas and XR raycaster.
    public sealed class PlayerDetailsPopup
    {
        private readonly RegistrationCsvStore registrationStore;
        private readonly GameObject root;
        private readonly TMP_InputField nameField;
        private readonly TMP_InputField emailField;
        private readonly TMP_Text message;
        private readonly Button startButton;
        private readonly Action onSubmit;
        private readonly TMP_FontAsset font;
        private readonly RectTransform registration;
        private readonly Transform menuContent;
        private readonly Dictionary<GameObject, bool> menuVisibility = new Dictionary<GameObject, bool>();
        private readonly Sprite buttonSprite;
        private bool submitting;
        private AdminRegistrationPanel adminPanel;
        private readonly RegistrationSheetClient requestRunner;
        private readonly IRegistrationUploader uploader;
        private readonly Button cancelButton;
        private SheetRegistration pendingRegistration;
        private DateTime pendingTimestamp;
        private bool savedOnline;

        public PlayerDetailsPopup(Transform parent, Action onSubmit, GameObject registrationPrefab = null,
            RegistrationCsvStore registrationStore = null, IRegistrationUploader uploader = null)
        {
            GameObject prefab = registrationPrefab != null ? registrationPrefab : Resources.Load<GameObject>("UI/PlayerRegistration");
            if (prefab == null)
                throw new InvalidOperationException("Missing registration prefab. Assign it on MainMenuScreen.");
            this.onSubmit = onSubmit;
            this.registrationStore = registrationStore ?? new RegistrationCsvStore(Application.persistentDataPath);
            menuContent = parent;

            RectTransform overlay = Rect("Player Details Popup", parent, Vector2.zero, Vector2.zero);
            overlay.anchorMin = Vector2.zero;
            overlay.anchorMax = Vector2.one;
            overlay.offsetMin = overlay.offsetMax = Vector2.zero;
            overlay.localPosition = new Vector3(0, 0, -1);
            root = overlay.gameObject;
            requestRunner = root.AddComponent<RegistrationSheetClient>();
            this.uploader = uploader ?? requestRunner;
            // Block clicks on the menu while leaving the hangar and table visible.
            overlay.gameObject.AddComponent<Image>().color = Color.clear;

            registration = (RectTransform)UnityEngine.Object.Instantiate(prefab, overlay, false).transform;
            nameField = registration.Find("PopupBG/Content/NameInputField").GetComponent<TMP_InputField>();
            emailField = registration.Find("PopupBG/Content/EmailInputField").GetComponent<TMP_InputField>();
            startButton = registration.Find("RegisterButton").GetComponent<Button>();
            font = registration.Find("PopupBG/HeaderText").GetComponent<TMP_Text>().font;
            buttonSprite = nameField.GetComponent<Image>().sprite;
            ConfigureField(nameField, 80);
            ConfigureField(emailField, 254);
            emailField.keyboardType = TouchScreenKeyboardType.EmailAddress;
            RectTransform formPanel = (RectTransform)registration.Find("PopupBG");
            formPanel.anchorMin = formPanel.anchorMax = new Vector2(0.5f, 0.5f);
            formPanel.sizeDelta = new Vector2(1500, 470);
            formPanel.anchoredPosition = new Vector2(0, 84);
            EnlargeField(nameField, 70);
            EnlargeField(emailField, -70);
            nameField.onSelect.AddListener(_ => SelectField(nameField));
            emailField.onSelect.AddListener(_ => SelectField(emailField));
            nameField.onValueChanged.AddListener(_ => Validate());
            emailField.onValueChanged.AddListener(_ => Validate());
            startButton.navigation = new Navigation { mode = Navigation.Mode.None };
            startButton.onClick.AddListener(Submit);
            message = Text("", overlay, new Vector2(0, 58), new Vector2(1200, 50), 26);
            Text("Select the name or email field to type.", overlay, new Vector2(0, -75), new Vector2(1200, 60), 26);
            cancelButton = Button("Cancel", overlay, new Vector2(0, -175), new Vector2(300, 80), Hide);
            Hide();
        }

        public void Show()
        {
            if (root.activeSelf) Hide();
            foreach (Transform child in root.transform)
                child.gameObject.SetActive(adminPanel == null || child.gameObject != adminPanel.Root);
            submitting = false;
            pendingRegistration = null;
            savedOnline = false;
            nameField.interactable = emailField.interactable = true;
            cancelButton.interactable = true;
            nameField.SetTextWithoutNotify("");
            emailField.SetTextWithoutNotify("");
            menuVisibility.Clear();
            foreach (Transform child in menuContent)
            {
                if (child.gameObject == root) continue;
                menuVisibility[child.gameObject] = child.gameObject.activeSelf;
                child.gameObject.SetActive(false);
            }
            root.SetActive(true);
            root.transform.SetAsLastSibling();
            registration.localScale = Vector3.one * 0.95f;
            registration.anchoredPosition = new Vector2(0, 230);
            SelectField(nameField);
            Validate();
        }

        public void Hide()
        {
            requestRunner.Cancel();
            nameField.DeactivateInputField();
            emailField.DeactivateInputField();
            root.SetActive(false);
            foreach (var entry in menuVisibility)
                if (entry.Key != null) entry.Key.SetActive(entry.Value);
            menuVisibility.Clear();
        }

        private bool Validate()
        {
            bool validName = !string.IsNullOrWhiteSpace(nameField.text);
            string email = emailField.text.Trim();
            bool validEmail = Regex.IsMatch(email, @"^[^\s@]+@[^\s@.]+(?:\.[^\s@.]+)+$");
            message.text = !validName ? "Please enter your name." :
                !validEmail ? "Please enter a valid email address." : "Vos informations seront enregistrées avant de commencer.";
            startButton.interactable = validName && validEmail && !submitting;
            return validName && validEmail;
        }

        private void SelectField(TMP_InputField field)
        {
            nameField.targetGraphic.color = field == nameField ? new Color(0.65f, 1f, 1f) : Color.white;
            emailField.targetGraphic.color = field == emailField ? new Color(0.65f, 1f, 1f) : Color.white;
        }

        private void Submit()
        {
            if (submitting || !Validate()) return;
            submitting = true;
            startButton.interactable = false;
            if (string.Equals(nameField.text.Trim(), "admin", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(emailField.text.Trim(), "admin@admin.com", StringComparison.OrdinalIgnoreCase))
            {
                nameField.DeactivateInputField();
                emailField.DeactivateInputField();
                if (adminPanel == null)
                    adminPanel = new AdminRegistrationPanel(root.transform, font, buttonSprite, registrationStore,
                        () => { Hide(); onSubmit(); }, () => { Hide(); Show(); });
                foreach (Transform child in root.transform)
                    child.gameObject.SetActive(child.gameObject == adminPanel.Root);
                adminPanel.Show();
                return;
            }
            nameField.DeactivateInputField();
            emailField.DeactivateInputField();
            nameField.interactable = emailField.interactable = false;
            cancelButton.interactable = false;
            if (!savedOnline)
            {
                pendingTimestamp = DateTime.UtcNow;
                pendingRegistration = new SheetRegistration
                {
                    name = nameField.text.Trim(),
                    email = emailField.text.Trim(),
                    dateTime = pendingTimestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                };
            }
            message.color = Color.white;
            message.text = "Enregistrement en cours…";
            requestRunner.StartCoroutine(SubmitRegistration());
        }

        private IEnumerator SubmitRegistration()
        {
            bool success = savedOnline;
            string error = null;
            if (!savedOnline)
                yield return uploader.Upload(pendingRegistration, (saved, failure) => { success = saved; error = failure; });
            if (!success)
            {
                ShowSubmissionError(error ?? RegistrationSheetClient.InternetAlert);
                yield break;
            }
            savedOnline = true;
            bool savedLocally = true;
            try { registrationStore.Save(pendingRegistration.name, pendingRegistration.email, pendingTimestamp); }
            catch (Exception exception)
            {
                savedLocally = false;
                Debug.LogError("Registration could not be saved to " + registrationStore.FilePath + ": " + exception.Message);
            }
            if (!savedLocally)
            {
                ShowSubmissionError("Enregistré en ligne. Réessayez pour terminer la sauvegarde locale.");
                yield break;
            }
            Debug.Log("Registration saved to " + registrationStore.FilePath);
            Hide();
            onSubmit();
        }

        private void ShowSubmissionError(string error)
        {
            submitting = false;
            startButton.interactable = true;
            cancelButton.interactable = true;
            nameField.interactable = emailField.interactable = !savedOnline;
            message.color = new Color(1f, 0.65f, 0.4f);
            message.text = error;
        }

        private static void ConfigureField(TMP_InputField field, int limit)
        {
            field.characterLimit = limit;
            field.lineType = TMP_InputField.LineType.SingleLine;
            field.richText = false;
            // TMP opens and synchronizes the Quest overlay through TouchScreenKeyboard.
            field.shouldHideSoftKeyboard = false;
            field.shouldHideMobileInput = false;
            field.keyboardType = TouchScreenKeyboardType.Default;
            field.restoreOriginalTextOnEscape = false;
            field.customCaretColor = true;
            field.caretColor = Color.white;
            field.textComponent.raycastTarget = false;
            field.placeholder.raycastTarget = false;
            field.navigation = new Navigation { mode = Navigation.Mode.None };
        }

        private static void EnlargeField(TMP_InputField field, float y)
        {
            RectTransform rect = (RectTransform)field.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, y);
            rect.sizeDelta = new Vector2(1280, 86);
            field.pointSize = 38;
            field.textComponent.enableAutoSizing = false;
            if (field.placeholder is TMP_Text placeholder)
            {
                placeholder.enableAutoSizing = false;
                placeholder.fontSize = 38;
            }
        }

        private Button Button(string label, Transform parent, Vector2 position, Vector2 size, Action action)
        {
            RectTransform rect = Rect(label, parent, position, size);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = buttonSprite;
            image.color = Color.white;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => action());
            Text(label, rect, Vector2.zero, size, 34);
            return button;
        }

        private TMP_Text Text(string value, Transform parent, Vector2 position, Vector2 size, float fontSize)
        {
            RectTransform rect = Rect("Text", parent, position, size);
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
            text.richText = false;
            return text;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.layer = parent.gameObject.layer;
            RectTransform rect = (RectTransform)obj.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }
    }
}
