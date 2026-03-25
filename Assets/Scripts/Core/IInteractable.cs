/// <summary>
/// Interface that all interactable world objects must implement.
/// Provides a contract for the InteractionSystem to detect and trigger
/// interactions without needing to know the concrete type.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when the player presses the interact key while this object is in range.
    /// </summary>
    /// <param name="player">Reference to the PlayerController initiating the interaction.</param>
    void Interact(PlayerController player);

    /// <summary>
    /// Returns the prompt text to display in the UI when the player is nearby.
    /// Example: "Press E to open chest"
    /// </summary>
    string GetPromptText();

    /// <summary>
    /// Whether this object can currently be interacted with.
    /// Allows objects to disable themselves mid-game (e.g. already-opened chest).
    /// </summary>
    bool IsInteractable { get; }
}
