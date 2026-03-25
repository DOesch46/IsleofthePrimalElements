using UnityEngine;

/// <summary>
/// Central MonoBehaviour for the player. Reads input and delegates
/// to focused subsystems: MovementSystem, CollisionHandler, InteractionSystem.
///
/// This class does NOT contain physics math, collision logic, or interaction
/// details — it only coordinates. Each subsystem is independently testable.
///
/// Setup: Attach this to the Player GameObject. Assign GroundCheck child
/// transform. The subsystem components should also be on the same GameObject.
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
    // Animator Parameter Hashes (cached for performance)
    // -------------------------------------------------------------------------

    private static readonly int AnimSpeed        = Animator.StringToHash("Speed");
    private static readonly int AnimIsGrounded   = Animator.StringToHash("IsGrounded");
    private static readonly int AnimVerticalVelocity = Animator.StringToHash("VerticalVelocity");

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
        HandleMovementInput();
        HandleJumpInput();
        UpdateCollisionState();
        UpdateAnimator();
    }

    // -------------------------------------------------------------------------
    // Input Handling — reads raw input and passes values to subsystems
    // -------------------------------------------------------------------------

    private void HandleMovementInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        movementSystem.Move(horizontalInput);

        FlipSpriteToFaceDirection(horizontalInput);
    }

    private void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump"))
            movementSystem.TryJump();

        if (Input.GetButtonUp("Jump"))
            movementSystem.OnJumpReleased();
    }

    // -------------------------------------------------------------------------
    // State Updates
    // -------------------------------------------------------------------------

    /// <summary>
    /// Passes current grounded state to CollisionHandler so it can detect landing.
    /// </summary>
    private void UpdateCollisionState()
    {
        collisionHandler.UpdateGroundedState(movementSystem.IsGrounded());
    }

    /// <summary>
    /// Pushes velocity and grounded state to the Animator each frame.
    /// </summary>
    private void UpdateAnimator()
    {
        if (animator == null) return;

        Vector2 velocity = movementSystem.GetVelocity();

        animator.SetFloat(AnimSpeed, Mathf.Abs(velocity.x));
        animator.SetBool(AnimIsGrounded, movementSystem.IsGrounded());
        animator.SetFloat(AnimVerticalVelocity, velocity.y);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Flips the player sprite to face the direction of movement.
    /// </summary>
    private void FlipSpriteToFaceDirection(float horizontalInput)
    {
        if (horizontalInput == 0) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(horizontalInput);
        transform.localScale = scale;
    }
}
