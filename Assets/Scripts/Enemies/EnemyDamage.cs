using UnityEngine;

/// <summary>
/// Deals contact damage to the player when the enemy is close enough.
/// Uses distance checking instead of collider callbacks for reliability
/// with Kinematic Rigidbody2D enemies.
///
/// SETUP:
/// 1. Attach to the enemy GameObject (alongside EnemyAI and EnemyHealth).
/// 2. Tag the Player as "Player" and ensure it has PlayerHealth.
/// </summary>
public class EnemyDamage : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Damage Settings")]
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField] private float damageRange = 0.8f;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private float cooldownTimer = 0f;
    private Transform player;
    private PlayerHealth playerHealth;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            Debug.Log($"EnemyDamage: Found player. PlayerHealth: {playerHealth != null}");
        }
        else
        {
            Debug.LogWarning("EnemyDamage: No Player found!");
        }
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (player == null || playerHealth == null) return;
        if (cooldownTimer > 0f) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= damageRange)
        {
            playerHealth.TakeDamage(contactDamage);
            cooldownTimer = damageCooldown;
            Debug.Log($"Enemy dealt {contactDamage} damage to player!");
        }
    }
}
