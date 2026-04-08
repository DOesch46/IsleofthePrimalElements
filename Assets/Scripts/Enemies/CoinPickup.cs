using UnityEngine;

/// <summary>
/// Auto-collected coin that grants currency to the player on contact.
/// Spawned by enemies on death via EnemyHealth.
/// </summary>
public class CoinPickup : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int coinValue = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Add to persistent save system
        GameProgressManager.Instance.AddCoins(coinValue);

        // Sync the wallet SO so shop UI updates immediately
        ShopManager shop = FindFirstObjectByType<ShopManager>();
        if (shop != null && shop.GetWallet() != null)
            shop.GetWallet().SyncFromProgress();

        Debug.Log($"Coin collected! +{coinValue}");
        Destroy(gameObject);
    }
}
