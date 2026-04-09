using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossExitPortalEarth : MonoBehaviour
{
    [Header("Return Settings")]
    [Tooltip("Scene name to return to (e.g. 'MainIsland').")]
    [SerializeField] private string returnSceneName = "MainIsland";

    [Tooltip("Transition ID to match a SpawnPoint in the return scene.")]
    [SerializeField] private string returnTransitionId = "FromEarthLevel";

    [Header("Level Completion")]
    [Tooltip("The current level's LevelData. Used to mark the level as completed and grant the element.")]
    [SerializeField] private LevelData currentLevel;

    private void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player entered Earth Boss exit portal! Returning to " + returnSceneName);

        if (currentLevel != null && GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.CompleteLevel(currentLevel);
            Debug.Log($"Level '{currentLevel.levelName}' completed! Earth element granted.");
        }

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(returnSceneName, returnTransitionId);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(returnSceneName);
        }
    }
}