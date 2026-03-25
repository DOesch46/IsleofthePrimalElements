using UnityEngine;
using TMPro;

/// <summary>
/// Controls the "Press E to interact" prompt shown near interactable objects.
/// Kept as its own component so the UI can be swapped or animated independently
/// without touching interaction logic.
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector References
    // -------------------------------------------------------------------------

    [Header("UI References")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        Hide();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Shows the prompt panel with the given message.
    /// </summary>
    public void Show(string message)
    {
        if (promptPanel == null) return;

        promptText.text = message;
        promptPanel.SetActive(true);
    }

    /// <summary>
    /// Hides the prompt panel.
    /// </summary>
    public void Hide()
    {
        if (promptPanel == null) return;

        promptPanel.SetActive(false);
    }
}
