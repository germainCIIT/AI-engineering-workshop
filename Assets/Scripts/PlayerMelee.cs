using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Right Mouse Button performs a melee cone swing aimed at the mouse cursor,
/// killing every enemy inside the cone within range. A fading wedge is drawn
/// with a LineRenderer so you can see exactly where you swung.
/// </summary>
public class PlayerMelee : MonoBehaviour
{
    [Tooltip("How far the swing reaches, in world units.")]
    [SerializeField] private float meleeRange = 2f;

    [Tooltip("Total width of the swing cone in degrees.")]
    [Range(0f, 360f)]
    [SerializeField] private float meleeAngle = 90f;

    [Tooltip("Seconds between swings.")]
    [SerializeField] private float cooldown = 0.5f;

    [Tooltip("Which layers the swing can hit. Default = Everything.")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Swing Visual")]
    [SerializeField] private Color swingColor = new Color(1f, 1f, 0.6f, 0.9f);
    [SerializeField] private float swingVisualDuration = 0.15f;
    [SerializeField] private int swingSegments = 16;
    [SerializeField] private float swingLineWidth = 0.08f;

    [Header("Juice")]
    [SerializeField] private float shakeDuration = 0.12f;
    [SerializeField] private float shakeMagnitude = 0.12f;

    private Camera cam;
    private float cooldownTimer;
    private LineRenderer swingLine;
    private Coroutine swingRoutine;

    private void Awake()
    {
        cam = Camera.main;
        BuildSwingVisual();
    }

    private void BuildSwingVisual()
    {
        GameObject visual = new GameObject("MeleeSwingVisual");
        visual.transform.SetParent(transform, false);

        swingLine = visual.AddComponent<LineRenderer>();
        swingLine.useWorldSpace = true;
        swingLine.loop = false;
        swingLine.widthMultiplier = swingLineWidth;
        swingLine.numCapVertices = 2;
        swingLine.material = new Material(Shader.Find("Sprites/Default"));
        swingLine.sortingOrder = 50;
        swingLine.enabled = false;
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.rightButton.wasPressedThisFrame && cooldownTimer <= 0f)
        {
            Swing(mouse.position.ReadValue());
            cooldownTimer = cooldown;
        }
    }

    private void Swing(Vector2 screenPosition)
    {
        Vector2 aim = AimDirection(screenPosition);

        Collider2D[] candidates = Physics2D.OverlapCircleAll(transform.position, meleeRange, hitMask);
        foreach (Collider2D col in candidates)
        {
            EnemyController enemy = col.GetComponentInParent<EnemyController>();
            if (enemy == null)
                continue;

            Vector2 toEnemy = (Vector2)(enemy.transform.position - transform.position);
            if (Vector2.Angle(aim, toEnemy) <= meleeAngle * 0.5f)
                enemy.Die();
        }

        ShowSwing(aim);

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);
    }

    private void ShowSwing(Vector2 aim)
    {
        if (swingLine == null)
            return;

        float baseAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        float half = meleeAngle * 0.5f;
        Vector3 center = transform.position;

        // Points: center -> arc(0..segments) -> center, drawing the wedge outline.
        swingLine.positionCount = swingSegments + 3;
        swingLine.SetPosition(0, center);
        for (int i = 0; i <= swingSegments; i++)
        {
            float a = (baseAngle - half + meleeAngle * i / swingSegments) * Mathf.Deg2Rad;
            Vector3 point = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * meleeRange;
            swingLine.SetPosition(i + 1, point);
        }
        swingLine.SetPosition(swingSegments + 2, center);

        if (swingRoutine != null)
            StopCoroutine(swingRoutine);
        swingRoutine = StartCoroutine(FadeSwing());
    }

    private IEnumerator FadeSwing()
    {
        swingLine.enabled = true;
        float elapsed = 0f;

        while (elapsed < swingVisualDuration)
        {
            float a = swingColor.a * (1f - elapsed / swingVisualDuration);
            Color c = new Color(swingColor.r, swingColor.g, swingColor.b, a);
            swingLine.startColor = c;
            swingLine.endColor = c;

            elapsed += Time.deltaTime;
            yield return null;
        }

        swingLine.enabled = false;
        swingRoutine = null;
    }

    private Vector2 AimDirection(Vector2 screenPosition)
    {
        if (cam == null)
            cam = Camera.main;
        if (cam == null)
            return Vector2.right;

        Vector3 worldPoint = cam.ScreenToWorldPoint(screenPosition);
        worldPoint.z = transform.position.z;

        Vector2 dir = (Vector2)(worldPoint - transform.position);
        return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}
