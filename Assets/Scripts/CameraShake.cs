using System.Collections;
using UnityEngine;

/// <summary>
/// Simple positional camera shake. Attach to the Main Camera and call
/// CameraShake.Instance.Shake(duration, magnitude) from anywhere.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 originalLocalPos;
    private Coroutine routine;

    private void Awake()
    {
        Instance = this;
        originalLocalPos = transform.localPosition;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Shakes the camera for <paramref name="duration"/> seconds, starting at
    /// <paramref name="magnitude"/> units of offset and easing out to zero.
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        if (routine != null)
            StopCoroutine(routine);
        routine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Ease the shake out over its lifetime so it settles smoothly.
            float damper = 1f - Mathf.Clamp01(elapsed / duration);
            float x = (Random.value * 2f - 1f) * magnitude * damper;
            float y = (Random.value * 2f - 1f) * magnitude * damper;

            transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalLocalPos;
        routine = null;
    }
}
