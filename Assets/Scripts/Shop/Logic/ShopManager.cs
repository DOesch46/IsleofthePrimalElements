using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Handles upgrade purchase logic. Tracks upgrade levels and applies effects.
/// </summary>
public class ShopManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerWalletSO wallet;
    [SerializeField] private ShopCatalogSO catalog;

    // Upgrade levels persist via GameProgressManager/PlayerPrefs
    private Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>();

    public event Action OnUpgradePurchased;
    public event Action<string> OnPurchaseFailed;

    public ShopCatalogSO GetCatalog() => catalog;
    public PlayerWalletSO GetWallet() => wallet;

    private void Awake()
    {
        LoadUpgradeLevels();
    }

    public int GetUpgradeLevel(UpgradeType type)
    {
        return upgradeLevels.TryGetValue(type, out int level) ? level : 0;
    }

    public bool CanAfford(ShopUpgradeSO upgrade)
    {
        int level = GetUpgradeLevel(upgrade.upgradeType);
        return wallet.CanAfford(upgrade.GetCost(level));
    }

    public bool TryPurchase(ShopUpgradeSO upgrade)
    {
        int currentLevel = GetUpgradeLevel(upgrade.upgradeType);

        // Health potions don't have a max level, others do
        if (upgrade.IsMaxed(currentLevel))
        {
            OnPurchaseFailed?.Invoke("Already maxed out!");
            return false;
        }

        int cost = upgrade.GetCost(currentLevel);

        if (!wallet.SpendCoins(cost))
        {
            OnPurchaseFailed?.Invoke("Not enough coins!");
            return false;
        }

        // Health potions add to potion count, other upgrades increment level
        if (upgrade.upgradeType == UpgradeType.HealthPotion)
        {
            int potions = GetHealthPotionCount() + 1;
            PlayerPrefs.SetInt("Elementara_HealthPotions", potions);
            PlayerPrefs.Save();
        }
        else
        {
            upgradeLevels[upgrade.upgradeType] = currentLevel + 1;
            SaveUpgradeLevels();
        }

        // Apply stat upgrade immediately if player exists in this scene
        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
            stats.ApplyUpgrades(this);

        OnUpgradePurchased?.Invoke();
        return true;
    }

    public int GetHealthPotionCount()
    {
        return PlayerPrefs.GetInt("Elementara_HealthPotions", 0);
    }

    public bool UseHealthPotion()
    {
        int potions = GetHealthPotionCount();
        if (potions <= 0) return false;

        PlayerPrefs.SetInt("Elementara_HealthPotions", potions - 1);
        PlayerPrefs.Save();

        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
            health.Heal(50f);

        OnUpgradePurchased?.Invoke(); // refresh UI
        return true;
    }

    private void SaveUpgradeLevels()
    {
        foreach (var kvp in upgradeLevels)
        {
            PlayerPrefs.SetInt("Elementara_Upgrade_" + kvp.Key, kvp.Value);
        }
        PlayerPrefs.Save();
    }

    private void LoadUpgradeLevels()
    {
        upgradeLevels.Clear();
        foreach (UpgradeType type in Enum.GetValues(typeof(UpgradeType)))
        {
            if (type == UpgradeType.HealthPotion) continue;
            int level = PlayerPrefs.GetInt("Elementara_Upgrade_" + type, 0);
            upgradeLevels[type] = level;
        }
    }

    /// <summary>
    /// Called by GameProgressManager.ResetAllProgress to clear upgrade data.
    /// </summary>
    public static void ResetAllUpgrades()
    {
        foreach (UpgradeType type in Enum.GetValues(typeof(UpgradeType)))
        {
            PlayerPrefs.DeleteKey("Elementara_Upgrade_" + type);
        }
        PlayerPrefs.DeleteKey("Elementara_HealthPotions");
        PlayerPrefs.Save();
    }
}
