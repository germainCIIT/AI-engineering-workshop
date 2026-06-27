using UnityEngine;

/// <summary>
/// Square enemy: a kiter. Tries to hold a preferred distance from the player
/// (backing away if too close, approaching if too far) and shoots slow
/// projectiles at a reduced rate.
/// </summary>
public class SquareEnemy : EnemyController
{
    [Header("Kiting")]
    [Tooltip("The distance the square tries to keep from the player.")]
    [SerializeField] private float preferredDistance = 5f;

    [Tooltip("Dead zone around the preferred distance where the square holds position.")]
    [SerializeField] private float distanceBuffer = 0.75f;

    [Header("Shooting")]
    [Tooltip("Projectile prefab to fire (needs the Projectile component). Can reuse the player's bullet.")]
    [SerializeField] private Projectile projectilePrefab;

    [Tooltip("Seconds between shots (higher = slower fire rate).")]
    [SerializeField] private float fireInterval = 2f;

    [Tooltip("Projectile travel speed (kept low so the player can dodge).")]
    [SerializeField] private float projectileSpeed = 5f;

    private float fireTimer;

    protected override void Behave()
    {
        float distance = DistanceToPlayer();
        Vector2 toPlayer = DirectionToPlayer();

        if (distance < preferredDistance - distanceBuffer)
            MoveInDirection(-toPlayer, moveSpeed);      // too close: back away
        else if (distance > preferredDistance + distanceBuffer)
            MoveInDirection(toPlayer, moveSpeed);       // too far: close in
        // else: within the comfort band, hold position

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Shoot(toPlayer);
            fireTimer = fireInterval;
        }
    }

    private void Shoot(Vector2 direction)
    {
        if (projectilePrefab == null)
            return;

        Projectile projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectile.Configure(ProjectileTeam.Enemy, projectileSpeed);
        projectile.Launch(direction);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, preferredDistance);
    }
}
