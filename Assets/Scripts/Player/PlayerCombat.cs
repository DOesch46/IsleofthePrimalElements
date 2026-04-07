using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage   = 20f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float hitboxActiveTime = 0.15f;

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
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

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

        Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
        if (rb2d.linearVelocity.sqrMagnitude > 0.1f)
        {
            lastMoveDirection = rb2d.linearVelocity.normalized;
        }
    }

    // -------------------------------------------------------------------------
    // Input System Callback (Send Messages)
    // -------------------------------------------------------------------------

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

        if (animator != null)
            animator.SetTrigger(AnimAttack);

        if (attackHitbox != null)
        {
            attackHitbox.transform.localPosition = lastMoveDirection * 0.7f;
            attackHitbox.SetActive(true);
            hitboxActive = true;
            hitboxTimer  = hitboxActiveTime;
        }
        else
        {
            FallbackOverlapAttack();
        }
    }

    // -------------------------------------------------------------------------
    // Hitbox Collision
    // -------------------------------------------------------------------------

    private void FallbackOverlapAttack()
    {
        // Attack in the direction the player is facing, not a circle around them
        Vector2 attackPos = (Vector2)transform.position + lastMoveDirection * 0.7f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, 0.5f);

        // Only damage the closest enemy, not everything in range
        float closestDist = float.MaxValue;
        IDamageable closestTarget = null;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (hit.transform.IsChildOf(transform)) continue;

            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestTarget = damageable;
                }
            }
        }

        if (closestTarget != null)
            closestTarget.TakeDamage(attackDamage);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void OnHitboxTrigger(Collider2D other)
    {
        Debug.Log("Hit: " + other.name);

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(attackDamage);
    }

    /// <summary>
    /// Called by PlayerStats to apply attack bonus from shop items.
    /// </summary>
    public void SetAttackDamage(float newDamage)
    {
        attackDamage = newDamage;
    }

    // -------------------------------------------------------------------------
    // Editor Gizmos
    // -------------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 1.2f);
    }
}