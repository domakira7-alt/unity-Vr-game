using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    // Measures the actual message instead of relying on the original short-text rectangle.
    public sealed class OutcomeMessageLayout
    {
        private readonly TMP_Text text;
        private readonly RectTransform frame;
        private readonly RectTransform parent;
        private readonly RectTransform banner;
        private readonly RectTransform continueButton;
        private readonly RectTransform exitButton;
        private readonly RectTransform viewport;
        private readonly ScrollRect scroll;
        private readonly Button up;
        private readonly Button down;

        public OutcomeMessageLayout(TMP_Text text, Image banner, Button continueButton, Button exitButton)
        {
            this.text = text;
            frame = (RectTransform)text.transform.parent;
            // A separately scaling frame makes ScrollRect calculate bounds at zero scale.
            // Keep this panel stable while the existing banner/buttons animate into place.
            foreach (Scale animation in frame.GetComponents<Scale>())
            {
                animation.StopAnimate();
                animation.fromScale = animation.toScale = animation.finalScale = Vector3.one;
            }
            frame.localScale = Vector3.one;
            parent = (RectTransform)frame.parent;
            this.banner = banner.rectTransform;
            this.continueButton = (RectTransform)continueButton.transform;
            this.exitButton = (RectTransform)exitButton.transform;
            viewport = Rect("Message Viewport", frame);
            viewport.gameObject.AddComponent<RectMask2D>();
            Image surface = viewport.gameObject.AddComponent<Image>();
            surface.color = Color.clear;
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.inertia = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 80;
            text.transform.SetParent(viewport, false);
            text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(0.5f, 1);
            text.rectTransform.pivot = new Vector2(0.5f, 1);
            text.rectTransform.anchoredPosition = Vector2.zero;
            text.enableAutoSizing = false;
            text.fontSize = 40;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.paragraphSpacing = 18;
            // The prefab's outlined font extends beyond the glyph advance; leave an inset for it.
            text.margin = new Vector4(16, 12, 16, 12);
            text.raycastTarget = false;
            scroll.viewport = viewport;
            scroll.content = text.rectTransform;
            up = ScrollButton("Haut", continueButton, -1);
            down = ScrollButton("Bas", continueButton, 1);
            scroll.onValueChanged.AddListener(_ => UpdateControls());
        }

        public void Refresh()
        {
            float canvasWidth = parent.rect.width > 0 ? parent.rect.width : 1920;
            float canvasHeight = parent.rect.height > 0 ? parent.rect.height : 1080;
            float width = Mathf.Min(1560, canvasWidth - 160);
            float textWidth = width - 144;
            float textHeight = Mathf.Ceil(text.GetPreferredValues(text.text, textWidth - 32, Mathf.Infinity).y) + 40;
            float height = Mathf.Clamp(textHeight + 112, 320, Mathf.Max(320, canvasHeight - 380));
            Place(frame, Vector2.zero, new Vector2(width, height));
            Place(viewport, Vector2.zero, new Vector2(textWidth, height - 112));
            text.rectTransform.sizeDelta = new Vector2(textWidth, textHeight);
            text.rectTransform.anchoredPosition = Vector2.zero;
            Place(banner, new Vector2(0, height / 2 + 90), new Vector2(1000, 120));
            float buttonY = -height / 2 - 80;
            Place(continueButton, new Vector2(width / 2 - 290, buttonY), new Vector2(520, 110));
            Place(exitButton, new Vector2(-width / 2 + 290, buttonY), new Vector2(520, 110));
            Place((RectTransform)up.transform, new Vector2(-68, buttonY), new Vector2(120, 90));
            Place((RectTransform)down.transform, new Vector2(68, buttonY), new Vector2(120, 90));
            bool overflow = textHeight > viewport.rect.height + 1;
            up.gameObject.SetActive(overflow);
            down.gameObject.SetActive(overflow);
            scroll.vertical = overflow;
            scroll.enabled = overflow;
            scroll.StopMovement();
            text.ForceMeshUpdate(true);
            text.rectTransform.anchoredPosition = Vector2.zero;
            UpdateControls();
        }

        private void Scroll(float direction)
        {
            float maximum = Mathf.Max(0, text.rectTransform.rect.height - viewport.rect.height);
            scroll.StopMovement();
            text.rectTransform.anchoredPosition = new Vector2(0,
                Mathf.Clamp(text.rectTransform.anchoredPosition.y + direction * viewport.rect.height * 0.8f, 0, maximum));
            UpdateControls();
        }

        private void UpdateControls()
        {
            float maximum = Mathf.Max(0, text.rectTransform.rect.height - viewport.rect.height);
            up.interactable = text.rectTransform.anchoredPosition.y > 0.5f;
            down.interactable = text.rectTransform.anchoredPosition.y < maximum - 0.5f;
        }

        private Button ScrollButton(string label, Button template, float direction)
        {
            RectTransform rect = Rect("Message " + label, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            if (template.targetGraphic is Image source) image.sprite = source.sprite;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => Scroll(direction));
            var labelText = Rect("Label", rect).gameObject.AddComponent<TextMeshProUGUI>();
            labelText.font = text.font;
            labelText.fontSize = 24;
            labelText.text = label;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.raycastTarget = false;
            Place(labelText.rectTransform, Vector2.zero, new Vector2(120, 90));
            return button;
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.layer = parent.gameObject.layer;
            var rect = (RectTransform)obj.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            // Preserve each slide's travel distance, but move its destination with the layout.
            foreach (Translate animation in rect.GetComponents<Translate>())
            {
                Vector3 oldDestination = animation.animationType == UIAnimationType.Show
                    ? animation.toPosition : animation.fromPosition;
                Vector3 delta = (Vector3)position - oldDestination;
                animation.fromPosition += delta;
                animation.toPosition += delta;
                animation.finalPosition += delta;
            }
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
