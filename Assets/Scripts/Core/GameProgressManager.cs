using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton manager that tracks all game progression.
/// Persists between scenes and handles saving/loading.
/// 
/// Access from anywhere using: GameProgressManager.Instance
/// </summary>
public class GameProgressManager : MonoBehaviour
{
    // =====================================================
    // SINGLETON PATTERN
    // =====================================================
    
    private static GameProgressManager instance;
    
    /// <summary>
    /// Access the GameProgressManager from anywhere in your code.
    /// Example: GameProgressManager.Instance.HasElement(ElementType.Fire)
    /// </summary>
    public static GameProgressManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameProgressManager>();
                
                if (instance == null)
                {
                    GameObject managerObject = new GameObject("GameProgressManager");
                    instance = managerObject.AddComponent<GameProgressManager>();
                    Debug.Log("GameProgressManager created automatically.");
                }
            }
            return instance;
        }
    }

    // =====================================================
    // INSPECTOR SETTINGS
    // =====================================================
    
    [Header("Level Data References")]
    [Tooltip("Drag all your LevelData assets here")]
    [SerializeField] private List<LevelData> allLevels = new List<LevelData>();

    [Header("Save Settings")]
    [Tooltip("Should progress be saved to PlayerPrefs?")]
    [SerializeField] private bool enableSaving = true;
    
    [Tooltip("Key prefix for PlayerPrefs saves")]
    [SerializeField] private string saveKeyPrefix = "Elementara_";

    [Header("Debug Info (Read Only)")]
    [SerializeField] private List<string> completedLevelNames = new List<string>();
    [SerializeField] private List<string> collectedElementNames = new List<string>();

    // =====================================================
    // PRIVATE DATA
    // =====================================================
    
    private HashSet<string> completedLevels = new HashSet<string>();
    private HashSet<ElementType> collectedElements = new HashSet<ElementType>();

    // =====================================================
    // UNITY LIFECYCLE
    // =====================================================

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Duplicate GameProgressManager found. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (enableSaving)
        {
            LoadProgress();
        }
        
        Debug.Log("GameProgressManager initialized.");
    }

    private void Start()
    {
        UpdateDebugLists();
    }

    // =====================================================
    // ELEMENT TRACKING
    // =====================================================

    /// <summary>
    /// Checks if the player has collected a specific element.
    /// </summary>
    public bool HasElement(ElementType element)
    {
        if (element == ElementType.None)
        {
            return true;
        }
        return collectedElements.Contains(element);
    }

    /// <summary>
    /// Grants an element to the player.
    /// Called when defeating an elemental boss.
    /// </summary>
    public void CollectElement(ElementType element)
    {
        if (element == ElementType.None)
        {
            return;
        }

        if (!collectedElements.Contains(element))
        {
            collectedElements.Add(element);
            Debug.Log($"Element collected: {element}!");
            
            UpdateDebugLists();
            
            if (enableSaving)
            {
                SaveProgress();
            }

            if (HasAllElements())
            {
                Debug.Log("ALL ELEMENTS COLLECTED! Zerath's Fortress is now accessible!");
            }
        }
        else
        {
            Debug.Log($"Element {element} was already collected.");
        }
    }

    /// <summary>
    /// Removes an element from the player.
    /// Called when losing to Zerath (per your game design).
    /// </summary>
    public void LoseElement(ElementType element)
    {
        if (collectedElements.Contains(element))
        {
            collectedElements.Remove(element);
            Debug.Log($"Element lost: {element}! You must reclaim it.");
            
            UpdateDebugLists();
            
            if (enableSaving)
            {
                SaveProgress();
            }
        }
    }

    /// <summary>
    /// Checks if the player has all 4 elements.
    /// Required to access Zerath's Fortress.
    /// </summary>
    public bool HasAllElements()
    {
        return HasElement(ElementType.Fire) &&
               HasElement(ElementType.Water) &&
               HasElement(ElementType.Earth) &&
               HasElement(ElementType.Lightning);
    }

    /// <summary>
    /// Gets the count of collected elements (0-4).
    /// </summary>
    public int GetElementCount()
    {
        return collectedElements.Count;
    }

    /// <summary>
    /// Gets a list of all collected elements.
    /// </summary>
    public List<ElementType> GetCollectedElements()
    {
        return new List<ElementType>(collectedElements);
    }

    // =====================================================
    // LEVEL COMPLETION TRACKING
    // =====================================================

    /// <summary>
    /// Checks if a specific level has been completed.
    /// </summary>
    public bool IsLevelCompleted(LevelData levelData)
    {
        if (levelData == null)
        {
            return false;
        }
        return completedLevels.Contains(levelData.sceneName);
    }

    /// <summary>
    /// Checks if a level is completed by scene name.
    /// </summary>
    public bool IsLevelCompleted(string sceneName)
    {
        return completedLevels.Contains(sceneName);
    }

    /// <summary>
    /// Marks a level as completed.
    /// Also collects the element if it's an elemental level.
    /// </summary>
    public void CompleteLevel(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogWarning("Tried to complete a null level!");
            return;
        }

        if (!completedLevels.Contains(levelData.sceneName))
        {
            completedLevels.Add(levelData.sceneName);
            Debug.Log($"Level completed: {levelData.levelName}!");
        }

        if (levelData.IsElementalLevel)
        {
            CollectElement(levelData.elementType);
        }

        UpdateDebugLists();

        if (enableSaving)
        {
            SaveProgress();
        }
    }

    /// <summary>
    /// Marks a level as incomplete.
    /// Used if player loses to Zerath and must redo a level.
    /// </summary>
    public void ResetLevelCompletion(LevelData levelData)
    {
        if (levelData == null)
        {
            return;
        }

        if (completedLevels.Contains(levelData.sceneName))
        {
            completedLevels.Remove(levelData.sceneName);
            Debug.Log($"Level reset: {levelData.levelName}");
            
            UpdateDebugLists();
            
            if (enableSaving)
            {
                SaveProgress();
            }
        }
    }

    // =====================================================
    // LEVEL UNLOCK CHECKING
    // =====================================================

    /// <summary>
    /// Checks if a level is unlocked and accessible.
    /// </summary>
    public bool IsLevelUnlocked(LevelData levelData)
    {
        if (levelData == null)
        {
            return false;
        }

        if (levelData.unlockedByDefault)
        {
            return true;
        }

        if (levelData.requiresAllElements)
        {
            return HasAllElements();
        }

        if (levelData.requiredLevels != null && levelData.requiredLevels.Length > 0)
        {
            foreach (LevelData required in levelData.requiredLevels)
            {
                if (!IsLevelCompleted(required))
                {
                    return false;
                }
            }
        }

        return true;
    }

    // =====================================================
    // SCENE LOADING
    // =====================================================

    /// <summary>
    /// Loads a level if it's unlocked.
    /// </summary>
    public bool TryLoadLevel(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogWarning("Tried to load a null level!");
            return false;
        }

        if (!IsLevelUnlocked(levelData))
        {
            Debug.Log($"Level '{levelData.levelName}' is locked!");
            return false;
        }

        Debug.Log($"Loading level: {levelData.levelName}");
        SceneManager.LoadScene(levelData.sceneName);
        return true;
    }

    /// <summary>
    /// Loads a scene directly by name.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    // =====================================================
    // LEVEL DATA HELPERS
    // =====================================================

    /// <summary>
    /// Finds a LevelData asset by its scene name.
    /// </summary>
    public LevelData GetLevelDataBySceneName(string sceneName)
    {
        foreach (LevelData level in allLevels)
        {
            if (level != null && level.sceneName == sceneName)
            {
                return level;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds a LevelData asset by element type.
    /// </summary>
    public LevelData GetLevelDataByElement(ElementType element)
    {
        foreach (LevelData level in allLevels)
        {
            if (level != null && level.elementType == element)
            {
                return level;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the LevelData for the current scene.
    /// </summary>
    public LevelData GetCurrentLevelData()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        return GetLevelDataBySceneName(currentScene);
    }

    // =====================================================
    // SAVE / LOAD SYSTEM
    // =====================================================

    /// <summary>
    /// Saves current progress to PlayerPrefs.
    /// </summary>
    public void SaveProgress()
    {
        string completedLevelsString = string.Join(",", completedLevels);
        PlayerPrefs.SetString(saveKeyPrefix + "CompletedLevels", completedLevelsString);

        List<string> elementStrings = new List<string>();
        foreach (ElementType element in collectedElements)
        {
            elementStrings.Add(element.ToString());
        }
        string elementsString = string.Join(",", elementStrings);
        PlayerPrefs.SetString(saveKeyPrefix + "CollectedElements", elementsString);

        PlayerPrefs.Save();
        
        Debug.Log("Progress saved!");
    }

    /// <summary>
    /// Loads saved progress from PlayerPrefs.
    /// </summary>
    public void LoadProgress()
    {
        completedLevels.Clear();
        collectedElements.Clear();

        string completedLevelsString = PlayerPrefs.GetString(saveKeyPrefix + "CompletedLevels", "");
        if (!string.IsNullOrEmpty(completedLevelsString))
        {
            string[] levels = completedLevelsString.Split(',');
            foreach (string level in levels)
            {
                if (!string.IsNullOrEmpty(level))
                {
                    completedLevels.Add(level);
                }
            }
        }

        string elementsString = PlayerPrefs.GetString(saveKeyPrefix + "CollectedElements", "");
        if (!string.IsNullOrEmpty(elementsString))
        {
            string[] elements = elementsString.Split(',');
            foreach (string elementName in elements)
            {
                if (System.Enum.TryParse(elementName, out ElementType element))
                {
                    collectedElements.Add(element);
                }
            }
        }

        UpdateDebugLists();
        Debug.Log($"Progress loaded! Completed levels: {completedLevels.Count}, Elements: {collectedElements.Count}");
    }

    /// <summary>
    /// Deletes all saved progress.
    /// </summary>
    public void ClearAllProgress()
    {
        completedLevels.Clear();
        collectedElements.Clear();
        
        PlayerPrefs.DeleteKey(saveKeyPrefix + "CompletedLevels");
        PlayerPrefs.DeleteKey(saveKeyPrefix + "CollectedElements");
        PlayerPrefs.Save();

        UpdateDebugLists();
        Debug.Log("All progress cleared!");
    }

    // =====================================================
    // DEBUG HELPERS
    // =====================================================

    private void UpdateDebugLists()
    {
        completedLevelNames.Clear();
        completedLevelNames.AddRange(completedLevels);

        collectedElementNames.Clear();
        foreach (ElementType element in collectedElements)
        {
            collectedElementNames.Add(element.ToString());
        }
    }

    [ContextMenu("Debug: Grant All Elements")]
    public void DebugGrantAllElements()
    {
        CollectElement(ElementType.Fire);
        CollectElement(ElementType.Water);
        CollectElement(ElementType.Earth);
        CollectElement(ElementType.Lightning);
        Debug.Log("DEBUG: All elements granted!");
    }

    [ContextMenu("Debug: Clear All Progress")]
    public void DebugClearProgress()
    {
        ClearAllProgress();
    }
}