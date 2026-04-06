using UnityEngine;

/// <summary>
/// Deals contact damage to the player when the enemy touches them.
///
/// SETUP:
/// 1. Attach to the enemy GameObject (alongside EnemyAI and EnemyHealth).
/// 2. Requires a Collider2D on the enemy (non-trigger for physics, or trigger for overlap).
/// 3. Tag the Player as "Player" and ensure it has PlayerHealth.
/// </summary>
public class EnemyDamage : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Damage Settings")]
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float damageCooldown = 1f;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private float cooldownTimer = 0f;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    // -------------------------------------------------------------------------
    // Collision Detection (supports both trigger and non-trigger colliders)
    // -------------------------------------------------------------------------

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other.gameObject);
    }

    private void TryDamagePlayer(GameObject other)
    {
        if (cooldownTimer > 0f) return;
        if (!other.CompareTag("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;

        playerHealth.TakeDamage(contactDamage);
        cooldownTimer = damageCooldown;
        Debug.Log($"Enemy dealt {contactDamage} damage to player!");
    }
}
