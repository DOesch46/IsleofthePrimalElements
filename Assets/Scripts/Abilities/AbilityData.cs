using UnityEngine;

/// <summary>
/// Defines the different types of abilities.
/// Your team can add more types here if needed.
/// </summary>
public enum AbilityType
{
    Projectile,     // Shoots something (fireball, water bolt, etc.)
    AreaEffect,     // Affects an area (ground slam, lightning strike)
    Buff,           // Temporary boost (speed, shield, damage up)
    Melee           // Close range attack (sword swing, earth punch)
}

/// <summary>
/// ScriptableObject that stores all data for a single ability.
/// Create one asset for each ability in the game.
/// 
/// To create: Right-click in Project → Create → Game → Ability Data
/// 
/// Examples:
/// - Fireball: Projectile, Fire element, 20 damage
/// - Tidal Wave: AreaEffect, Water element, 15 damage
/// - Stone Shield: Buff, Earth element, +50 defense for 5 seconds
/// - Lightning Bolt: Projectile, Lightning element, 25 damage
/// </summary>
[CreateAssetMenu(fileName = "NewAbility", menuName = "Game/Ability Data", order = 2)]
public class AbilityData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Name of the ability shown to the player")]
    public string abilityName = "New Ability";
    
    [Tooltip("Description of what the ability does")]
    [TextArea(2, 4)]
    public string description = "";
    
    [Tooltip("Icon shown in UI")]
    public Sprite icon;

    [Header("Element & Type")]
    [Tooltip("Which element does this ability belong to?")]
    public ElementType element = ElementType.None;
    
    [Tooltip("What kind of ability is this?")]
    public AbilityType abilityType = AbilityType.Projectile;

    [Header("Combat Stats")]
    [Tooltip("Base damage dealt by this ability")]
    public int damage = 10;
    
    [Tooltip("Cooldown time in seconds between uses")]
    public float cooldown = 1f;
    
    [Tooltip("How long the effect lasts (for buffs and area effects)")]
    public float duration = 0f;
    
    [Tooltip("Range or size of the ability")]
    public float range = 5f;

    [Header("Projectile Settings (Only for Projectile type)")]
    [Tooltip("How fast the projectile moves")]
    public float projectileSpeed = 10f;
    
    [Tooltip("Prefab to spawn as the projectile")]
    public GameObject projectilePrefab;

    [Header("Area Effect Settings (Only for AreaEffect type)")]
    [Tooltip("Radius of the area effect")]
    public float areaRadius = 3f;
    
    [Tooltip("Prefab to spawn as the area effect visual")]
    public GameObject areaEffectPrefab;

    [Header("Buff Settings (Only for Buff type)")]
    [Tooltip("How much to boost the stat")]
    public float buffAmount = 0f;
    
    [Tooltip("What stat does this buff affect?")]
    public BuffStat buffStat = BuffStat.None;

    [Header("Visual & Audio")]
    [Tooltip("Prefab for the visual effect when ability is used")]
    public GameObject castEffectPrefab;
    
    [Tooltip("Color tint for the ability visuals")]
    public Color abilityColor = Color.white;

    /// <summary>
    /// Gets the element color for this ability.
    /// </summary>
    public Color GetElementColor()
    {
        switch (element)
        {
            case ElementType.Fire:
                return new Color(1f, 0.4f, 0f);
            case ElementType.Water:
                return new Color(0f, 0.6f, 1f);
            case ElementType.Earth:
                return new Color(0.6f, 0.4f, 0.2f);
            case ElementType.Lightning:
                return new Color(1f, 1f, 0f);
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Checks if this ability is strong against a given element.
    /// Water beats Fire, Fire beats Earth, etc.
    /// </summary>
    public bool IsStrongAgainst(ElementType targetElement)
    {
        switch (element)
        {
            case ElementType.Fire:
                return targetElement == ElementType.Earth;
            case ElementType.Water:
                return targetElement == ElementType.Fire;
            case ElementType.Earth:
                return targetElement == ElementType.Lightning;
            case ElementType.Lightning:
                return targetElement == ElementType.Water;
            default:
                return false;
        }
    }

    /// <summary>
    /// Returns the damage multiplier when used against a specific element.
    /// 2x damage if strong against, 0.5x if weak, 1x if neutral.
    /// </summary>
    public float GetDamageMultiplier(ElementType targetElement)
    {
        if (IsStrongAgainst(targetElement))
        {
            return 2f; // Double damage
        }
        
        // Check if weak against
        switch (element)
        {
            case ElementType.Fire when targetElement == ElementType.Water:
            case ElementType.Water when targetElement == ElementType.Lightning:
            case ElementType.Earth when targetElement == ElementType.Fire:
            case ElementType.Lightning when targetElement == ElementType.Earth:
                return 0.5f; // Half damage
            default:
                return 1f; // Normal damage
        }
    }
}

/// <summary>
/// Stats that can be buffed by buff-type abilities.
/// </summary>
public enum BuffStat
{
    None,
    Health,         // Increase max health
    Speed,          // Increase movement speed
    Defense,        // Reduce damage taken
    AttackDamage,   // Increase damage dealt
    CooldownReduction // Reduce ability cooldowns
}