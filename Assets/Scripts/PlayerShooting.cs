using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Fires projectiles toward the mouse cursor on Left Mouse Button (new Input System).
/// Top-down 2D: the aim direction is the vector from the player to the mouse's
/// world position.
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Tooltip("Projectile prefab to spawn. Must have the Projectile component.")]
    [SerializeField] private Projectile projectilePrefab;

    [Tooltip("Where projectiles spawn from. If empty, the player's own position is used.")]
    [SerializeField] private Transform firePoint;

    [Tooltip("Minimum seconds between shots.")]
    [SerializeField] private float fireCooldown = 0.2f;

    [Header("Juice")]
    [Tooltip("Camera shake duration when firing.")]
    [SerializeField] private float shootShakeDuration = 0.08f;

    [Tooltip("Camera shake strength when firing.")]
    [SerializeField] private float shootShakeMagnitude = 0.06f;

    private Camera cam;
    private float cooldownTimer = 0f;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        Mouse mouse = Mouse.current;
        if (mouse == null || projectilePrefab == null)
            return;

        if (mouse.leftButton.isPressed && cooldownTimer <= 0f)
        {
            Shoot(mouse.position.ReadValue());
            cooldownTimer = fireCooldown;
        }
    }

    private void Shoot(Vector2 screenPosition)
    {
        if (cam == null)
            cam = Camera.main;
        if (cam == null)
            return;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;

        // Convert the mouse screen position into a world point on the player's plane.
        Vector3 worldPoint = cam.ScreenToWorldPoint(screenPosition);
        worldPoint.z = origin.z;

        Vector2 aimDirection = (Vector2)(worldPoint - origin);
        if (aimDirection.sqrMagnitude < 0.0001f)
            aimDirection = Vector2.right;

        Projectile projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
        projectile.Launch(aimDirection);

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(shootShakeDuration, shootShakeMagnitude);
    }
}
