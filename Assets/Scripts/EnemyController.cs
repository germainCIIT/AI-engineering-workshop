using System.Collections;
using UnityEngine;

/// <summary>
/// Abstract base for enemies. Handles the shared concerns: finding the player,
/// rotating the sprite to face the player every frame, and dying with juice
/// (camera shake + screen flash). Concrete enemies implement <see cref="Behave"/>.
///
/// Detection/FOV is gone: enemies now know where the player is and act on it
/// relentlessly. Add a concrete subclass (TriangleEnemy / SquareEnemy) to a
/// GameObject, not this class directly.
/// </summary>
public abstract class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in units per second.")]
    [SerializeField] protected float moveSpeed = 3f;

    [Header("Facing")]
    [Tooltip("Rotation offset in degrees. Use -90 if the sprite points UP, 0 if it points RIGHT.")]
    [SerializeField] private float spriteForwardOffset = -90f;

    [Header("References")]
    [Tooltip("Tag used to find the player object.")]
    [SerializeField] protected string playerTag = "Player";

    [Header("Death Juice")]
    [Tooltip("Color the screen flashes when this enemy is killed (alpha = intensity).")]
    [SerializeField] private Color killFlashColor = new Color(1f, 0.25f, 0.25f, 0.5f);

    [SerializeField] private float killFlashDuration = 0.25f;
    [SerializeField] private float killShakeDuration = 0.25f;
    [SerializeField] private float killShakeMagnitude = 0.3f;

    [Header("Spawn Animation")]
    [Tooltip("How long the spawn pop-in (scale up) takes, in seconds.")]
    [SerializeField] private float spawnAnimDuration = 0.25f;

    [Header("Scoring")]
    [Tooltip("Points awarded to the player for killing this enemy.")]
    [SerializeField] private int scoreValue = 1;

    protected Transform player;

    protected virtual void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
            player = playerObject.transform;
        else
            Debug.LogWarning($"{name}: No GameObject tagged '{playerTag}' found.", this);

        StartCoroutine(SpawnPopIn());
    }

    /// <summary>Scales the enemy up from zero with a slight overshoot when it spawns.</summary>
    private IEnumerator SpawnPopIn()
    {
        Vector3 targetScale = transform.localScale;
        float elapsed = 0f;

        transform.localScale = Vector3.zero;
        while (elapsed < spawnAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spawnAnimDuration);
            transform.localScale = targetScale * EaseOutBack(t);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    protected virtual void Update()
    {
        if (player == null)
            return;

        FacePlayer();
        Behave();
    }

    /// <summary>Type-specific behavior, run every frame while the player exists.</summary>
    protected abstract void Behave();

    /// <summary>Rotates the sprite so it points at the player.</summary>
    protected void FacePlayer()
    {
        Vector2 dir = (Vector2)(player.position - transform.position);
        if (dir.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + spriteForwardOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    protected float DistanceToPlayer() => Vector2.Distance(transform.position, player.position);

    protected Vector2 DirectionToPlayer()
    {
        Vector2 d = (Vector2)(player.position - transform.position);
        return d.sqrMagnitude > 0.0001f ? d.normalized : Vector2.right;
    }

    protected void MoveInDirection(Vector2 direction, float speed)
    {
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
    }

    /// <summary>
    /// Eliminates the enemy: triggers kill juice (shake + screen flash), then removes it.
    /// </summary>
    public void Die()
    {
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(killShakeDuration, killShakeMagnitude);

        if (ScreenFlash.Instance != null)
            ScreenFlash.Instance.Flash(killFlashColor, killFlashDuration);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddKill(scoreValue);

        Destroy(gameObject);
    }
}
