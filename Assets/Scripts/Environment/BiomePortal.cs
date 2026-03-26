using UnityEngine;

/// <summary>
/// A portal on the MainIsland hub world that takes the player to an elemental level.
/// Place one of these at each biome entrance on the map.
/// 
/// Features:
/// - Detects player entering the portal
/// - Checks if portal is locked or available
/// - Loads the assigned level
/// - Can be locked/unlocked by the PortalManager
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BiomePortal : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Which level does this portal lead to? Drag a LevelData asset here.")]
    [SerializeField] private LevelData destinationLevel;
    
    [Tooltip("Display name for this portal (shown in UI later)")]
    [SerializeField] private string portalName = "Portal";

    [Header("Lock Settings")]
    [Tooltip("Is this portal available at the start of the game?")]
    [SerializeField] private bool startsUnlocked = true;
    
    [Tooltip("If true, this portal requires all 4 elements (for boss portal)")]
    [SerializeField] private bool requiresAllElements = false;
    
    [Tooltip("How many coins the player needs to unlock portals")]
    [SerializeField] private int requiredCoins = 10;

    [Header("Visual Settings")]
    [Tooltip("The SpriteRenderer for this portal")]
    [SerializeField] private SpriteRenderer portalSprite;
    
    [Tooltip("Color when portal is locked/unavailable")]
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    
    [Tooltip("Color when portal is available")]
    [SerializeField] private Color availableColor = new Color(1f, 1f, 1f, 1f);

    // Internal state
    private bool isLocked = true;
    private bool hasBeenChosen = false;
    private static bool anyPortalChosen = false;

    /// <summary>
    /// Is this portal currently available to enter?
    /// </summary>
    public bool IsLocked => isLocked;

    /// <summary>
    /// The level this portal leads to.
    /// </summary>
    public LevelData DestinationLevel => destinationLevel;

    /// <summary>
    /// The display name of this portal.
    /// </summary>
    public string PortalName => portalName;

    /// <summary>
    /// Static method to reset portal choice (call when returning to hub).
    /// </summary>
    public static void ResetPortalChoice()
    {
        anyPortalChosen = false;
    }

    private void Start()
    {
        // Get sprite renderer if not assigned
        if (portalSprite == null)
        {
            portalSprite = GetComponent<SpriteRenderer>();
        }

        // Ensure collider is a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }

        // Reset static variable when hub loads
        anyPortalChosen = false;

        // Check initial lock state
        UpdatePortalState();
    }

    /// <summary>
    /// Updates whether this portal is locked or unlocked based on game state.
    /// </summary>
    public void UpdatePortalState()
    {
        // Boss portal has special requirements
        if (requiresAllElements)
        {
            if (GameProgressManager.Instance != null)
            {
                isLocked = !GameProgressManager.Instance.HasAllElements();
            }
            else
            {
                isLocked = true;
            }
        }
        // Already completed levels stay unlocked but show as completed
        else if (destinationLevel != null && GameProgressManager.Instance != null 
                 && GameProgressManager.Instance.IsLevelCompleted(destinationLevel))
        {
            isLocked = false;
        }
        // Normal portal - unlocked if player meets requirements
        else if (startsUnlocked)
        {
            isLocked = false;
        }

        // Update visuals
        UpdateVisuals();
    }

    /// <summary>
    /// Unlocks this portal (called by PortalManager when coins are collected).
    /// </summary>
    public void Unlock()
    {
        isLocked = false;
        UpdateVisuals();
        Debug.Log($"Portal unlocked: {portalName}");
    }

    /// <summary>
    /// Locks this portal.
    /// </summary>
    public void Lock()
    {
        isLocked = true;
        UpdateVisuals();
        Debug.Log($"Portal locked: {portalName}");
    }

    /// <summary>
    /// Updates the portal visual based on state.
    /// </summary>
    private void UpdateVisuals()
    {
        if (portalSprite == null) return;

        if (isLocked)
        {
            portalSprite.color = lockedColor;
        }
        else
        {
            portalSprite.color = availableColor;
        }
    }

    /// <summary>
    /// Called when player enters the portal trigger.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Debug.Log($"Player touched portal: {portalName}");

        // Check if portal is locked
        if (isLocked)
        {
            OnPortalLocked();
            return;
        }

        // Check if another portal was already chosen this visit
        if (anyPortalChosen)
        {
            Debug.Log("A portal has already been chosen!");
            return;
        }

        // Check boss portal requirements
        if (requiresAllElements)
        {
            if (GameProgressManager.Instance == null || !GameProgressManager.Instance.HasAllElements())
            {
                Debug.Log("You need all 4 elements to enter Zerath's Fortress!");
                return;
            }
        }

        // Enter the portal!
        EnterPortal();
    }

    /// <summary>
    /// Processes entering the portal and loading the level.
    /// </summary>
    private void EnterPortal()
    {
        if (destinationLevel == null)
        {
            Debug.LogError($"Portal {portalName} has no destination level assigned!");
            return;
        }

        if (GameProgressManager.Instance == null)
        {
            Debug.LogError("GameProgressManager not found!");
            return;
        }

        // Mark that a portal has been chosen
        anyPortalChosen = true;
        hasBeenChosen = true;

        Debug.Log($"Entering portal: {portalName} → {destinationLevel.levelName}");

        // Load the destination level
        GameProgressManager.Instance.TryLoadLevel(destinationLevel);
    }

    /// <summary>
    /// Called when player tries to enter a locked portal.
    /// </summary>
    private void OnPortalLocked()
    {
        if (requiresAllElements)
        {
            int count = GameProgressManager.Instance != null 
                ? GameProgressManager.Instance.GetElementCount() : 0;
            Debug.Log($"Zerath's Fortress requires all 4 elements! You have {count}/4.");
        }
        else
        {
            Debug.Log($"Portal '{portalName}' is locked! Collect {requiredCoins} coins to unlock portals.");
        }
    }

    /// <summary>
    /// Draws portal info in Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = isLocked ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}