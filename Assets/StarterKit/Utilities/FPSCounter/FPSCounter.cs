using CommanTickManager;
using StarterKit;
using TMPro;
using UnityEngine;

public class FPSCounter : Singleton<FPSCounter>, ITick
{
	[SerializeField] private float _hudRefreshRate = 0.2f; // Refresh more frequently (e.g., every 0.2s)
	[SerializeField] private Canvas canvas;
	[SerializeField] private TMP_Text fpsText;

	private float _timer;

	public void EnableFPS(bool isActive)
	{
		canvas.enabled = isActive;
		if (isActive)
		{
			ProcessingUpdate.Instance.Add(this);
		}
		else
		{
			ProcessingUpdate.Instance.Remove(this);
		}

	}

	public void Tick()
	{
		if (Time.unscaledTime > _timer)
		{
			int fps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime); // Current FPS
			fpsText.text = "<color=white>FPS : </color>" + fps;

			_timer = Time.unscaledTime + _hudRefreshRate;
		}
	}
}
