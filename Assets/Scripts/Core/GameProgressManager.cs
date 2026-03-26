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
    [SerializeField] private int debugCoinCount = 0;

    // =====================================================
    // PRIVATE DATA
    // =====================================================

    private HashSet<string> completedLevels = new HashSet<string>();
    private HashSet<ElementType> collectedElements = new HashSet<ElementType>();
    private int totalCoins = 0;

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
    // COIN TRACKING
    // =====================================================

    /// <summary>
    /// Adds coins to the player's total.
    /// Call this from your coin pickup script:
    /// GameProgressManager.Instance.AddCoins(1);
    /// </summary>
    public void AddCoins(int amount)
    {
        totalCoins += amount;
        debugCoinCount = totalCoins;
        Debug.Log($"Coins collected: {totalCoins}");

        if (enableSaving)
        {
            SaveProgress();
        }
    }

    /// <summary>
    /// Removes coins from the player's total.
    /// Returns true if the player had enough coins.
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            totalCoins -= amount;
            debugCoinCount = totalCoins;
            Debug.Log($"Spent {amount} coins. Remaining: {totalCoins}");

            if (enableSaving)
            {
                SaveProgress();
            }
            return true;
        }
        else
        {
            Debug.Log($"Not enough coins! Have {totalCoins}, need {amount}.");
            return false;
        }
    }

    /// <summary>
    /// Gets the current coin count.
    /// </summary>
    public int GetCoins()
    {
        return totalCoins;
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

    // =====================================================
    // LEVEL LOADING
    // =====================================================

    /// <summary>
    /// Attempts to load a level using its LevelData.
    /// Called by BiomePortal and LevelExit.
    /// </summary>
    public bool TryLoadLevel(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogWarning("TryLoadLevel: LevelData is null!");
            return false;
        }

        if (string.IsNullOrEmpty(levelData.sceneName))
        {
            Debug.LogWarning($"TryLoadLevel: '{levelData.levelName}' has no scene name assigned!");
            return false;
        }

        Debug.Log($"Loading level: {levelData.levelName} (Scene: {levelData.sceneName})");
        SceneManager.LoadScene(levelData.sceneName);
        return true;
    }

    /// <summary>
    /// Attempts to load a level by scene name directly.
    /// </summary>
    public bool TryLoadLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("TryLoadLevel: Scene name is null or empty!");
            return false;
        }

        Debug.Log($"Loading level: {sceneName}");
        SceneManager.LoadScene(sceneName);
        return true;
    }

    // =====================================================
    // SAVE / LOAD
    // =====================================================

    private void SaveProgress()
    {
        // Save completed levels
        string levelsSave = string.Join(",", completedLevels);
        PlayerPrefs.SetString(saveKeyPrefix + "CompletedLevels", levelsSave);

        // Save collected elements
        List<string> elementStrings = new List<string>();
        foreach (ElementType element in collectedElements)
        {
            elementStrings.Add(element.ToString());
        }
        string elementsSave = string.Join(",", elementStrings);
        PlayerPrefs.SetString(saveKeyPrefix + "CollectedElements", elementsSave);

        // Save coins
        PlayerPrefs.SetInt(saveKeyPrefix + "TotalCoins", totalCoins);

        PlayerPrefs.Save();
        Debug.Log("Progress saved.");
    }

    private void LoadProgress()
    {
        // Load completed levels
        string levelsSave = PlayerPrefs.GetString(saveKeyPrefix + "CompletedLevels", "");
        if (!string.IsNullOrEmpty(levelsSave))
        {
            string[] levels = levelsSave.Split(',');
            foreach (string level in levels)
            {
                if (!string.IsNullOrEmpty(level))
                {
                    completedLevels.Add(level);
                }
            }
        }

        // Load collected elements
        string elementsSave = PlayerPrefs.GetString(saveKeyPrefix + "CollectedElements", "");
        if (!string.IsNullOrEmpty(elementsSave))
        {
            string[] elements = elementsSave.Split(',');
            foreach (string element in elements)
            {
                if (System.Enum.TryParse(element, out ElementType parsedElement))
                {
                    collectedElements.Add(parsedElement);
                }
            }
        }

        // Load coins
        totalCoins = PlayerPrefs.GetInt(saveKeyPrefix + "TotalCoins", 0);
        debugCoinCount = totalCoins;

        Debug.Log($"Progress loaded. Levels: {completedLevels.Count}, Elements: {collectedElements.Count}, Coins: {totalCoins}");
    }

    // =====================================================
    // DEBUG HELPERS
    // =====================================================

    private void UpdateDebugLists()
    {
        completedLevelNames.Clear();
        foreach (string level in completedLevels)
        {
            completedLevelNames.Add(level);
        }

        collectedElementNames.Clear();
        foreach (ElementType element in collectedElements)
        {
            collectedElementNames.Add(element.ToString());
        }

        debugCoinCount = totalCoins;
    }

    /// <summary>
    /// Resets all progress. Use for testing or new game.
    /// </summary>
    public void ResetAllProgress()
    {
        completedLevels.Clear();
        collectedElements.Clear();
        totalCoins = 0;

        UpdateDebugLists();

        if (enableSaving)
        {
            PlayerPrefs.DeleteKey(saveKeyPrefix + "CompletedLevels");
            PlayerPrefs.DeleteKey(saveKeyPrefix + "CollectedElements");
            PlayerPrefs.DeleteKey(saveKeyPrefix + "TotalCoins");
            PlayerPrefs.Save();
        }

        Debug.Log("All progress has been reset.");
    }
}