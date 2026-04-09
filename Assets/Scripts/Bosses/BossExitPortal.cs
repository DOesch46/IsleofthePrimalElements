using UnityEngine;

/// <summary>
/// A portal that appears when the boss is defeated and takes the player
/// back to the MainIsland scene, spawning them where they entered.
///
/// SETUP IN UNITY:
/// 1. Create a GameObject in the LightningLevel scene (e.g. "BossExitPortal").
/// 2. Add a SpriteRenderer with a portal sprite.
/// 3. Add a Collider2D (BoxCollider2D or CircleCollider2D), check "Is Trigger".
/// 4. Attach this script.
/// 5. Set it to INACTIVE (uncheck the checkbox) — it gets activated when the boss dies.
/// 6. Set returnSceneName to "MainIsland".
/// 7. Set returnTransitionId to "FromLightningLevel".
/// 8. In the MainIsland scene, create a SpawnPoint near the Lightning Portal
///    with transitionId = "FromLightningLevel".
///
/// The BossMusicWatcher will activate this portal automatically when the boss dies.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BossExitPortal : MonoBehaviour
{
    [Header("Return Settings")]
    [Tooltip("Scene name to return to (e.g. 'MainIsland').")]
    [SerializeField] private string returnSceneName = "MainIsland";

    [Tooltip("Transition ID to match a SpawnPoint in the return scene (e.g. 'FromLightningLevel').")]
    [SerializeField] private string returnTransitionId = "FromLightningLevel";

    [Header("Level Completion")]
    [Tooltip("The current level's LevelData. Used to mark the level as completed and grant the element.")]
    [SerializeField] private LevelData currentLevel;

    private void Start()
    {
        // Ensure collider is a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player entered boss exit portal! Returning to " + returnSceneName);

        // Mark level as completed and grant element
        if (currentLevel != null && GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.CompleteLevel(currentLevel);
            Debug.Log($"Level '{currentLevel.levelName}' completed! grantElementOnComplete={currentLevel.grantElementOnComplete}");
        }

        // Transition back to the hub, spawning at the matching SpawnPoint
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(returnSceneName, returnTransitionId);
        }
        else
        {
            // Fallback — just load the scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(returnSceneName);
        }
    }
}
