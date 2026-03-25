using UnityEngine;

/// <summary>
/// Handles ground detection for the player using an overlap circle check.
/// Attached as a component and referenced by MovementSystem.
/// Keeps ground detection logic isolated and reusable.
/// </summary>
public class GroundChecker : MonoBehaviour
{
    [Header("Ground Check Settings")]
    [SerializeField] private float checkRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns true if the player is currently touching the ground.
    /// Uses an overlap circle at this transform's position (place this
    /// GameObject at the player's feet in the hierarchy).
    /// </summary>
    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(transform.position, checkRadius, groundLayer);
    }

    // -------------------------------------------------------------------------
    // Editor Visualization
    // -------------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}
