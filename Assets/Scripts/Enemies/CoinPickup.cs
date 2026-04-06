using UnityEngine;

/// <summary>
/// Auto-collected coin that grants currency to the player on contact.
/// Spawned by enemies on death via EnemyHealth.
///
/// SETUP:
/// 1. Create a coin prefab with a SpriteRenderer and CircleCollider2D (Is Trigger = true).
/// 2. Attach this script to the coin prefab.
/// 3. Tag the Player as "Player".
/// 4. Assign the prefab to EnemyHealth's coinPrefab field.
/// </summary>
public class CoinPickup : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Coin Settings")]
    [SerializeField] private int coinValue = 1;

    // -------------------------------------------------------------------------
    // Collection
    // -------------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        GameProgressManager.Instance.AddCoins(coinValue);
        Debug.Log($"Coin collected! +{coinValue}");
        Destroy(gameObject);
    }
}
