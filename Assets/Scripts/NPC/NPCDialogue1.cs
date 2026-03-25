using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Implements IInteractable for NPCs that have dialogue to deliver.
/// Supports multi-line dialogue sequences with continue-on-press flow.
/// Reusable across all levels — just set the dialogue lines in the Inspector.
/// </summary>
public class NPCDialogueHandler : MonoBehaviour, IInteractable
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("NPC Info")]
    [SerializeField] private string npcName = "Thalen";
    [SerializeField] private string interactPrompt = "Press E to talk";

    [Header("Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] dialogueLines;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueBodyText;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private int currentLineIndex = 0;
    private bool isDialogueActive = false;

    // -------------------------------------------------------------------------
    // IInteractable Implementation
    // -------------------------------------------------------------------------

    public bool IsInteractable => true;  // NPCs can always be talked to

    public string GetPromptText() => $"{interactPrompt} to {npcName}";

    /// <summary>
    /// On first press: opens dialogue. On subsequent presses: advances to next line.
    /// On final line: closes dialogue.
    /// </summary>
    public void Interact(PlayerController player)
    {
        if (!isDialogueActive)
        {
            OpenDialogue();
        }
        else
        {
            AdvanceDialogue();
        }
    }

    // -------------------------------------------------------------------------
    // Dialogue Flow
    // -------------------------------------------------------------------------

    private void OpenDialogue()
    {
        isDialogueActive = true;
        currentLineIndex = 0;

        dialoguePanel.SetActive(true);
        speakerNameText.text = npcName;

        ShowCurrentLine();
    }

    private void AdvanceDialogue()
    {
        currentLineIndex++;

        if (currentLineIndex >= dialogueLines.Length)
        {
            CloseDialogue();
        }
        else
        {
            ShowCurrentLine();
        }
    }

    private void ShowCurrentLine()
    {
        if (dialogueLines == null || dialogueLines.Length == 0) return;
        dialogueBodyText.text = dialogueLines[currentLineIndex];
    }

    private void CloseDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        currentLineIndex = 0;
    }

    // -------------------------------------------------------------------------
    // Cleanup
    // -------------------------------------------------------------------------

    private void OnDisable()
    {
        // Ensure dialogue closes if the NPC is deactivated mid-conversation
        if (isDialogueActive)
            CloseDialogue();
    }
}
