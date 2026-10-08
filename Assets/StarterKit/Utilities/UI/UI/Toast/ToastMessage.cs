using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    public class ToastMessage : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI toastMessage;
        [SerializeField] private CanvasGroup toast;

        [SerializeField] RectTransform rectTransform;


        public void SetToast(Vector3 position, string message, float duration = 1f)
        {
            Vector3 spawnPoint = position;
            spawnPoint.y += 1f;
            spawnPoint.x = 0;

            toastMessage.text = message;
            transform.position = spawnPoint;

            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y, 0);

            transform.DOMoveY(transform.position.y + 0.5f, duration);
            toast.DOFade(0, duration).OnComplete(() => Destroy(gameObject));
        }

        // New method with offset support
        public void SetToastWithOffset(Vector2 offset, string message, float duration = 1f)
        {
            // Get screen center and apply offset
            Vector3 screenCenter = new Vector3(UnityEngine.Screen.width / 2f, UnityEngine.Screen.height / 2f, 0);
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenCenter);

            // Apply the offset
            Vector3 spawnPoint = new Vector3(worldPosition.x + offset.x, worldPosition.y + offset.y, 0);

            toastMessage.text = message;
            transform.position = spawnPoint;

            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y, 0);

            transform.DOMoveY(transform.position.y + 0.5f, duration);
            toast.DOFade(0, duration).OnComplete(() => Destroy(gameObject));
        }

        // For UI Canvas positioning (Recommended approach)
        public void SetToastAnchoredPosition(Vector2 anchoredOffset, string message, float duration = 1f, Color? textColor = null)
        {
            toastMessage.text = message;

            if (textColor.HasValue)
                toastMessage.color = textColor.Value;

            rectTransform.anchoredPosition = anchoredOffset;
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y, 0);

            Vector2 targetPosition = rectTransform.anchoredPosition + Vector2.up * 50f;
            rectTransform.DOAnchorPos(targetPosition, duration);
            toast.DOFade(0, duration).OnComplete(() => Destroy(gameObject));
        }

        public void SetToastFade(Vector3 position, string message, float duration = 1f)
        {
            Vector3 spawnPoint = position;
            spawnPoint.y += 1f;
            spawnPoint.x = 0;

            toastMessage.text = message;
            transform.position = spawnPoint;

            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y, 0);

            toast.DOFade(0, duration).OnComplete(() => Destroy(gameObject));
        }

        // New fade method with offset
        public void SetToastFadeWithOffset(Vector2 anchoredOffset, string message, float duration = 1f)
        {
            toastMessage.text = message;
            rectTransform.anchoredPosition = anchoredOffset;
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y, 0);

            toast.DOFade(0, duration).OnComplete(() => Destroy(gameObject));
        }
    }
}