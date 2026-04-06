using UnityEngine;

/// <summary>
/// Lava hazard that damages the player while they stand in it.
/// Works with the team's PlayerHealth system (float damage).
/// 
/// Setup:
/// 1. Create a GameObject with a SpriteRenderer (red/orange)
/// 2. Add a BoxCollider2D set as Trigger
/// 3. Attach this script
/// </summary>
public class LavaPool : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Damage dealt to player each tick")]
    [SerializeField] private float damagePerTick = 10f;
    
    [Tooltip("Time in seconds between each damage tick")]
    [SerializeField] private float damageInterval = 0.5f;

    // Reference to the player's health component
    private PlayerHealth playerHealth;
    
    // Is the player currently in the lava?
    private bool playerInLava = false;
    
    // Timer to track damage intervals
    private float nextDamageTime = 0f;

    private void Update()
    {
        if (playerInLava && playerHealth != null)
        {
            if (Time.time >= nextDamageTime)
            {
                playerHealth.TakeDamage(damagePerTick);
                nextDamageTime = Time.time + damageInterval;
                Debug.Log($"Lava dealt {damagePerTick} damage! Player HP: {playerHealth.GetCurrentHealth()}/{playerHealth.GetMaxHealth()}");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                playerInLava = true;
                playerHealth.TakeDamage(damagePerTick);
                nextDamageTime = Time.time + damageInterval;
                Debug.Log("Player entered lava!");
            }
            else
            {
                Debug.LogWarning("Player has no PlayerHealth component!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInLava = false;
            playerHealth = null;
            Debug.Log("Player exited lava.");
        }
    }
}