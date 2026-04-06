using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single item row in the shop UI.
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

    private ShopItemSO item;
    private ShopManager shopManager;

    public void Setup(ShopItemSO item, ShopManager manager)
    {
        this.item = item;
        this.shopManager = manager;

        if (iconImage != null) iconImage.sprite = item.icon;
        if (nameText != null) nameText.text = item.itemName;
        if (descriptionText != null) descriptionText.text = item.description;
        if (priceText != null) priceText.text = $"{item.price}";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);

        RefreshButtonState();
    }

    private void OnBuyClicked()
    {
        shopManager.TryPurchase(item);
        RefreshButtonState();
    }

    private void RefreshButtonState()
    {
        if (shopManager.AlreadyOwned(item))
        {
            buyButtonText.text = "Owned";
            buyButton.interactable = false;
        }
        else if (!shopManager.CanAfford(item))
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