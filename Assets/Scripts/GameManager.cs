using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Tracks game-over state. Call GameManager.TriggerGameOver() from anything that
/// should end the run (one hit is fatal: a triangle touching the player, or an
/// enemy shot landing). On game over it flashes the screen, shows an on-screen
/// prompt, pauses the game, and waits for R to restart the current scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Tooltip("Screen flash color shown on game over (alpha = intensity).")]
    [SerializeField] private Color gameOverFlashColor = new Color(0.6f, 0f, 0f, 0.7f);

    [SerializeField] private float gameOverFlashDuration = 0.6f;

    private bool isGameOver;
    private GameObject gameOverUI;

    private void Awake()
    {
        Instance = this;
        BuildGameOverUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void BuildGameOverUI()
    {
        GameObject canvasGo = new GameObject("GameOverCanvas");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000; // above the screen flash

        GameObject textGo = new GameObject("GameOverText");
        textGo.transform.SetParent(canvasGo.transform, false);

        Text text = textGo.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 64;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = "GAME OVER\n<press R to restart>";

        RectTransform rt = text.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        gameOverUI = canvasGo;
        gameOverUI.SetActive(false);
    }

    public static void TriggerGameOver()
    {
        // Fall back to a scene search in case Instance wasn't assigned yet
        // (script execution order, or a GameManager that started inactive).
        if (Instance == null)
            Instance = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);

        if (Instance != null)
            Instance.GameOver();
        else
            Debug.LogWarning("No GameManager found in the scene; add a GameManager component to an active GameObject.");
    }

    public void GameOver()
    {
        if (isGameOver)
            return;
        isGameOver = true;

        Debug.Log("GAME OVER - press R to restart.");

        if (ScreenFlash.Instance != null)
            ScreenFlash.Instance.Flash(gameOverFlashColor, gameOverFlashDuration);

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (!isGameOver)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
