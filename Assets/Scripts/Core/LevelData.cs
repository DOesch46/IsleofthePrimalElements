using UnityEngine;

/// <summary>
/// ScriptableObject that stores data about a single level.
/// Create one asset for each level: Fire, Water, Earth, Lightning, Hub, Zerath's Fortress.
/// 
/// To create: Right-click in Project → Create → Game → Level Data
/// </summary>
[CreateAssetMenu(fileName = "NewLevelData", menuName = "Game/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    [Header("Level Identity")]
    [Tooltip("Display name shown to player (e.g., 'Pyronis Domain')")]
    public string levelName = "New Level";
    
    [Tooltip("The exact name of the scene file in Build Settings (e.g., 'FireLevel')")]
    public string sceneName = "";
    
    [Tooltip("Brief description of this level")]
    [TextArea(2, 4)]
    public string description = "";

    [Header("Element Settings")]
    [Tooltip("Which element does this level represent? Use 'None' for Hub or intro areas.")]
    public ElementType elementType = ElementType.None;
    
    [Tooltip("Name of the boss in this level (leave empty if no boss)")]
    public string bossName = "";

    [Header("Progression Settings")]
    [Tooltip("Is this level available from the start of the game?")]
    public bool unlockedByDefault = false;
    
    [Tooltip("Does this level require all 4 elements to unlock? (For Zerath's Fortress)")]
    public bool requiresAllElements = false;
    
    [Tooltip("Levels that must be completed before this one unlocks (optional)")]
    public LevelData[] requiredLevels;

    [Header("Hub Portal Settings")]
    [Tooltip("Position offset for this level's portal in the hub world (optional)")]
    public Vector2 hubPortalPosition = Vector2.zero;
    
    [Tooltip("Color tint for this level's portal (helps identify elements)")]
    public Color portalColor = Color.white;

    /// <summary>
    /// Returns true if this level has a boss to defeat.
    /// </summary>
    public bool HasBoss => !string.IsNullOrEmpty(bossName);

    /// <summary>
    /// Returns true if this is an elemental level (not hub or fortress).
    /// </summary>
    public bool IsElementalLevel => elementType != ElementType.None;

    /// <summary>
    /// Gets a color associated with this level's element.
    /// Useful for UI and visual feedback.
    /// </summary>
    public Color GetElementColor()
    {
        switch (elementType)
        {
            case ElementType.Fire:
                return new Color(1f, 0.4f, 0f);      // Orange-red
            case ElementType.Water:
                return new Color(0f, 0.6f, 1f);      // Blue
            case ElementType.Earth:
                return new Color(0.6f, 0.4f, 0.2f);  // Brown
            case ElementType.Lightning:
                return new Color(1f, 1f, 0f);        // Yellow
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Gets the element that this element is STRONG against.
    /// Based on your design: Water beats Fire, Fire beats Earth, etc.
    /// </summary>
    public ElementType GetWeakAgainst()
    {
        switch (elementType)
        {
            case ElementType.Fire:
                return ElementType.Water;
            case ElementType.Water:
                return ElementType.Lightning;
            case ElementType.Earth:
                return ElementType.Fire;
            case ElementType.Lightning:
                return ElementType.Earth;
            default:
                return ElementType.None;
        }
    }

    /// <summary>
    /// Gets the element that this element is WEAK to.
    /// </summary>
    public ElementType GetStrongAgainst()
    {
        switch (elementType)
        {
            case ElementType.Fire:
                return ElementType.Earth;
            case ElementType.Water:
                return ElementType.Fire;
            case ElementType.Earth:
                return ElementType.Lightning;
            case ElementType.Lightning:
                return ElementType.Water;
            default:
                return ElementType.None;
        }
    }
}