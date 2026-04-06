using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles top-down 2D player movement using Rigidbody2D.
/// Moves the player in all four directions based on input.
/// No gravity, jumping, or ground detection needed for top-down.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovementSystem : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private Rigidbody2D rb;
    private Vector2 currentDirection;
    private Vector2 previousInput;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Top-down games must have zero gravity
        rb.gravityScale = 0f;

        // Prevent any accidental rotation from physics collisions
        rb.freezeRotation = true;
    }

    // -------------------------------------------------------------------------
    // Public API — called by PlayerController each frame
    // -------------------------------------------------------------------------

    /// <summary>
    /// Moves the player in any direction using the full 2D input vector.
    /// normalized() ensures diagonal movement isn't faster than cardinal.
    /// </summary>
    public void Move(Vector2 input)
    {
        // Lock to cardinal directions — last pressed key wins
        if (input.sqrMagnitude < 0.01f)
        {
            currentDirection = Vector2.zero;
            previousInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Find which axis just changed (newly pressed)
        bool xNew = Mathf.Abs(input.x) > 0.1f && Mathf.Abs(previousInput.x) < 0.1f;
        bool yNew = Mathf.Abs(input.y) > 0.1f && Mathf.Abs(previousInput.y) < 0.1f;

        if (xNew && !yNew)
            currentDirection = new Vector2(Mathf.Sign(input.x), 0f);
        else if (yNew && !xNew)
            currentDirection = new Vector2(0f, Mathf.Sign(input.y));
        else if (xNew && yNew)
        {
            // Both pressed same frame — pick stronger
            if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
                currentDirection = new Vector2(Mathf.Sign(input.x), 0f);
            else
                currentDirection = new Vector2(0f, Mathf.Sign(input.y));
        }
        // else: nothing new pressed, keep current direction

        // If the held key for current direction was released, switch to the other
        if (currentDirection.x != 0f && Mathf.Abs(input.x) < 0.1f)
            currentDirection = new Vector2(0f, Mathf.Sign(input.y));
        else if (currentDirection.y != 0f && Mathf.Abs(input.y) < 0.1f)
            currentDirection = new Vector2(Mathf.Sign(input.x), 0f);

        previousInput = input;
        rb.linearVelocity = currentDirection * moveSpeed;
    }

    /// <summary>
    /// Stops all movement immediately. Called when no input is held.
    /// </summary>
    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Returns current velocity for use by the animator.
    /// </summary>
    public Vector2 GetVelocity()
    {
        return rb.linearVelocity;
    }

    /// <summary>
    /// Returns the snapped cardinal direction for animator use.
    /// </summary>
    public Vector2 GetDirection()
    {
        return currentDirection;
    }

    /// <summary>
    /// Returns current move speed. Useful for progression-based upgrades later.
    /// </summary>
    public float GetMoveSpeed() => moveSpeed;

    /// <summary>
    /// Allows progression system to upgrade movement speed.
    /// </summary>
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
}
