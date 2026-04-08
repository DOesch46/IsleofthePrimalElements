using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single upgrade row in the shop UI.
/// This is a prefab that gets instantiated by ShopUI.
/// </summary>
public class ShopItemSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buyButtonText;

    private ShopUpgradeSO upgrade;
    private ShopManager shopManager;

    public void Setup(ShopUpgradeSO upgrade, ShopManager manager)
    {
        this.upgrade = upgrade;
        this.shopManager = manager;

        int currentLevel = manager.GetUpgradeLevel(upgrade.upgradeType);

        if (iconImage != null && upgrade.icon != null)
            iconImage.sprite = upgrade.icon;

        // Show upgrade level for stat upgrades, potion count for potions
        if (upgrade.upgradeType == UpgradeType.HealthPotion)
        {
            int potions = manager.GetHealthPotionCount();
            if (nameText != null) nameText.text = upgrade.upgradeName;
            if (descriptionText != null) descriptionText.text = $"{upgrade.description} (Owned: {potions})";
        }
        else
        {
            string maxLabel = upgrade.maxLevel > 0 ? $" (Lv {currentLevel}/{upgrade.maxLevel})" : "";
            if (nameText != null) nameText.text = upgrade.upgradeName + maxLabel;
            if (descriptionText != null) descriptionText.text = upgrade.description;
        }

        int cost = upgrade.GetCost(currentLevel);
        if (priceText != null) priceText.text = $"{cost}";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);

        RefreshButtonState();
    }

    private void OnBuyClicked()
    {
        shopManager.TryPurchase(upgrade);
    }

    private void RefreshButtonState()
    {
        int currentLevel = shopManager.GetUpgradeLevel(upgrade.upgradeType);

        if (upgrade.IsMaxed(currentLevel))
        {
            buyButtonText.text = "MAXED";
            buyButton.interactable = false;
        }
        else if (!shopManager.CanAfford(upgrade))
        {
            buyButtonText.text = "Can't Afford";
            buyButton.interactable = false;
        }
        else
        {
            buyButtonText.text = "Buy";
            buyButton.interactable = true;
        }
    }
}
