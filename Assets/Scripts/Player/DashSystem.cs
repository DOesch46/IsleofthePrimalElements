using UnityEngine;
using System.Collections;

/// <summary>
/// Handles dash movement. Activated by PlayerController when player presses dash key.
/// Must be unlocked via PlayerStats.HasAbility("dash") from the Dash Scroll shop item.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MovementSystem))]
public class DashSystem : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;

    private Rigidbody2D rb;
    private MovementSystem movementSystem;
    private PlayerStats playerStats;

    private bool isDashing;
    private bool canDash = true;

    /// <summary>
    /// True while the player is mid-dash. PlayerController checks this
    /// to skip normal movement during a dash.
    /// </summary>
    public bool IsDashing => isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movementSystem = GetComponent<MovementSystem>();
        playerStats = GetComponent<PlayerStats>();
    }

    /// <summary>
    /// Attempts to dash in the current facing direction.
    /// Fails silently if: on cooldown, already dashing, no direction, or ability not unlocked.
    /// </summary>
    public void TryDash()
    {
        if (!canDash || isDashing)
            return;

        // Dash is only available after defeating the Lightning boss
        if (GameProgressManager.Instance == null || !GameProgressManager.Instance.HasElement(ElementType.Lightning))
            return;

        Vector2 direction = movementSystem.GetDirection();

        // Can't dash if not moving in any direction
        if (direction.sqrMagnitude < 0.01f)
            return;

        StartCoroutine(DashCoroutine(direction));
    }

    private IEnumerator DashCoroutine(Vector2 direction)
    {
        isDashing = true;
        canDash = false;

        // Apply burst velocity
        rb.linearVelocity = direction.normalized * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        // End dash — stop the burst
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        // Cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}