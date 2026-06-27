using UnityEngine;

/// <summary>Which side fired a projectile, deciding what it can hit.</summary>
public enum ProjectileTeam { Player, Enemy }

/// <summary>
/// A simple top-down 2D projectile. Travels in a fixed direction and, depending
/// on its team, either kills enemies (Player shots) or triggers a game over when
/// it hits the player (Enemy shots).
///
/// Hit detection uses Physics2D sweeps instead of trigger callbacks, so it works
/// regardless of Rigidbody body types or the collision matrix. Targets just need
/// a Collider2D.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Tooltip("Travel speed in units per second.")]
    [SerializeField] private float speed = 12f;

    [Tooltip("Seconds before the projectile self-destructs if it hits nothing.")]
    [SerializeField] private float lifetime = 3f;

    [Tooltip("Radius used to detect targets along the flight path.")]
    [SerializeField] private float hitRadius = 0.15f;

    [Tooltip("Which layers count as hittable. Default = Everything.")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Tooltip("Player shots kill enemies; Enemy shots end the game on hitting the player.")]
    [SerializeField] private ProjectileTeam team = ProjectileTeam.Player;

    [Tooltip("Tag of the player object (used by Enemy-team shots).")]
    [SerializeField] private string playerTag = "Player";

    private Vector2 direction = Vector2.right;

    /// <summary>
    /// Overrides team and speed at spawn time so enemies can reuse the same prefab.
    /// Call before <see cref="Launch"/>.
    /// </summary>
    public void Configure(ProjectileTeam newTeam, float newSpeed)
    {
        team = newTeam;
        speed = newSpeed;
    }

    /// <summary>Aims and arms the projectile. Called by the shooter after spawning.</summary>
    public void Launch(Vector2 aimDirection)
    {
        direction = aimDirection.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        float step = speed * Time.deltaTime;

        // Sweep along this frame's movement so fast projectiles can't tunnel through.
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            transform.position, hitRadius, direction, step, hitMask);

        foreach (RaycastHit2D hit in hits)
        {
            if (team == ProjectileTeam.Player)
            {
                EnemyController enemy = hit.collider.GetComponentInParent<EnemyController>();
                if (enemy != null)
                {
                    enemy.Die();
                    Destroy(gameObject);
                    return;
                }
            }
            else // Enemy shot: only the player matters.
            {
                if (hit.collider.CompareTag(playerTag))
                {
                    // Dodge phases through the shot.
                    if (PlayerController.Instance != null && PlayerController.Instance.IsDodging)
                        continue;

                    GameManager.TriggerGameOver();
                    Destroy(gameObject);
                    return;
                }
            }
        }

        transform.position += (Vector3)(direction * step);
    }
}
