using UnityEngine;
using TMPro;

public class EnemyKillCounter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int totalEnemies = 6;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI enemyCountText;
    [SerializeField] private TextMeshProUGUI torchCountText;

    [Header("Puzzle Reference")]
    [SerializeField] private TorchPuzzle torchPuzzle;

    [Header("Enemy Gate")]                                                    // ← NEW
    [Tooltip("Wall/gate to remove when all enemies are defeated")]            // ← NEW
    [SerializeField] private GameObject enemyGate;                            // ← NEW

    private int enemiesDefeated = 0;
    private bool uiShown = false;
    private bool allDefeated = false;

    private void Start()
    {
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
        if (!uiShown && torchPuzzle != null && torchPuzzle.IsSolved)
        {
            if (enemyCountText != null)
            {
                if (torchCountText != null)
                {
                    torchCountText.gameObject.SetActive(false);
                }

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
            OpenEnemyGate();                                                  // ← NEW
        }

        UpdateUI();
    }

    private void OpenEnemyGate()                                              // ← NEW
    {                                                                         // ← NEW
        if (enemyGate != null)                                                // ← NEW
        {                                                                     // ← NEW
            enemyGate.SetActive(false);                                       // ← NEW
            Debug.Log("*** ENEMY GATE OPENED! Path to boss unlocked! ***");   // ← NEW
        }                                                                     // ← NEW
    }                                                                         // ← NEW

    private void UpdateUI()
    {
        if (enemyCountText == null) return;

        if (allDefeated)
        {
            enemyCountText.text = "All enemies defeated! Path opened!";
            enemyCountText.color = Color.green;
        }
        else
        {
            enemyCountText.text = $"Enemies Defeated: {enemiesDefeated}/{totalEnemies}";
            enemyCountText.color = Color.white;
        }
    }
}