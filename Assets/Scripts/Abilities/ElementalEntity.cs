using UnityEngine;

/// <summary>
/// Marks a GameObject as having an elemental type.
/// Put this on enemies, bosses, or any object that should
/// take elemental damage.
/// 
/// The ability system checks this to calculate damage multipliers:
/// - Water vs Fire enemy = 2x damage
/// - Fire vs Water enemy = 0.5x damage
/// </summary>
public class ElementalEntity : MonoBehaviour
{
    [Header("Element Settings")]
    [Tooltip("What element is this entity?")]
    [SerializeField] private ElementType element = ElementType.None;

    /// <summary>
    /// This entity's element type.
    /// </summary>
    public ElementType Element => element;

    /// <summary>
    /// Check if this entity is weak to a specific element.
    /// </summary>
    public bool IsWeakTo(ElementType attackElement)
    {
        switch (element)
        {
            case ElementType.Fire:
                return attackElement == ElementType.Water;
            case ElementType.Water:
                return attackElement == ElementType.Lightning;
            case ElementType.Earth:
                return attackElement == ElementType.Fire;
            case ElementType.Lightning:
                return attackElement == ElementType.Earth;
            default:
                return false;
        }
    }
}