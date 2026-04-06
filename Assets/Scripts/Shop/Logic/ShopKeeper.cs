using UnityEngine;

/// <summary>
/// NPC that opens the shop when interacted with. Implements IInteractable
/// so it integrates with the existing InteractionSystem.
///
/// Setup:
///   - Add Collider2D, put on Interactable layer
///   - Assign the ShopUI reference in the inspector
/// </summary>
public class ShopKeeper : MonoBehaviour, IInteractable
{
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private string promptMessage = "Press E to open shop";

    public bool IsInteractable => !GameStateManager.IsUIOpen;

    public string GetPromptText() => promptMessage;

    public void Interact(PlayerController player)
    {
        if (shopUI != null)
            shopUI.Open();
    }
}