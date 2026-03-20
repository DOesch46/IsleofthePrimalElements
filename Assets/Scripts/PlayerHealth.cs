using UnityEngine;

/// <summary>
/// Simple player health system.
/// Attach this to your Player GameObject.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health the player can have")]
    [SerializeField] private int maxHealth = 100;
    
    [Tooltip("Current health - visible in Inspector for debugging")]
    [SerializeField] private int currentHealth;

    [Header("Invincibility Settings")]
    [Tooltip("Time in seconds the player is invincible after taking damage")]
    [SerializeField] private float invincibilityDuration = 0.5f;
    
    // Tracks when invincibility ends
    private float invincibilityEndTime = 0f;

    // Property to check if player is currently invincible
    public bool IsInvincible => Time.time < invincibilityEndTime;
    
    // Property to check current health from other scripts
    public int CurrentHealth => currentHealth;
    
    // Property to check max health from other scripts
    public int MaxHealth => maxHealth;

    private void Start()
    {
        // Initialize health to maximum when game starts
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Call this method to damage the player.
    /// Returns true if damage was applied, false if player was invincible.
    /// </summary>
    /// <param name="damageAmount">How much damage to deal</param>
    /// <returns>True if damage was applied</returns>
    public bool TakeDamage(int damageAmount)
    {
        // Don't take damage if invincible
        if (IsInvincible)
        {
            return false;
        }

        // Apply damage
        currentHealth -= damageAmount;
        
        // Clamp health to not go below 0
        currentHealth = Mathf.Max(currentHealth, 0);
        
        // Start invincibility period
        invincibilityEndTime = Time.time + invincibilityDuration;
        
        // Log for debugging - remove this in final game
        Debug.Log($"Player took {damageAmount} damage! Health: {currentHealth}/{maxHealth}");

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }

        return true;
    }

    /// <summary>
    /// Heals the player by the specified amount.
    /// </summary>
    /// <param name="healAmount">How much to heal</param>
    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        
        // Don't exceed max health
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        Debug.Log($"Player healed for {healAmount}! Health: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Called when player health reaches 0.
    /// </summary>
    private void Die()
    {
        Debug.Log("Player died!");
        
        // For now, just log it. You can add respawn logic later.
        // Options you might add:
        // - Reload the scene
        // - Trigger a death animation
        // - Show game over screen
        // - Respawn at checkpoint
    }
}