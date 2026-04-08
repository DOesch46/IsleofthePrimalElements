using UnityEngine;
using TMPro;

/// <summary>
/// Manages a torch puzzle. When all torches are lit, opens the gate.
/// Shows a UI counter in the corner of the screen.
/// 
/// Setup:
/// 1. Create an empty GameObject called "TorchPuzzle"
/// 2. Attach this script
/// 3. Drag all FireTorch objects into the "torches" array
/// 4. Drag the gate object into "gateToOpen"
/// 5. Create a UI Text for the counter and drag it in
/// </summary>
public class TorchPuzzle : MonoBehaviour
{
    [Header("Puzzle References")]
    [Tooltip("All torches that must be lit to solve the puzzle")]
    [SerializeField] private FireTorch[] torches;

    [Tooltip("The gate/barrier that opens when puzzle is solved")]
    [SerializeField] private GameObject gateToOpen;

    [Header("UI")]
    [Tooltip("Text that shows torch count (drag the TMP text here)")]
    [SerializeField] private TextMeshProUGUI torchCountText;

    [Header("Settings")]
    [Tooltip("Should the gate be destroyed or just disabled?")]
    [SerializeField] private bool destroyGate = false;

    [Header("Debug Info")]
    [SerializeField] private int torchesLit = 0;
    [SerializeField] private int totalTorches = 0;
    [SerializeField] private bool puzzleSolved = false;

    /// <summary>
    /// Is the puzzle solved?
    /// </summary>
    public bool IsSolved => puzzleSolved;

    private void Start()
    {
        // Count total torches
        if (torches != null)
        {
            totalTorches = torches.Length;
        }
        else
        {
            totalTorches = 0;
            Debug.LogWarning("TorchPuzzle: No torches assigned!");
        }

        // Make sure gate is active/closed at start
        if (gateToOpen != null)
        {
            gateToOpen.SetActive(true);
        }

        // Update UI
        UpdateUI();

        Debug.Log($"Torch Puzzle initialized: {totalTorches} torches to light.");
    }

    /// <summary>
    /// Called by individual torches when they are lit.
    /// </summary>
    public void CheckAllTorches()
    {
        if (puzzleSolved) return;
        if (torches == null) return;

        // Count how many torches are lit
        torchesLit = 0;
        foreach (FireTorch torch in torches)
        {
            if (torch != null && torch.IsLit)
            {
                torchesLit++;
            }
        }

        Debug.Log($"Torches lit: {torchesLit}/{totalTorches}");

        // Update UI
        UpdateUI();

        // Check if all torches are lit
        if (torchesLit >= totalTorches)
        {
            SolvePuzzle();
        }
    }

    /// <summary>
    /// Updates the torch counter UI text.
    /// </summary>
    private void UpdateUI()
    {
        if (torchCountText != null)
        {
            if (puzzleSolved)
            {
                torchCountText.text = "All torches lit! Gate opened!";
                torchCountText.color = Color.green;
            }
            else
            {
                torchCountText.text = $"Torches Lit: {torchesLit}/{totalTorches}";
                torchCountText.color = Color.white;
            }
        }
    }

    /// <summary>
    /// Called when all torches are lit.
    /// </summary>
    private void SolvePuzzle()
    {
        puzzleSolved = true;
        Debug.Log("*** TORCH PUZZLE SOLVED! Gate is opening! ***");

        // Update UI
        UpdateUI();

        // Open the gate
        if (gateToOpen != null)
        {
            if (destroyGate)
            {
                Destroy(gateToOpen);
            }
            else
            {
                gateToOpen.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Debug: Force solve the puzzle.
    /// </summary>
    [ContextMenu("Debug: Solve Puzzle")]
    public void DebugSolvePuzzle()
    {
        foreach (FireTorch torch in torches)
        {
            if (torch != null)
            {
                torch.LightTorch();
            }
        }
    }
}