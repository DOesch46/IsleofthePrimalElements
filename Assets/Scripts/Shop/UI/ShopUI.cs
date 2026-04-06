using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the shop panel. Spawns ShopItemSlotUI elements from the catalog.
/// Separated from ShopManager — this is ONLY UI logic.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemSlotPrefab;

    [Header("Wallet Display")]
    [SerializeField] private TMP_Text coinText;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        shopPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        if (shopManager != null)
        {
            shopManager.OnPurchaseSuccess += HandlePurchaseSuccess;
        }

        PlayerWalletSO wallet = shopManager?.GetWallet();
        if (wallet != null)
            wallet.OnCoinsChanged += UpdateCoinDisplay;
    }

    private void OnDisable()
    {
        if (shopManager != null)
        {
            shopManager.OnPurchaseSuccess -= HandlePurchaseSuccess;
        }

        PlayerWalletSO wallet = shopManager?.GetWallet();
        if (wallet != null)
            wallet.OnCoinsChanged -= UpdateCoinDisplay;
    }

    public void Open()
    {
        shopPanel.SetActive(true);
        GameStateManager.IsUIOpen = true;
        PopulateItems();
        UpdateCoinDisplay(shopManager.GetWallet().Coins);
    }

    public void Close()
    {
        shopPanel.SetActive(false);
        GameStateManager.IsUIOpen = false;
    }

    private void PopulateItems()
    {
        // Clear existing slots
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        foreach (ShopItemSO item in shopManager.GetCatalog().items)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemContainer);
            ShopItemSlotUI slotUI = slot.GetComponent<ShopItemSlotUI>();
            slotUI.Setup(item, shopManager);
        }
    }

    private void HandlePurchaseSuccess(ShopItemSO item)
    {
        // Refresh all slots to update button states
        PopulateItems();
    }

    private void UpdateCoinDisplay(int newAmount)
    {
        if (coinText != null)
            coinText.text = $"{newAmount} coins";
    }
}