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
        rb.linearVelocity = input.normalized * moveSpeed;
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
