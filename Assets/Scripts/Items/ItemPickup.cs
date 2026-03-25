using UnityEngine;
using System;

/// <summary>
/// Implements IInteractable for collectible items in the world.
/// When the player interacts, fires a collection event with item data
/// and disables itself. Designed to be reusable across all levels and item types.
/// </summary>
public class ItemPickup : MonoBehaviour, IInteractable
{
    // -------------------------------------------------------------------------
    // Item Data Definition
    // -------------------------------------------------------------------------

    [System.Serializable]
    public class ItemData
    {
        public string itemName;
        public string itemDescription;
        public int xpValue;
        public Sprite icon;
    }

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>Fired when this item is collected. Subscribe in ProgressionManager.</summary>
    public static event Action<ItemData> OnItemCollected;

    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Item Settings")]
    [SerializeField] private ItemData item;
    [SerializeField] private string promptMessage = "Press E to pick up";

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private bool hasBeenCollected = false;

    // -------------------------------------------------------------------------
    // IInteractable Implementation
    // -------------------------------------------------------------------------

    public bool IsInteractable => !hasBeenCollected;

    public string GetPromptText()
    {
        return $"{promptMessage} {item.itemName}";
    }

    public void Interact(PlayerController player)
    {
        if (hasBeenCollected) return;

        hasBeenCollected = true;

        OnItemCollected?.Invoke(item);

        // Hide the object immediately; let the event listener handle XP/inventory
        gameObject.SetActive(false);
    }
}
