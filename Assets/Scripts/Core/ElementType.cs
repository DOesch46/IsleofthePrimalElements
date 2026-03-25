/// <summary>
/// Defines all elemental types in the game.
/// Used for tracking player powers, level themes, and combat interactions.
/// 
/// Element Strengths (from your design doc):
/// - Earth beats Lightning
/// - Lightning beats Water
/// - Water beats Fire
/// - Fire beats Earth
/// </summary>
public enum ElementType
{
    None,       // No element (used for Hub World, intro areas)
    Fire,       // Pyronis - Lord of the Flames
    Water,      // Aqualis - Keeper of the Tides
    Earth,      // Terradon - Warden of the Earth
    Lightning   // Voltaris - Herald of the Storm
}