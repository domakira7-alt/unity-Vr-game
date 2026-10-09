using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    // Shares the menu's world-space canvas and controller ray interaction.
    public sealed class AdminRegistrationPanel
    {
        public GameObject Root { get; }
        private readonly GameObject menu;
        private readonly GameObject table;
        private readonly RectTransform viewport;
        private readonly RectTransform rows;
        private readonly ScrollRect scroll;
        private readonly TMP_Text status;
        private readonly TMP_FontAsset font;
        private readonly Sprite buttonSprite;
        private readonly RegistrationCsvStore store;
        private readonly Button startButton;
        private readonly Button upButton;
        private readonly Button downButton;
        private bool started;
        private const float NameX = -580;
        private const float NameWidth = 420;
        private const float EmailX = 10;
        private const float EmailWidth = 760;
        private const float DateX = 590;
        private const float DateWidth = 400;

        public AdminRegistrationPanel(Transform parent, TMP_FontAsset font, Sprite buttonSprite,
            RegistrationCsvStore store, Action start, Action logout)
        {
            this.font = font;
            this.buttonSprite = buttonSprite;
            this.store = store;
            Root = Rect("Admin Panel", parent, Vector2.zero, new Vector2(1720, 980)).gameObject;
            Root.AddComponent<Image>().color = new Color(0.035f, 0.18f, 0.23f, 0.97f);
            Panel("Top Accent", Root.transform, new Vector2(0, 488), new Vector2(1720, 4), new Color(0.1f, 0.9f, 0.95f));

            menu = Rect("Admin Menu", Root.transform, Vector2.zero, new Vector2(1720, 980)).gameObject;
            Text("ADMIN", menu.transform, new Vector2(0, 300), new Vector2(1500, 100), 54);
            Text("Choose an action", menu.transform, new Vector2(0, 180), new Vector2(1500, 80), 32);
            startButton = Button("Start", menu.transform, new Vector2(0, 40), new Vector2(650, 100), () =>
            {
                if (started) return;
                started = true;
                startButton.interactable = false;
                start();
            });
            Button("View registrations", menu.transform, new Vector2(0, -110), new Vector2(650, 100), ShowRegistrations);
            Button("Log out", menu.transform, new Vector2(0, -330), new Vector2(420, 90), logout);

            table = Rect("Registrations Table", Root.transform, Vector2.zero, new Vector2(1720, 980)).gameObject;
            Text("REGISTRATIONS", table.transform, new Vector2(0, 415), new Vector2(1500, 80), 46);
            status = Text("", table.transform, new Vector2(0, 350), new Vector2(1500, 50), 24);
            status.color = new Color(0.65f, 0.82f, 0.86f);
            RectTransform header = Panel("Table Header", table.transform, new Vector2(0, 285),
                new Vector2(1580, 70), new Color(0.06f, 0.32f, 0.39f));
            HeaderCell("NAME", header, NameX, NameWidth);
            HeaderCell("EMAIL", header, EmailX, EmailWidth);
            HeaderCell("DATE / TIME", header, DateX, DateWidth);
            Dividers(header, 70);

            viewport = Rect("Table Viewport", table.transform, new Vector2(0, -55), new Vector2(1580, 610));
            viewport.gameObject.AddComponent<Image>().color = new Color(0.02f, 0.1f, 0.14f, 0.95f);
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = false;
            scroll.scrollSensitivity = 80;
            rows = Rect("Registration Rows", viewport, Vector2.zero, Vector2.zero);
            rows.anchorMin = new Vector2(0, 1);
            rows.anchorMax = new Vector2(1, 1);
            rows.pivot = new Vector2(0.5f, 1);
            scroll.viewport = viewport;
            scroll.content = rows;
            scroll.onValueChanged.AddListener(_ => UpdateScrollButtons());
            Button("Back", table.transform, new Vector2(-590, -410), new Vector2(300, 85), ShowMenu);
            Button("Refresh", table.transform, new Vector2(-245, -410), new Vector2(300, 85), ShowRegistrations);
            upButton = Button("Up", table.transform, new Vector2(245, -410), new Vector2(300, 85), () => ScrollBy(-450));
            downButton = Button("Down", table.transform, new Vector2(590, -410), new Vector2(300, 85), () => ScrollBy(450));
            Root.SetActive(false);
        }

        public void Show()
        {
            started = false;
            startButton.interactable = true;
            Root.SetActive(true);
            ShowMenu();
        }

        private void ShowMenu()
        {
            menu.SetActive(true);
            table.SetActive(false);
        }

        private void ShowRegistrations()
        {
            menu.SetActive(false);
            table.SetActive(true);
            for (int i = rows.childCount - 1; i >= 0; i--)
            {
                Transform child = rows.GetChild(i);
                child.gameObject.SetActive(false);
                if (Application.isPlaying) UnityEngine.Object.Destroy(child.gameObject);
                else UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
            rows.sizeDelta = Vector2.zero;
            rows.anchoredPosition = Vector2.zero;
            try
            {
                var records = store.ReadAll();
                status.text = records.Count == 0 ? "No registrations yet." : records.Count + " registrations · newest first";
                float top = 0;
                for (int i = records.Count - 1; i >= 0; i--)
                {
                    RegistrationRecord record = records[i];
                    RectTransform row = Rect("Registration Row", rows, Vector2.zero, new Vector2(1580, 90));
                    row.anchorMin = row.anchorMax = new Vector2(0.5f, 1);
                    row.pivot = new Vector2(0.5f, 1);
                    row.anchoredPosition = new Vector2(0, -top);
                    row.gameObject.AddComponent<Image>().color = ((records.Count - 1 - i) % 2 == 0)
                        ? new Color(0.055f, 0.22f, 0.28f, 0.95f) : new Color(0.04f, 0.17f, 0.22f, 0.95f);
                    TMP_Text name = Cell(record.Name, row, NameX, 0, NameWidth, 98, 28);
                    TMP_Text email = Cell(record.Email, row, EmailX, 0, EmailWidth, 98, 28);
                    float height = Mathf.Max(98, name.GetPreferredValues(record.Name, NameWidth - 48, Mathf.Infinity).y + 32,
                        email.GetPreferredValues(record.Email, EmailWidth - 48, Mathf.Infinity).y + 32);
                    row.sizeDelta = new Vector2(1580, height);
                    PositionCell(name, height);
                    PositionCell(email, height);
                    string date = record.RegisteredAtUtc.ToLocalTime().ToString("dd MMM yyyy\nHH:mm:ss", CultureInfo.InvariantCulture);
                    TMP_Text dateText = Cell(date, row, DateX, 0, DateWidth, height, 24);
                    dateText.color = new Color(0.7f, 0.85f, 0.88f);
                    PositionCell(dateText, height);
                    Dividers(row, height);
                    Panel("Row Separator", row, new Vector2(0, -height / 2 + 0.5f),
                        new Vector2(1580, 1), new Color(0.2f, 0.48f, 0.53f, 0.5f));
                    top += height;
                }
                rows.sizeDelta = new Vector2(0, top);
            }
            catch (Exception exception)
            {
                status.text = "Could not read registrations. Please try Refresh.";
                Debug.LogError("Could not read " + store.FilePath + ": " + exception.Message);
            }
            Canvas.ForceUpdateCanvases();
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1;
            UpdateScrollButtons();
        }

        private static void PositionCell(TMP_Text text, float height)
        {
            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -height / 2);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height - 12);
        }

        private void ScrollBy(float distance)
        {
            scroll.StopMovement();
            float maximum = Mathf.Max(0, rows.rect.height - viewport.rect.height);
            rows.anchoredPosition = new Vector2(0, Mathf.Clamp(rows.anchoredPosition.y + distance, 0, maximum));
            UpdateScrollButtons();
        }

        private void UpdateScrollButtons()
        {
            float maximum = Mathf.Max(0, rows.rect.height - viewport.rect.height);
            if (upButton != null) upButton.interactable = rows.anchoredPosition.y > 0.5f;
            if (downButton != null) downButton.interactable = rows.anchoredPosition.y < maximum - 0.5f;
        }

        private TMP_Text Cell(string value, Transform parent, float x, float y, float width, float height, float size)
        {
            TMP_Text text = Text(value, parent, new Vector2(x, y), new Vector2(width, height), size);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.margin = new Vector4(24, 0, 24, 0);
            text.enableWordWrapping = true;
            return text;
        }

        private void HeaderCell(string value, Transform parent, float x, float width)
        {
            TMP_Text text = Cell(value, parent, x, 0, width, 70, 26);
            text.color = new Color(0.4f, 0.95f, 1f);
        }

        private static void Dividers(Transform parent, float height)
        {
            Color color = new Color(0.2f, 0.48f, 0.53f, 0.45f);
            Panel("Name Divider", parent, new Vector2(-370, 0), new Vector2(1, height), color);
            Panel("Email Divider", parent, new Vector2(390, 0), new Vector2(1, height), color);
        }

        private static RectTransform Panel(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            RectTransform rect = Rect(name, parent, position, size);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private TMP_Text Text(string value, Transform parent, Vector2 position, Vector2 size, float fontSize)
        {
            var text = Rect("Text", parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = fontSize;
            text.text = value;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            text.richText = false;
            return text;
        }

        private Button Button(string label, Transform parent, Vector2 position, Vector2 size, Action action)
        {
            RectTransform rect = Rect(label, parent, position, size);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = buttonSprite;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => action());
            Text(label, rect, Vector2.zero, size, 34);
            return button;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.layer = parent.gameObject.layer;
            var rect = (RectTransform)obj.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }
    }
}
