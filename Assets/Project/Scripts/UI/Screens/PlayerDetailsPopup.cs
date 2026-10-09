using System;
using System.Collections.Generic;
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
        private readonly TMP_Text typingLabel;
        private readonly List<TMP_Text> letterLabels = new List<TMP_Text>();
        private TMP_InputField selectedField;
        private bool uppercase;
        private bool submitting;
        private AdminRegistrationPanel adminPanel;

        public PlayerDetailsPopup(Transform parent, Action onSubmit, GameObject registrationPrefab = null,
            RegistrationCsvStore registrationStore = null)
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
            message = Text("", overlay, new Vector2(-590, 0), new Vector2(430, 75), 26);
            typingLabel = Text("Typing: Name", overlay, new Vector2(590, 0), new Vector2(420, 75), 30);

            RectTransform keyboard = Rect("VR Keyboard", overlay, new Vector2(0, -270), new Vector2(1500, 520));
            string[] rows = { "1234567890", "qwertyuiop", "asdfghjkl", "zxcvbnm", "@._+-'" };
            for (int row = 0; row < rows.Length; row++)
            {
                string keys = rows[row];
                for (int column = 0; column < keys.Length; column++)
                {
                    string key = keys[column].ToString();
                    Button keyButton = Button(key, keyboard,
                        new Vector2((column - (keys.Length - 1) / 2f) * 146, 160 - row * 76),
                        new Vector2(138, 68), () => Type(uppercase ? key.ToUpperInvariant() : key));
                    if (char.IsLetter(key[0]))
                        letterLabels.Add(keyButton.GetComponentInChildren<TMP_Text>());
                }
            }
            Button("Shift", keyboard, new Vector2(-504, -225), new Vector2(240, 64), ToggleCase);
            Button("Space", keyboard, new Vector2(-252, -225), new Vector2(240, 64), () => Type(" "));
            Button("Delete", keyboard, new Vector2(0, -225), new Vector2(240, 64), Delete);
            Button("Clear", keyboard, new Vector2(252, -225), new Vector2(240, 64), () => selectedField.text = "");
            Button("Next", keyboard, new Vector2(504, -225), new Vector2(240, 64), NextField);
            Button("Cancel", overlay, new Vector2(810, -495), new Vector2(240, 64), Hide);
            Hide();
        }

        public void Show()
        {
            if (root.activeSelf) Hide();
            foreach (Transform child in root.transform)
                child.gameObject.SetActive(adminPanel == null || child.gameObject != adminPanel.Root);
            submitting = false;
            nameField.SetTextWithoutNotify("");
            emailField.SetTextWithoutNotify("");
            uppercase = false;
            foreach (TMP_Text label in letterLabels)
                label.text = label.text.ToLowerInvariant();
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
                !validEmail ? "Please enter a valid email address." : "Your details will be saved on this device when you start.";
            startButton.interactable = validName && validEmail && !submitting;
            return validName && validEmail;
        }

        private void SelectField(TMP_InputField field)
        {
            selectedField = field;
            typingLabel.text = field == nameField ? "Typing: Name" : "Typing: Email";
            nameField.targetGraphic.color = field == nameField ? new Color(0.65f, 1f, 1f) : Color.white;
            emailField.targetGraphic.color = field == emailField ? new Color(0.65f, 1f, 1f) : Color.white;
        }

        private void NextField()
        {
            TMP_InputField next = selectedField == nameField ? emailField : nameField;
            SelectField(next);
            next.ActivateInputField();
        }

        private void Type(string value)
        {
            if (selectedField != null && selectedField.text.Length + value.Length <= selectedField.characterLimit)
                selectedField.text += value;
        }

        private void Delete()
        {
            if (selectedField != null && selectedField.text.Length > 0)
                selectedField.text = selectedField.text.Substring(0, selectedField.text.Length - 1);
        }

        private void ToggleCase()
        {
            uppercase = !uppercase;
            foreach (TMP_Text label in letterLabels)
                label.text = uppercase ? label.text.ToUpperInvariant() : label.text.ToLowerInvariant();
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
            try
            {
                registrationStore.Save(nameField.text, emailField.text);
            }
            catch (Exception exception)
            {
                submitting = false;
                startButton.interactable = true;
                message.text = "Could not save your details. Please try Register again.";
                Debug.LogError("Registration could not be saved to " + registrationStore.FilePath + ": " + exception.Message);
                return;
            }
            Debug.Log("Registration saved to " + registrationStore.FilePath);
            Hide();
            onSubmit();
        }

        private static void ConfigureField(TMP_InputField field, int limit)
        {
            field.characterLimit = limit;
            field.lineType = TMP_InputField.LineType.SingleLine;
            field.richText = false;
            field.shouldHideSoftKeyboard = true;
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
