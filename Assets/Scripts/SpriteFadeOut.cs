using UnityEngine;

/// <summary>
/// Fades a SpriteRenderer's alpha to zero over a duration, then destroys the
/// GameObject. Used for the player's dodge afterimages ("ghosts").
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFadeOut : MonoBehaviour
{
    private SpriteRenderer sr;
    private float duration;
    private float elapsed;
    private float startAlpha;

    public void Begin(float fadeDuration)
    {
        sr = GetComponent<SpriteRenderer>();
        duration = Mathf.Max(0.01f, fadeDuration);
        startAlpha = sr.color.a;
    }

    private void Update()
    {
        if (sr == null)
        {
            Destroy(gameObject);
            return;
        }

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        Color c = sr.color;
        c.a = Mathf.Lerp(startAlpha, 0f, t);
        sr.color = c;

        if (t >= 1f)
            Destroy(gameObject);
    }
}
