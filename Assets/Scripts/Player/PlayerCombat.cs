using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player melee attacks.
///
/// SETUP:
/// 1. Attach this to the Player GameObject (alongside PlayerController).
/// 2. Create a child GameObject named "AttackHitbox".
///    - Add a CircleCollider2D to it (Is Trigger = true).
///    - Set its position in front of the player (e.g. x = 0.7).
///    - Disable the GameObject by default in the Hierarchy.
///    - Drag it into the attackHitbox field below.
/// 3. In your Input Actions asset, add an "Attack" action (Button).
///    PlayerController's Send Messages behavior will call OnAttack() automatically.
/// 4. Assign an Animator if you want attack animations.
///
/// HOW IT WORKS:
/// - OnAttack() is called by the Input System via Send Messages.
/// - The hitbox GameObject is briefly enabled, detects overlapping enemies/bosses,
///   and calls TakeDamage() on anything it finds with the right tag.
/// - The hitbox is then disabled again so it only hits once per swing.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage   = 20f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float hitboxActiveTime = 0.15f;  // how long the hitbox stays on

    [Header("References")]
    [Tooltip("Child GameObject with a Trigger Collider2D used as the melee hitbox.")]
    [SerializeField] private GameObject attackHitbox;

    [Tooltip("Optional Animator — triggers 'Attack' parameter.")]
    [SerializeField] private Animator animator;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private float cooldownTimer  = 0f;
    private float hitboxTimer    = 0f;
    private bool  hitboxActive   = false;
    private Vector2 lastMoveDirection = Vector2.right;

    private static readonly int AnimAttack = Animator.StringToHash("Attack");

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Count down cooldown
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // Deactivate hitbox after its active window
        if (hitboxActive)
        {
            hitboxTimer -= Time.deltaTime;
            if (hitboxTimer <= 0f)
            {
                hitboxActive = false;
                if (attackHitbox != null)
                    attackHitbox.SetActive(false);
            }
        }
        //Track last movement direction
        Rigidbody2D rb2d = GetComponent<Rigidbody2D>();

        if (rb2d.linearVelocity.sqrMagnitude > 0.1f)
        {
            lastMoveDirection = rb2d.linearVelocity.normalized;
        }
    }

    // -------------------------------------------------------------------------
    // Input System Callback (Send Messages)
    // Name must match your Input Action exactly: "On" + ActionName
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called automatically by PlayerInput (Send Messages) when Attack fires.
    /// </summary>
    private void OnAttack(InputValue value)
    {
        if (!value.isPressed)      return;
        if (cooldownTimer > 0f)    return;

        PerformAttack();
    }

    // -------------------------------------------------------------------------
    // Attack Logic
    // -------------------------------------------------------------------------

    private void PerformAttack()
    {
        cooldownTimer = attackCooldown;

        // Trigger animation
        if (animator != null)
            animator.SetTrigger(AnimAttack);

        // Enable hitbox for a brief window
        if (attackHitbox != null)
        {
            //Move hitbox in front of player based on direction
            attackHitbox.transform.localPosition = lastMoveDirection * 0.7f;
            
            attackHitbox.SetActive(true);
            hitboxActive = true;
            hitboxTimer  = hitboxActiveTime;
        }
        else
        {
            // Fallback: use OverlapCircle at player position if no hitbox assigned
            FallbackOverlapAttack();
        }
    }

    // -------------------------------------------------------------------------
    // Hitbox Collision (handled on the child hitbox object via AttackHitboxReporter,
    // OR via fallback below if no hitbox GameObject is set up yet)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Fallback attack detection using OverlapCircle (no child hitbox needed).
    /// Less precise but works out of the box.
    /// </summary>
    private void FallbackOverlapAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.2f);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue; // don't hit self

            // Try to damage a boss
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
                continue;
            }

            // You can add other boss types here:
            // PyronisController pyronis = hit.GetComponent<PyronisController>();
            // if (pyronis != null) pyronis.TakeDamage(attackDamage);
        }
    }

    // -------------------------------------------------------------------------
    // Public API (called by AttackHitboxReporter on the child hitbox object)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by AttackHitboxReporter when the hitbox triggers a collider.
    /// </summary>
    public void OnHitboxTrigger(Collider2D other)
    {
        Debug.Log("Hit: " + other.name);
        
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(attackDamage);

        // Add other enemy/boss types here as you build them out.
    }

    // -------------------------------------------------------------------------
    // Editor Gizmos
    // -------------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 1.2f); // shows fallback range
    }
}
