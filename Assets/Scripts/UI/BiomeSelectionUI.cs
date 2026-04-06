using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel that appears when player enters the portal.
/// Shows available biomes for the player to choose from.
/// </summary>
public class BiomeSelectionUI : MonoBehaviour
{
    [Header("Panel")]
    [Tooltip("The main selection panel that shows/hides")]
    [SerializeField] private GameObject selectionPanel;

    [Header("Biome Buttons")]
    [SerializeField] private Button fireButton;
    [SerializeField] private Button waterButton;
    [SerializeField] private Button earthButton;
    [SerializeField] private Button lightningButton;
    [SerializeField] private Button bossButton;
    [SerializeField] private Button closeButton;

    [Header("Biome Button Text (Optional)")]
    [SerializeField] private TextMeshProUGUI fireButtonText;
    [SerializeField] private TextMeshProUGUI waterButtonText;
    [SerializeField] private TextMeshProUGUI earthButtonText;
    [SerializeField] private TextMeshProUGUI lightningButtonText;
    [SerializeField] private TextMeshProUGUI bossButtonText;

    [Header("Info Text")]
    [Tooltip("Text that shows instructions or element count")]
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Level Data References")]
    [SerializeField] private LevelData fireLevelData;
    [SerializeField] private LevelData waterLevelData;
    [SerializeField] private LevelData earthLevelData;
    [SerializeField] private LevelData lightningLevelData;
    [SerializeField] private LevelData bossLevelData;

    [Header("Button Colors")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color completedColor = new Color(0.5f, 1f, 0.5f); // Green tint
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f);

    // Singleton for easy access
    private static BiomeSelectionUI instance;
    public static BiomeSelectionUI Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Hide panel at start
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }

        // Set up button listeners
        SetupButtons();
    }

    /// <summary>
    /// Connects buttons to their functions.
    /// </summary>
    private void SetupButtons()
    {
        if (fireButton != null)
            fireButton.onClick.AddListener(() => SelectBiome(fireLevelData));

        if (waterButton != null)
            waterButton.onClick.AddListener(() => SelectBiome(waterLevelData));

        if (earthButton != null)
            earthButton.onClick.AddListener(() => SelectBiome(earthLevelData));

        if (lightningButton != null)
            lightningButton.onClick.AddListener(() => SelectBiome(lightningLevelData));

        if (bossButton != null)
            bossButton.onClick.AddListener(() => SelectBiome(bossLevelData));

        if (closeButton != null)
            closeButton.onClick.AddListener(HideSelection);
    }

    /// <summary>
    /// Shows the biome selection panel and updates button states.
    /// Called by the portal when player enters it.
    /// </summary>
    public void ShowSelection()
    {
        if (selectionPanel == null)
        {
            Debug.LogError("BiomeSelectionUI: Selection panel is not assigned!");
            return;
        }

        // Show the panel
        selectionPanel.SetActive(true);

        // Pause the game while selecting
        Time.timeScale = 0f;

        // Update all button states
        UpdateButtonStates();

        // Update info text
        UpdateInfoText();

        Debug.Log("Biome selection panel opened.");
    }

    /// <summary>
    /// Hides the biome selection panel.
    /// </summary>
    public void HideSelection()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }

        // Unpause the game
        Time.timeScale = 1f;

        Debug.Log("Biome selection panel closed.");
    }

    /// <summary>
    /// Updates each button to show if the biome is available, completed, or locked.
    /// </summary>
    private void UpdateButtonStates()
    {
        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("GameProgressManager not found!");
            return;
        }

        // Update each biome button
        UpdateSingleButton(fireButton, fireButtonText, fireLevelData, "Fire - Pyronis Domain");
        UpdateSingleButton(waterButton, waterButtonText, waterLevelData, "Water - Aqualis Depths");
        UpdateSingleButton(earthButton, earthButtonText, earthLevelData, "Earth - Terradon Peaks");
        UpdateSingleButton(lightningButton, lightningButtonText, lightningLevelData, "Lightning - Voltaris Spire");

        // Boss button has special logic
        UpdateBossButton();
    }

    /// <summary>
    /// Updates a single biome button's appearance and interactability.
    /// </summary>
    private void UpdateSingleButton(Button button, TextMeshProUGUI buttonText, 
                                      LevelData levelData, string displayName)
    {
        if (button == null || levelData == null) return;

        bool isCompleted = GameProgressManager.Instance.IsLevelCompleted(levelData);
        bool isUnlocked = GameProgressManager.Instance.IsLevelUnlocked(levelData);

        // Set button interactable
        button.interactable = isUnlocked && !isCompleted;

        // Set button color
        ColorBlock colors = button.colors;
        if (isCompleted)
        {
            colors.normalColor = completedColor;
            if (buttonText != null)
                buttonText.text = displayName + " ✓ COMPLETED";
        }
        else if (isUnlocked)
        {
            colors.normalColor = availableColor;
            if (buttonText != null)
                buttonText.text = displayName;
        }
        else
        {
            colors.normalColor = lockedColor;
            if (buttonText != null)
                buttonText.text = displayName + " (LOCKED)";
        }
        button.colors = colors;
    }

    /// <summary>
    /// Updates the boss button with special requirements.
    /// </summary>
    private void UpdateBossButton()
    {
        if (bossButton == null) return;

        bool hasAll = GameProgressManager.Instance.HasAllElements();
        int elementCount = GameProgressManager.Instance.GetElementCount();

        bossButton.interactable = hasAll;

        ColorBlock colors = bossButton.colors;
        if (hasAll)
        {
            colors.normalColor = availableColor;
            if (bossButtonText != null)
                bossButtonText.text = "Zerath's Fortress - ENTER";
        }
        else
        {
            colors.normalColor = lockedColor;
            if (bossButtonText != null)
                bossButtonText.text = $"Zerath's Fortress ({elementCount}/4 Elements)";
        }
        bossButton.colors = colors;
    }

    /// <summary>
    /// Updates the info text at the top of the panel.
    /// </summary>
    private void UpdateInfoText()
    {
        if (infoText == null) return;

        int elementCount = GameProgressManager.Instance != null 
            ? GameProgressManager.Instance.GetElementCount() : 0;

        if (elementCount == 0)
        {
            infoText.text = "Choose your path! Enter a biome to challenge its guardian.";
        }
        else if (elementCount < 4)
        {
            infoText.text = $"Elements collected: {elementCount}/4. Keep going!";
        }
        else
        {
            infoText.text = "All elements collected! Zerath's Fortress awaits!";
        }
    }

    /// <summary>
    /// Called when player selects a biome.
    /// </summary>
    private void SelectBiome(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("No level data assigned for this biome!");
            return;
        }

        // Unpause before loading
        Time.timeScale = 1f;

        // Hide the panel
        selectionPanel.SetActive(false);

        Debug.Log($"Player selected: {levelData.levelName}");

        // Load the level
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.TryLoadLevel(levelData);
        }
    }
}