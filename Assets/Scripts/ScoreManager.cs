using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tracks the kill score and shows it in a self-built UI label that "pops"
/// (scale punch) on every kill. Call ScoreManager.Instance.AddKill(points).
/// Just add this component to a GameObject; the canvas is created in code.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 36;

    [Header("Pop")]
    [SerializeField] private float popScale = 1.4f;
    [SerializeField] private float popDuration = 0.18f;

    private int score;
    private Text label;
    private Coroutine popRoutine;

    private void Awake()
    {
        Instance = this;
        BuildUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void BuildUI()
    {
        GameObject canvasGo = new GameObject("ScoreCanvas");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        GameObject textGo = new GameObject("ScoreText");
        textGo.transform.SetParent(canvasGo.transform, false);

        label = textGo.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.fontStyle = FontStyle.Bold;
        label.color = textColor;
        label.alignment = TextAnchor.UpperLeft;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.text = "Score: 0";

        RectTransform rt = label.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -20f);
        rt.sizeDelta = new Vector2(400f, 60f);
    }

    public void AddKill(int points)
    {
        score += points;
        if (label != null)
            label.text = $"Score: {score}";

        if (popRoutine != null)
            StopCoroutine(popRoutine);
        popRoutine = StartCoroutine(Pop());
    }

    private IEnumerator Pop()
    {
        if (label == null)
            yield break;

        Transform t = label.transform;
        Vector3 big = Vector3.one * popScale;
        float elapsed = 0f;

        while (elapsed < popDuration)
        {
            // Use unscaled time so the pop plays even if the game is paused.
            elapsed += Time.unscaledDeltaTime;
            t.localScale = Vector3.Lerp(big, Vector3.one, elapsed / popDuration);
            yield return null;
        }

        t.localScale = Vector3.one;
        popRoutine = null;
    }
}
