using UnityEngine;

/// <summary>
/// Triangle enemy: relentlessly chases the player and triggers a game over on
/// contact. If the player is dodging (invulnerable), contact is ignored.
/// </summary>
public class TriangleEnemy : EnemyController
{
    [Header("Triangle")]
    [Tooltip("Distance to the player that counts as a touch (game over).")]
    [SerializeField] private float contactRange = 0.6f;

    protected override void Behave()
    {
        MoveInDirection(DirectionToPlayer(), moveSpeed);

        if (DistanceToPlayer() <= contactRange)
        {
            // Dodge grants i-frames.
            if (PlayerController.Instance != null && PlayerController.Instance.IsDodging)
                return;

            GameManager.TriggerGameOver();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, contactRange);
    }
}
