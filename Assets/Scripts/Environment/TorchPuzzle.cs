using UnityEngine;

/// <summary>
/// Manages a torch puzzle. When all torches are lit, opens the door/barrier.
/// 
/// Setup:
/// 1. Create an empty GameObject called "TorchPuzzle"
/// 2. Attach this script
/// 3. Drag all FireTorch objects into the "torches" array
/// 4. Drag the door/barrier object into "doorToOpen"
/// </summary>
public class TorchPuzzle : MonoBehaviour
{
    [Header("Puzzle References")]
    [Tooltip("All torches that must be lit to solve the puzzle")]
    [SerializeField] private FireTorch[] torches;

    [Tooltip("The door or barrier that opens when puzzle is solved")]
    [SerializeField] private GameObject doorToOpen;

    [Header("Settings")]
    [Tooltip("Should the door be destroyed or just disabled?")]
    [SerializeField] private bool destroyDoor = false;

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

        // Make sure door is active/closed at start
        if (doorToOpen != null)
        {
            doorToOpen.SetActive(true);
        }

        Debug.Log($"Torch Puzzle initialized: {totalTorches} torches to light.");
    }

    /// <summary>
    /// Called by individual torches when they are lit.
    /// Checks if all torches are lit to solve the puzzle.
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

        // Check if all torches are lit
        if (torchesLit >= totalTorches)
        {
            SolvePuzzle();
        }
    }

    /// <summary>
    /// Called when all torches are lit.
    /// </summary>
    private void SolvePuzzle()
    {
        puzzleSolved = true;
        Debug.Log("*** TORCH PUZZLE SOLVED! Door is opening! ***");

        // Open the door
        if (doorToOpen != null)
        {
            if (destroyDoor)
            {
                Destroy(doorToOpen);
            }
            else
            {
                doorToOpen.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Debug: Force solve the puzzle.
    /// </summary>
    [ContextMenu("Debug: Solve Puzzle")]
    public void DebugSolvePuzzle()
    {
        // Light all torches
        foreach (FireTorch torch in torches)
        {
            if (torch != null)
            {
                torch.LightTorch();
            }
        }
    }
}