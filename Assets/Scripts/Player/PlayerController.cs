using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central MonoBehaviour for the player in a top-down 2D game.
///
/// Receives Input System callbacks via Send Messages behavior on PlayerInput.
/// Stores input values and delegates to MovementSystem, CollisionHandler,
/// and InteractionSystem each frame.
///
/// Top-down specific: uses full Vector2 input (WASD/arrows in all directions),
/// no jumping or gravity involved.
///
/// REQUIRED SETUP:
/// - PlayerInput component on this GameObject, Behavior = Send Messages
/// - Default Map set to "Player"
/// - Actions asset has: Move (Value/Vector2), Interact (Button)
/// - Rigidbody2D with Gravity Scale = 0, Freeze Rotation Z checked
/// </summary>
[RequireComponent(typeof(MovementSystem))]
[RequireComponent(typeof(CollisionHandler))]
[RequireComponent(typeof(InteractionSystem))]
public class PlayerController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector References
    // -------------------------------------------------------------------------

    [Header("Animator (optional)")]
    [SerializeField] private Animator animator;

    // -------------------------------------------------------------------------
    // Subsystem References
    // -------------------------------------------------------------------------

    private MovementSystem movementSystem;
    private CollisionHandler collisionHandler;
    private InteractionSystem interactionSystem;

    // -------------------------------------------------------------------------
    // Input State — stored from callbacks, consumed in Update
    // -------------------------------------------------------------------------

    private Vector2 moveInput;

    // -------------------------------------------------------------------------
    // Animator Parameter Hashes (cached for performance)
    // -------------------------------------------------------------------------

    private static readonly int AnimSpeedX   = Animator.StringToHash("SpeedX");
    private static readonly int AnimSpeedY   = Animator.StringToHash("SpeedY");
    private static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        movementSystem    = GetComponent<MovementSystem>();
        collisionHandler  = GetComponent<CollisionHandler>();
        interactionSystem = GetComponent<InteractionSystem>();
    }

    private void Update()
    {
        movementSystem.Move(moveInput);
        FlipSpriteToFaceDirection(moveInput);
        UpdateAnimator();
    }

    // -------------------------------------------------------------------------
    // Input System Callbacks (Send Messages)
    // Unity automatically calls these when the matching action fires.
    // Method name = "On" + exact action name from your Input Actions asset.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Receives Move input as a Vector2 (x = horizontal, y = vertical).
    /// Called every frame the move value changes.
    /// </summary>
    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    /// <summary>
    /// Receives Interact button press.
    /// </summary>
    private void OnInteract(InputValue value)
    {
        if (value.isPressed)
            interactionSystem.TriggerInteraction();
    }

    // -------------------------------------------------------------------------
    // Private Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Flips the sprite horizontally to face the direction of movement.
    /// Only flips on horizontal input to avoid snapping on pure vertical movement.
    /// </summary>
    private void FlipSpriteToFaceDirection(Vector2 input)
    {
        if (input.x == 0) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(input.x);
        transform.localScale = scale;
    }

    /// <summary>
    /// Updates animator parameters each frame if an Animator is assigned.
    /// SpeedX and SpeedY allow directional walk animations.
    /// IsMoving is a simple bool for idle vs walk blend.
    /// </summary>
    private void UpdateAnimator()
    {
        if (animator == null) return;

        Vector2 velocity = movementSystem.GetVelocity();
        animator.SetFloat(AnimSpeedX, velocity.x);
        animator.SetFloat(AnimSpeedY, velocity.y);
        animator.SetBool(AnimIsMoving, velocity.sqrMagnitude > 0.01f);
    }
}