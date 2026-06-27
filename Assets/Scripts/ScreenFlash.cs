using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Flashes a full-screen color overlay that fades out, used to "discolor the
/// world" briefly (e.g. to confirm a kill). The overlay canvas and image are
/// created in code, so the only setup is adding this component to a GameObject.
///
/// Call ScreenFlash.Instance.Flash(color, duration) from anywhere.
/// </summary>
public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash Instance { get; private set; }

    private Image overlay;
    private Coroutine routine;

    private void Awake()
    {
        Instance = this;
        BuildOverlay();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void BuildOverlay()
    {
        GameObject canvasGo = new GameObject("ScreenFlashCanvas");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // draw on top of everything

        GameObject imageGo = new GameObject("FlashImage");
        imageGo.transform.SetParent(canvasGo.transform, false);

        overlay = imageGo.AddComponent<Image>();
        overlay.raycastTarget = false; // never block clicks

        // Stretch to fill the screen.
        RectTransform rt = overlay.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        overlay.color = new Color(1f, 1f, 1f, 0f); // start invisible
    }

    /// <summary>
    /// Fades the given color in instantly and out over <paramref name="duration"/> seconds.
    /// The color's alpha sets the peak intensity.
    /// </summary>
    public void Flash(Color color, float duration)
    {
        if (overlay == null)
            return;

        if (routine != null)
            StopCoroutine(routine);
        routine = StartCoroutine(FlashRoutine(color, duration));
    }

    private IEnumerator FlashRoutine(Color color, float duration)
    {
        float peakAlpha = color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = 1f - Mathf.Clamp01(elapsed / duration);
            overlay.color = new Color(color.r, color.g, color.b, peakAlpha * t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        overlay.color = new Color(color.r, color.g, color.b, 0f);
        routine = null;
    }
}
