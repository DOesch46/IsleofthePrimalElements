using UnityEngine;

/// <summary>
/// A rock projectile launched by Terradon.
///
/// SETUP IN UNITY:
/// 1. Create a new GameObject, name it "FallingRock".
/// 2. Add a SpriteRenderer with your rock sprite.
/// 3. Add a CircleCollider2D (set as Trigger).
/// 4. Add a Rigidbody2D (Gravity Scale = 0, Kinematic is fine).
/// 5. Attach this script.
/// 6. Save as a Prefab in Assets/Prefabs/Hazards/.
/// 7. Assign this prefab to TerradonController's rockPrefab field.
///
/// HOW IT WORKS:
/// - TerradonController calls Launch(targetPosition) after instantiation.
/// - The rock moves toward targetPosition at rockSpeed.
/// - It deals damage on contact with the player, then destroys itself.
/// - It auto-destroys after lifetime seconds if it misses.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class FallingRock : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Movement")]
    [SerializeField] private float rockSpeed   = 5f;
    [SerializeField] private float lifetime    = 4f;   // auto-destroy after this many seconds

    [Header("Damage")]
    [SerializeField] private float damage      = 15f;

    [Header("Visuals")]
    [Tooltip("Spin the rock sprite while in flight")]
    [SerializeField] private float spinSpeed   = 180f; // degrees per second
    [Tooltip("Scale up as the rock 'falls'. Set to 1 to disable.")]
    [SerializeField] private float finalScale  = 1.2f; // scale at impact vs launch (arc feel)

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private Rigidbody2D  rb;
    private CircleCollider2D col;
    private Vector2      moveDirection;
    private bool         launched      = false;
    private float        lifeTimer     = 0f;
    private Vector3      startScale;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();

        rb.gravityScale  = 0f;
        rb.freezeRotation = true;   // we rotate the sprite manually for spin
        col.isTrigger    = true;

        startScale = transform.localScale;
    }

    private void Update()
    {
        if (!launched) return;

        lifeTimer += Time.deltaTime;

        // Spin the sprite
        if (spinSpeed != 0f)
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

        // Gradually scale up to give a "falling" arc feeling
        float t = Mathf.Clamp01(lifeTimer / lifetime);
        transform.localScale = Vector3.Lerp(startScale, startScale * finalScale, t);

        // Auto-destroy if lifetime exceeded
        if (lifeTimer >= lifetime)
            Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (!launched) return;

        // Keep constant velocity toward target
        rb.linearVelocity = moveDirection * rockSpeed;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by TerradonController immediately after Instantiate.
    /// Sets the direction the rock will travel.
    /// </summary>
    public void Launch(Vector2 targetPosition)
    {
        moveDirection = ((Vector2)targetPosition - (Vector2)transform.position).normalized;
        launched = true;
    }

    /// <summary>Allows the boss to override default damage (e.g., enraged phase).</summary>
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    // -------------------------------------------------------------------------
    // Collision
    // -------------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit the player
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage);

            // Small knockback impulse
            Rigidbody2D playerRB = other.GetComponent<Rigidbody2D>();
            if (playerRB != null)
                playerRB.AddForce(moveDirection * 4f, ForceMode2D.Impulse);

            Destroy(gameObject);
            return;
        }

        // Shatter on walls / terrain (anything tagged "Ground" or on the Default layer)
        if (other.CompareTag("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            Destroy(gameObject);
        }
    }
}
