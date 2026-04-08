using UnityEngine;
using TMPro;

/// <summary>
/// Displays an enemy kill counter UI, just like the torch counter.
/// Listens to EnemyHealth.OnEnemyDied event — no changes to other scripts needed.
///
/// SETUP:
/// 1. Create an empty GameObject called "EnemyKillCounter"
/// 2. Attach this script
/// 3. Drag EnemyCountText (TMP) into the text field
/// 4. Drag TorchPuzzle into the puzzle field
/// 5. Set totalEnemies to 6
/// </summary>
public class EnemyKillCounter : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Total number of enemies to defeat")]
    [SerializeField] private int totalEnemies = 6;

    [Header("UI")]
    [Tooltip("TMP text under PuzzleCanvas (like TorchCountText)")]
    [SerializeField] private TextMeshProUGUI enemyCountText;

    [Header("Puzzle Reference")]
    [Tooltip("Counter stays hidden until torch puzzle is solved")]
    [SerializeField] private TorchPuzzle torchPuzzle;

    private int enemiesDefeated = 0;
    private bool uiShown = false;
    private bool allDefeated = false;

    private void Start()
    {
        // Hide until puzzle is solved
        if (enemyCountText != null)
        {
            if (torchPuzzle != null)
            {
                enemyCountText.gameObject.SetActive(false);
            }
            else
            {
                enemyCountText.gameObject.SetActive(true);
                uiShown = true;
                UpdateUI();
            }
        }
    }

    private void OnEnable()
    {
        EnemyHealth.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        EnemyHealth.OnEnemyDied -= HandleEnemyDied;
    }

    private void Update()
    {
        // Show counter once torch puzzle is solved
        if (!uiShown && torchPuzzle != null && torchPuzzle.IsSolved)
        {
            if (enemyCountText != null)
            {
                enemyCountText.gameObject.SetActive(true);
                uiShown = true;
                UpdateUI();
            }
        }
    }

    private void HandleEnemyDied(GameObject enemy)
    {
        if (allDefeated) return;

        enemiesDefeated++;
        Debug.Log($"EnemyKillCounter: {enemiesDefeated}/{totalEnemies}");

        if (enemiesDefeated >= totalEnemies)
        {
            allDefeated = true;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (enemyCountText == null) return;

        if (allDefeated)
        {
            enemyCountText.text = "All enemies defeated!";
            enemyCountText.color = Color.green;
        }
        else
        {
            enemyCountText.text = $"Enemies Defeated: {enemiesDefeated}/{totalEnemies}";
            enemyCountText.color = Color.white;
        }
    }
}