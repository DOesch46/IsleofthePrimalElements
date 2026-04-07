using UnityEngine;
using System;

/// <summary>
/// Handles purchase logic. NO UI code here — only validates and executes purchases.
/// </summary>
public class ShopManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerWalletSO wallet;
    [SerializeField] private PlayerInventorySO inventory;
    [SerializeField] private ShopCatalogSO catalog;

    public event Action<ShopItemSO> OnPurchaseSuccess;
    public event Action<string> OnPurchaseFailed;

    public ShopCatalogSO GetCatalog() => catalog;
    public PlayerWalletSO GetWallet() => wallet;

    public bool CanAfford(ShopItemSO item) => wallet.CanAfford(item.price);
    public bool AlreadyOwned(ShopItemSO item) => inventory.HasItem(item);

    
    private void Awake()
{
    // Reset SO data at start of play session (editor safety)
    #if UNITY_EDITOR
    inventory.ResetInventory();
    wallet.ResetWallet(200);
    #endif
}
    public bool TryPurchase(ShopItemSO item)
    {
        if (inventory.HasItem(item))
        {
            OnPurchaseFailed?.Invoke("Already owned!");
            return false;
        }

        if (!wallet.SpendCoins(item.price))
        {
            OnPurchaseFailed?.Invoke("Not enough coins!");
            return false;
        }

        inventory.AddItem(item);

        // Apply effect immediately if PlayerStats exists in this scene
        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            ItemEffectApplier.Apply(item, stats);
        }

        OnPurchaseSuccess?.Invoke(item);
        return true;
    }
}