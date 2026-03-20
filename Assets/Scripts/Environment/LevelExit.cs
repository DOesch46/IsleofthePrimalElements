using UnityEngine;

/// <summary>
/// Attach this to a portal, door, or exit zone in your level.
/// When the player enters the trigger, it completes the level and loads the next scene.
/// 
/// Setup:
/// 1. Add a Collider2D and set it as a Trigger
/// 2. Assign the current level's LevelData
/// 3. Assign the destination LevelData (usually HubWorld)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("The level the player is currently in (drag your LevelData asset here)")]
    [SerializeField] private LevelData currentLevel;
    
    [Tooltip("The level to load when using this exit (usually HubWorld)")]
    [SerializeField] private LevelData destinationLevel;

    [Header("Exit Requirements")]
    [Tooltip("If true, player cannot exit until objective/boss is complete")]
    [SerializeField] private bool requiresObjectiveComplete = true;
    
    [Tooltip("Reference to the LevelObjective script (assign in Phase 5, optional for now)")]
    [SerializeField] private MonoBehaviour levelObjective;

    [Header("Visual Feedback")]
    [Tooltip("If assigned, this sprite renderer will change color when exit is available")]
    [SerializeField] private SpriteRenderer portalVisual;
    
    [Tooltip("Color when exit is locked")]
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    [Tooltip("Color when exit is available")]
    [SerializeField] private Color unlockedColor = new Color(1f, 1f, 1f, 1f);

    // Internal state
    private bool exitAvailable = false;
    private Collider2D exitCollider;

    private void Start()
    {
        // Cache the collider reference
        exitCollider = GetComponent<Collider2D>();
        
        // Ensure collider is a trigger
        if (exitCollider != null && !exitCollider.isTrigger)
        {
            Debug.LogWarning($"LevelExit on {gameObject.name}: Collider was not a trigger. Setting it automatically.");
            exitCollider.isTrigger = true;
        }

        // Validate setup
        if (currentLevel == null)
        {
            Debug.LogWarning($"LevelExit on {gameObject.name}: Missing 'Current Level' reference!");
        }
        
        if (destinationLevel == null)
        {
            Debug.LogWarning($"LevelExit on {gameObject.name}: Missing 'Destination Level' reference!");
        }

        // If no objective required, exit is immediately available
        if (!requiresObjectiveComplete)
        {
            SetExitAvailable(true);
        }
        else
        {
            SetExitAvailable(false);
        }
    }

    private void Update()
    {
        // If we require objective completion, check status each frame
        // This will be properly implemented in Phase 5
        if (requiresObjectiveComplete && levelObjective != null)
        {
            // We'll replace this with proper LevelObjective check in Phase 5
            // For now, it stays locked until we set it manually
        }
    }

    /// <summary>
    /// Called when something enters the trigger collider.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only respond to the player
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Debug.Log("Player touched the level exit!");

        // Attempt to use the exit
        AttemptExit();
    }

    /// <summary>
    /// Tries to complete the level and load the next scene.
    /// </summary>
    private void AttemptExit()
    {
        // Check if exit is available
        if (requiresObjectiveComplete && !exitAvailable)
        {
            Debug.Log("Exit is locked! Complete the objective first.");
            OnExitLocked();
            return;
        }

        // Complete the current level
        CompleteCurrentLevel();

        // Load the destination
        LoadDestination();
    }

    /// <summary>
    /// Marks the current level as complete and grants the element.
    /// </summary>
    private void CompleteCurrentLevel()
    {
        if (currentLevel == null)
        {
            Debug.LogWarning("Cannot complete level: Current Level is not assigned!");
            return;
        }

        if (GameProgressManager.Instance == null)
        {
            Debug.LogError("Cannot complete level: GameProgressManager not found!");
            return;
        }

        // This automatically:
        // 1. Marks the level as completed
        // 2. Grants the element (if it's an elemental level)
        // 3. Saves progress
        GameProgressManager.Instance.CompleteLevel(currentLevel);
        
        Debug.Log($"Level '{currentLevel.levelName}' completed!");
        
        // If this level has an element, confirm it was granted
        if (currentLevel.IsElementalLevel)
        {
            Debug.Log($"Element '{currentLevel.elementType}' has been granted!");
        }
    }

    /// <summary>
    /// Loads the destination level.
    /// </summary>
    private void LoadDestination()
    {
        if (destinationLevel == null)
        {
            Debug.LogError("Cannot load destination: Destination Level is not assigned!");
            return;
        }

        if (GameProgressManager.Instance == null)
        {
            Debug.LogError("Cannot load destination: GameProgressManager not found!");
            return;
        }

        Debug.Log($"Loading destination: {destinationLevel.levelName}");
        GameProgressManager.Instance.TryLoadLevel(destinationLevel);
    }

    /// <summary>
    /// Call this from other scripts (like LevelObjective) to unlock the exit.
    /// </summary>
    public void UnlockExit()
    {
        SetExitAvailable(true);
        Debug.Log("Level exit is now UNLOCKED!");
    }

    /// <summary>
    /// Call this to lock the exit again (if needed).
    /// </summary>
    public void LockExit()
    {
        SetExitAvailable(false);
        Debug.Log("Level exit is now LOCKED!");
    }

    /// <summary>
    /// Sets the exit availability and updates visuals.
    /// </summary>
    private void SetExitAvailable(bool available)
    {
        exitAvailable = available;
        UpdateVisuals();
    }

    /// <summary>
    /// Updates the portal visual based on locked/unlocked state.
    /// </summary>
    private void UpdateVisuals()
    {
        if (portalVisual == null)
        {
            return;
        }

        if (exitAvailable)
        {
            portalVisual.color = unlockedColor;
        }
        else
        {
            portalVisual.color = lockedColor;
        }
    }

    /// <summary>
    /// Called when player tries to exit but it's locked.
    /// Override or extend this for custom feedback (sounds, UI, etc.)
    /// </summary>
    protected virtual void OnExitLocked()
    {
        // You could add:
        // - Play a "locked" sound effect
        // - Show UI message "Defeat the boss first!"
        // - Shake the portal
        Debug.Log("TODO: Add locked feedback (sound, UI message, etc.)");
    }

    /// <summary>
    /// Draws the exit zone in Scene view for easier level design.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Draw different colors based on state
        if (Application.isPlaying)
        {
            Gizmos.color = exitAvailable ? Color.green : Color.red;
        }
        else
        {
            Gizmos.color = requiresObjectiveComplete ? Color.yellow : Color.green;
        }

        // Draw the exit zone
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}