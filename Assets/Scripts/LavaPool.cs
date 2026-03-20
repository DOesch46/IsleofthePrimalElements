using UnityEngine;

/// <summary>
/// Lava hazard that damages the player while they stand in it.
/// Attach this to a GameObject with a Collider2D set as a Trigger.
/// </summary>
public class LavaPool : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Damage dealt to player each tick")]
    [SerializeField] private int damagePerTick = 10;
    
    [Tooltip("Time in seconds between each damage tick")]
    [SerializeField] private float damageInterval = 0.5f;

    // Reference to the player's health component (set when player enters)
    private PlayerHealth playerHealth;
    
    // Is the player currently in the lava?
    private bool playerInLava = false;
    
    // Timer to track damage intervals
    private float nextDamageTime = 0f;

    private void Update()
    {
        // Only run damage logic if player is in the lava
        if (playerInLava && playerHealth != null)
        {
            // Check if enough time has passed to deal damage again
            if (Time.time >= nextDamageTime)
            {
                // Deal damage to the player
                playerHealth.TakeDamage(damagePerTick);
                
                // Set the next damage time
                nextDamageTime = Time.time + damageInterval;
            }
        }
    }

    /// <summary>
    /// Called when something enters the trigger collider.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object that entered is the player
        // We check by tag - make sure your player has the "Player" tag!
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered lava!");
            
            // Try to get the PlayerHealth component from the player
            playerHealth = other.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                playerInLava = true;
                
                // Deal damage immediately when entering (optional)
                playerHealth.TakeDamage(damagePerTick);
                
                // Set next damage time
                nextDamageTime = Time.time + damageInterval;
            }
            else
            {
                // This warning helps you debug if something is set up wrong
                Debug.LogWarning("Player entered lava but has no PlayerHealth component!");
            }
        }
    }

    /// <summary>
    /// Called when something exits the trigger collider.
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the player is leaving
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited lava!");
            
            playerInLava = false;
            playerHealth = null;
        }
    }
}