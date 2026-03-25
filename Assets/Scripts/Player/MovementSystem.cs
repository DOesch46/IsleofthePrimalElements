using UnityEngine;

/// <summary>
/// Handles all player movement: horizontal walking, jumping, and gravity tuning.
/// Works with Rigidbody2D for physics-based movement.
/// Depends on GroundChecker for grounded state.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovementSystem : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float coyoteTimeDuration = 0.12f;  // grace period after walking off a ledge
    [SerializeField] private float jumpBufferDuration = 0.12f;  // grace period for pressing jump just before landing

    [Header("Gravity Tuning")]
    [SerializeField] private float fallGravityMultiplier = 2.5f;    // faster fall for less floatiness
    [SerializeField] private float lowJumpGravityMultiplier = 2f;   // faster fall when jump is released early

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private Rigidbody2D rb;
    private GroundChecker groundChecker;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isJumping;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundChecker = GetComponentInChildren<GroundChecker>();
    }

    private void Update()
    {
        UpdateCoyoteTime();
        UpdateJumpBuffer();
        UpdateGravity();
    }

    // -------------------------------------------------------------------------
    // Public API — called by PlayerController each frame
    // -------------------------------------------------------------------------

    /// <summary>
    /// Moves the player horizontally. Pass the raw axis input (-1, 0, or 1).
    /// </summary>
    public void Move(float horizontalInput)
    {
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    /// <summary>
    /// Attempts a jump. Uses coyote time and jump buffer for forgiving feel.
    /// </summary>
    public void TryJump()
    {
        jumpBufferCounter = jumpBufferDuration;
    }

    /// <summary>
    /// Called when the jump button is released. Enables variable jump height.
    /// </summary>
    public void OnJumpReleased()
    {
        if (rb.velocity.y > 0)
            isJumping = false;
    }

    /// <summary>
    /// Returns true if the player is on the ground.
    /// </summary>
    public bool IsGrounded()
    {
        return groundChecker != null && groundChecker.IsGrounded();
    }

    /// <summary>
    /// Returns the current velocity of the Rigidbody2D.
    /// Used by PlayerController for animator state.
    /// </summary>
    public Vector2 GetVelocity()
    {
        return rb.velocity;
    }

    // -------------------------------------------------------------------------
    // Private Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tracks how long since the player was last grounded.
    /// Allows jumping for a short window after walking off a ledge.
    /// </summary>
    private void UpdateCoyoteTime()
    {
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTimeDuration;
            isJumping = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Handles the jump buffer — executes a buffered jump as soon as the
    /// player lands, even if they pressed the button slightly early.
    /// </summary>
    private void UpdateJumpBuffer()
    {
        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;

            if (coyoteTimeCounter > 0f && !isJumping)
            {
                ExecuteJump();
            }
        }
    }

    /// <summary>
    /// Applies extra gravity when falling or when jump is released early.
    /// This makes the jump arc feel intentional rather than floaty.
    /// </summary>
    private void UpdateGravity()
    {
        if (rb.velocity.y < 0)
        {
            // Falling — apply stronger gravity
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !isJumping)
        {
            // Jump button released early — cut the jump short
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpGravityMultiplier - 1) * Time.deltaTime;
        }
    }

    /// <summary>
    /// Applies the jump impulse and resets relevant counters.
    /// </summary>
    private void ExecuteJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
        isJumping = true;
    }
}
