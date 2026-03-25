using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Defines the different types of objectives a level can have.
/// </summary>
public enum ObjectiveType
{
    DefeatBoss,     // Kill the boss to complete the level
    DefeatAllEnemies, // Kill all enemies in the level
    ReachExit,      // Just get to the exit (no combat required)
    CollectItems    // Collect a certain number of items
}

/// <summary>
/// Tracks level objectives and unlocks the exit when complete.
/// Place ONE of these in each level.
/// 
/// Other scripts call LevelObjective.Instance.CompleteObjective() when
/// the boss is defeated or the objective is met.
/// </summary>
public class LevelObjective : MonoBehaviour
{
    // =====================================================
    // SINGLETON (Per-Scene, NOT DontDestroyOnLoad)
    // =====================================================
    
    // Each level has its own LevelObjective instance
    private static LevelObjective instance;
    
    /// <summary>
    /// Access the LevelObjective for the current scene.
    /// Example: LevelObjective.Instance.CompleteObjective()
    /// </summary>
    public static LevelObjective Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<LevelObjective>();
            }
            return instance;
        }
    }

    // =====================================================
    // INSPECTOR SETTINGS
    // =====================================================

    [Header("Objective Settings")]
    [Tooltip("What type of objective is this level?")]
    [SerializeField] private ObjectiveType objectiveType = ObjectiveType.DefeatBoss;
    
    [Tooltip("Description shown to the player (optional, for UI later)")]
    [SerializeField] private string objectiveDescription = "Defeat the boss to proceed.";

    [Header("Collection Objective (Only for CollectItems type)")]
    [Tooltip("How many items need to be collected (only used with CollectItems type)")]
    [SerializeField] private int requiredItemCount = 0;
    
    [Tooltip("Current items collected (visible for debugging)")]
    [SerializeField] private int currentItemCount = 0;

    [Header("Enemy Objective (Only for DefeatAllEnemies type)")]
    [Tooltip("How many enemies need to be defeated (only used with DefeatAllEnemies type)")]
    [SerializeField] private int requiredEnemyCount = 0;
    
    [Tooltip("Current enemies defeated (visible for debugging)")]
    [SerializeField] private int currentEnemyCount = 0;

    [Header("References")]
    [Tooltip("The LevelExit to unlock when objective is complete (drag it here)")]
    [SerializeField] private LevelExit levelExit;

    [Header("Events")]
    [Tooltip("Fires when the objective is completed. Hook up UI, sounds, etc.")]
    public UnityEvent OnObjectiveComplete;

    // =====================================================
    // PRIVATE DATA
    // =====================================================
    
    // Has the objective been completed?
    private bool isComplete = false;

    /// <summary>
    /// Check if the level objective has been completed.
    /// </summary>
    public bool IsComplete => isComplete;

    /// <summary>
    /// Get the objective description text.
    /// </summary>
    public string Description => objectiveDescription;

    /// <summary>
    /// Get the objective type.
    /// </summary>
    public ObjectiveType Type => objectiveType;

    // =====================================================
    // UNITY LIFECYCLE
    // =====================================================

    private void Awake()
    {
        // Set the scene-level singleton
        instance = this;
    }

    private void Start()
    {
        // Validate setup
        if (levelExit == null)
        {
            Debug.LogWarning($"LevelObjective on {gameObject.name}: No LevelExit assigned! " +
                             "The exit won't unlock automatically.");
        }

        // If objective type is ReachExit, it's already complete
        if (objectiveType == ObjectiveType.ReachExit)
        {
            CompleteObjective();
        }
        else
        {
            Debug.Log($"Level Objective: {objectiveDescription}");
        }
    }

    // =====================================================
    // OBJECTIVE COMPLETION
    // =====================================================

    /// <summary>
    /// Call this when the main objective is completed (boss defeated, etc.)
    /// Can be called from any script:
    /// LevelObjective.Instance.CompleteObjective();
    /// </summary>
    public void CompleteObjective()
    {
        // Don't complete twice
        if (isComplete)
        {
            Debug.Log("Objective was already completed.");
            return;
        }

        isComplete = true;
        
        Debug.Log($"*** OBJECTIVE COMPLETE: {objectiveDescription} ***");

        // Unlock the exit
        UnlockExit();

        // Fire the event (for UI, sounds, etc.)
        OnObjectiveComplete?.Invoke();
    }

    // =====================================================
    // BOSS DEFEAT (For DefeatBoss type)
    // =====================================================

    /// <summary>
    /// Call this when the boss is defeated.
    /// Example: In your boss script's Die() method, call:
    /// LevelObjective.Instance.BossDefeated();
    /// </summary>
    public void BossDefeated()
    {
        if (objectiveType != ObjectiveType.DefeatBoss)
        {
            Debug.LogWarning("BossDefeated() called but objective type is not DefeatBoss!");
        }

        Debug.Log("Boss has been defeated!");
        CompleteObjective();
    }

    // =====================================================
    // ENEMY DEFEAT (For DefeatAllEnemies type)
    // =====================================================

    /// <summary>
    /// Call this each time an enemy is defeated.
    /// Example: In your enemy script's Die() method, call:
    /// LevelObjective.Instance.EnemyDefeated();
    /// </summary>
    public void EnemyDefeated()
    {
        if (objectiveType != ObjectiveType.DefeatAllEnemies)
        {
            return; // Not tracking enemies for this objective type
        }

        currentEnemyCount++;
        
        Debug.Log($"Enemy defeated! {currentEnemyCount}/{requiredEnemyCount}");

        // Check if all enemies are defeated
        if (currentEnemyCount >= requiredEnemyCount)
        {
            Debug.Log("All enemies defeated!");
            CompleteObjective();
        }
    }

    /// <summary>
    /// Sets the required enemy count at runtime.
    /// Useful if you want to count enemies in the scene automatically.
    /// </summary>
    public void SetRequiredEnemyCount(int count)
    {
        requiredEnemyCount = count;
        Debug.Log($"Required enemy count set to: {requiredEnemyCount}");
    }

    // =====================================================
    // ITEM COLLECTION (For CollectItems type)
    // =====================================================

    /// <summary>
    /// Call this each time the player collects a required item.
    /// Example: In your item pickup script, call:
    /// LevelObjective.Instance.ItemCollected();
    /// </summary>
    public void ItemCollected()
    {
        if (objectiveType != ObjectiveType.CollectItems)
        {
            return; // Not tracking items for this objective type
        }

        currentItemCount++;
        
        Debug.Log($"Item collected! {currentItemCount}/{requiredItemCount}");

        // Check if all items are collected
        if (currentItemCount >= requiredItemCount)
        {
            Debug.Log("All items collected!");
            CompleteObjective();
        }
    }

    // =====================================================
    // EXIT CONTROL
    // =====================================================

    /// <summary>
    /// Unlocks the level exit portal.
    /// </summary>
    private void UnlockExit()
    {
        if (levelExit != null)
        {
            levelExit.UnlockExit();
            Debug.Log("Level exit has been unlocked!");
        }
        else
        {
            Debug.LogWarning("No LevelExit assigned! Cannot unlock exit.");
        }
    }

    // =====================================================
    // PROGRESS HELPERS
    // =====================================================

    /// <summary>
    /// Returns a string showing current progress.
    /// Useful for UI display.
    /// </summary>
    public string GetProgressText()
    {
        switch (objectiveType)
        {
            case ObjectiveType.DefeatBoss:
                return isComplete ? "Boss Defeated!" : "Defeat the Boss";
                
            case ObjectiveType.DefeatAllEnemies:
                return $"Enemies: {currentEnemyCount}/{requiredEnemyCount}";
                
            case ObjectiveType.CollectItems:
                return $"Items: {currentItemCount}/{requiredItemCount}";
                
            case ObjectiveType.ReachExit:
                return "Find the Exit";
                
            default:
                return objectiveDescription;
        }
    }

    /// <summary>
    /// Returns completion progress as a float from 0 to 1.
    /// Useful for progress bars.
    /// </summary>
    public float GetProgressPercent()
    {
        switch (objectiveType)
        {
            case ObjectiveType.DefeatBoss:
                return isComplete ? 1f : 0f;
                
            case ObjectiveType.DefeatAllEnemies:
                if (requiredEnemyCount <= 0) return 1f;
                return (float)currentEnemyCount / requiredEnemyCount;
                
            case ObjectiveType.CollectItems:
                if (requiredItemCount <= 0) return 1f;
                return (float)currentItemCount / requiredItemCount;
                
            case ObjectiveType.ReachExit:
                return 1f;
                
            default:
                return isComplete ? 1f : 0f;
        }
    }

    // =====================================================
    // DEBUG
    // =====================================================

    /// <summary>
    /// Debug method to force complete the objective.
    /// Right-click component → Debug: Force Complete
    /// </summary>
    [ContextMenu("Debug: Force Complete Objective")]
    public void DebugForceComplete()
    {
        CompleteObjective();
    }

    /// <summary>
    /// Debug method to reset the objective.
    /// </summary>
    [ContextMenu("Debug: Reset Objective")]
    public void DebugReset()
    {
        isComplete = false;
        currentItemCount = 0;
        currentEnemyCount = 0;
        
        if (levelExit != null)
        {
            levelExit.LockExit();
        }
        
        Debug.Log("Objective reset!");
    }
}