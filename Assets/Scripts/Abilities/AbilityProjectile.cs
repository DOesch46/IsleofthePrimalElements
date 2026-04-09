using UnityEngine;

/// <summary>
/// Attached to projectile GameObjects spawned by abilities.
/// Handles movement, collision, damage, and cleanup.
/// Now supports 4-directional movement.
/// </summary>
public class AbilityProjectile : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private string abilityName;
    [SerializeField] private int damage;
    [SerializeField] private ElementType element;
    [SerializeField] private float speed;
    [SerializeField] private Vector2 direction;
    [SerializeField] private float lifetime = 5f;

    private AbilityData abilityData;
    private Rigidbody2D rb;

    /// <summary>
    /// Initialize with a Vector2 direction (supports all 4 directions).
    /// Called by AbilityManager.
    /// </summary>
    public void Initialize(AbilityData ability, Vector2 aimDirection)
    {
        abilityData = ability;
        abilityName = ability.abilityName;
        damage = ability.damage;
        element = ability.element;
        speed = ability.projectileSpeed;
        direction = aimDirection.normalized;

        // Set velocity
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = direction * speed;
        }

        // Rotate sprite to match direction
        RotateToDirection();

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Old initialize method for backwards compatibility.
    /// Converts float direction to Vector2.
    /// </summary>
    public void Initialize(AbilityData ability, float facingDirection)
    {
        Vector2 dir = facingDirection >= 0 ? Vector2.right : Vector2.left;
        Initialize(ability, dir);
    }

    /// <summary>
    /// Rotates the projectile sprite to face the movement direction.
    /// </summary>
    private void RotateToDirection()
    {
        // Calculate angle from direction vector
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit the player
        if (other.CompareTag("Player"))
        {
            return;
        }

        // Hit an enemy
        if (other.CompareTag("Enemy"))
        {
            DealDamage(other.gameObject);
            Destroy(gameObject);
            return;
        }

        // Hit a wall or ground
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Deals damage with elemental multiplier.
    /// </summary>
    private void DealDamage(GameObject target)
    {
        float multiplier = 1f;
        if (abilityData != null)
        {
            ElementalEntity entity = target.GetComponent<ElementalEntity>();
            if (entity != null)
            {
                multiplier = abilityData.GetDamageMultiplier(entity.Element);
            }
        }

        float finalDamage = damage * multiplier;
        Debug.Log($"{abilityName} hit {target.name} for {finalDamage} damage! (x{multiplier})");

        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(finalDamage);
        }
        else
        {
            target.SendMessage("TakeDamage", finalDamage,
                SendMessageOptions.DontRequireReceiver);
        }
    }
}