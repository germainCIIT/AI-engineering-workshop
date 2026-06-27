using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Top-down 2D player movement (new Input System) with a Left Shift dodge.
///
/// WASD / arrows / left stick move on the XY plane. Left Shift performs a swift
/// dash in the current move direction (or the last one if standing still), with
/// brief invulnerability (i-frames) exposed via <see cref="IsDodging"/>.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement")]
    [Tooltip("Movement speed in units per second.")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dodge")]
    [Tooltip("Speed during the dodge dash.")]
    [SerializeField] private float dodgeSpeed = 18f;

    [Tooltip("How long the dodge lasts, in seconds.")]
    [SerializeField] private float dodgeDuration = 0.15f;

    [Tooltip("Seconds before the dodge can be used again.")]
    [SerializeField] private float dodgeCooldown = 0.8f;

    [Header("Dodge Ghosting")]
    [Tooltip("Seconds between afterimage spawns during a dodge.")]
    [SerializeField] private float ghostInterval = 0.025f;

    [Tooltip("How long each afterimage takes to fade out.")]
    [SerializeField] private float ghostFadeDuration = 0.3f;

    [Tooltip("Tint and starting opacity of the dodge afterimages.")]
    [SerializeField] private Color ghostColor = new Color(0.5f, 0.8f, 1f, 0.5f);

    /// <summary>True while dodging; used by enemies/projectiles as i-frames.</summary>
    public bool IsDodging { get; private set; }

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.right;

    private float dodgeTimer;
    private float dodgeCooldownTimer;
    private Vector2 dodgeDir;
    private float ghostTimer;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        moveInput = ReadInput();
        if (moveInput.sqrMagnitude > 0.0001f)
            lastMoveDir = moveInput.normalized;

        dodgeCooldownTimer -= Time.deltaTime;

        Keyboard keyboard = Keyboard.current;
        bool dodgePressed = keyboard != null && keyboard.leftShiftKey.wasPressedThisFrame;
        if (dodgePressed && !IsDodging && dodgeCooldownTimer <= 0f)
            StartDodge();

        if (IsDodging)
        {
            ghostTimer -= Time.deltaTime;
            if (ghostTimer <= 0f)
            {
                SpawnGhost();
                ghostTimer = ghostInterval;
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 delta;

        if (IsDodging)
        {
            delta = dodgeDir * dodgeSpeed * Time.fixedDeltaTime;

            dodgeTimer -= Time.fixedDeltaTime;
            if (dodgeTimer <= 0f)
                IsDodging = false;
        }
        else
        {
            delta = moveInput * moveSpeed * Time.fixedDeltaTime;
        }

        if (rb != null)
            rb.MovePosition(rb.position + delta);
        else
            transform.position += (Vector3)delta;
    }

    private void StartDodge()
    {
        IsDodging = true;
        dodgeTimer = dodgeDuration;
        dodgeCooldownTimer = dodgeCooldown;
        dodgeDir = (moveInput.sqrMagnitude > 0.0001f ? moveInput : lastMoveDir).normalized;

        ghostTimer = 0f;
        SpawnGhost();
    }

    /// <summary>Spawns a fading copy of the player's sprite as a dodge afterimage.</summary>
    private void SpawnGhost()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        GameObject ghost = new GameObject("PlayerGhost");
        Transform sourceTransform = spriteRenderer.transform;
        ghost.transform.SetPositionAndRotation(sourceTransform.position, sourceTransform.rotation);
        ghost.transform.localScale = sourceTransform.lossyScale;

        SpriteRenderer ghostRenderer = ghost.AddComponent<SpriteRenderer>();
        ghostRenderer.sprite = spriteRenderer.sprite;
        ghostRenderer.flipX = spriteRenderer.flipX;
        ghostRenderer.flipY = spriteRenderer.flipY;
        ghostRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        ghostRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        ghostRenderer.color = ghostColor;

        ghost.AddComponent<SpriteFadeOut>().Begin(ghostFadeDuration);
    }

    private Vector2 ReadInput()
    {
        Vector2 input = Vector2.zero;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null && input == Vector2.zero)
            input = gamepad.leftStick.ReadValue();

        return Vector2.ClampMagnitude(input, 1f);
    }
}
