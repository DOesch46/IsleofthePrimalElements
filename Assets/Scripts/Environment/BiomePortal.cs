using UnityEngine;

/// Simple portal that loads a level when the player walks into it.
/// Place one of these at each biome entrance on the map.

[RequireComponent(typeof(Collider2D))]
public class BiomePortal : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Which level does this portal lead to? Drag a LevelData asset here.")]
    [SerializeField] private LevelData destinationLevel;

    [Tooltip("Display name for debug messages")]
    [SerializeField] private string portalName = "Portal";

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

        // Load the level
        if (GameProgressManager.Instance != null)
        {
            Debug.Log($"Loading: {destinationLevel.levelName}");
            GameProgressManager.Instance.TryLoadLevel(destinationLevel);
        }
        else
        {
            Debug.LogError("GameProgressManager not found!");
        }
    }
}