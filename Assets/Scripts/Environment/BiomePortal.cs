using UnityEngine;

/// Simple portal that loads a level when the player walks into it.
/// Place one of these at each biome entrance on the map.
///
/// SETUP:
/// 1. Set destinationLevel to the level this portal leads to.
/// 2. Set transitionId to a unique ID (e.g. "LightningPortal").
/// 3. In the DESTINATION scene, create a SpawnPoint with the same transitionId.
/// 4. Set returnTransitionId (e.g. "FromLightningLevel") — this is the ID
///    that the level's exit portal will use to bring the player BACK here.
/// 5. In THIS scene, create a SpawnPoint near this portal with transitionId
///    matching returnTransitionId so the player spawns back here.

[RequireComponent(typeof(Collider2D))]
public class BiomePortal : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Which level does this portal lead to? Drag a LevelData asset here.")]
    [SerializeField] private LevelData destinationLevel;

    [Tooltip("Display name for debug messages")]
    [SerializeField] private string portalName = "Portal";

    [Header("Scene Transition")]
    [Tooltip("ID to match a SpawnPoint in the destination scene (e.g. 'LightningPortal').")]
    [SerializeField] private string transitionId = "";

    private void Start()
    {
        // Ensure collider is a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }

        // Warn if no destination set
        if (destinationLevel == null)
        {
            Debug.LogWarning($"BiomePortal '{portalName}' has no destination level assigned!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only respond to player
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Debug.Log($"Player entered portal: {portalName}");

        // Check if destination is assigned
        if (destinationLevel == null)
        {
            Debug.LogError($"Portal '{portalName}' has no destination!");
            return;
        }

        // Use SceneTransitionManager if a transitionId is set (for spawn point matching)
        if (!string.IsNullOrEmpty(transitionId) && SceneTransitionManager.Instance != null)
        {
            Debug.Log($"Loading: {destinationLevel.levelName} with transitionId: {transitionId}");
            SceneTransitionManager.Instance.TransitionToScene(destinationLevel.sceneName, transitionId);
        }
        else if (GameProgressManager.Instance != null)
        {
            Debug.Log($"Loading: {destinationLevel.levelName}");
            GameProgressManager.Instance.TryLoadLevel(destinationLevel);
        }
        else
        {
            Debug.LogError("No manager found to load the level!");
        }
    }
}