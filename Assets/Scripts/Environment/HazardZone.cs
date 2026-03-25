using UnityEngine;
using System;

/// <summary>
/// Placed on environmental hazard objects (fire tiles, electric fields, water zones).
/// Fires damage events that the health/progression system can subscribe to.
/// Works in conjunction with CollisionHandler on the player.
/// 
/// Usage: Attach to any GameObject on the "Hazard" physics layer.
///        Set the hazard type to match your biome (Fire, Water, Electric, Earth).
/// </summary>
public class HazardZone : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Hazard Type Definition
    // -------------------------------------------------------------------------

    public enum HazardType
    {
        Fire,
        Water,
        Electric,
        Earth,
        Generic
    }

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>Fired when a player enters this hazard. Passes damage per second and hazard type.</summary>
    public static event Action<float, HazardType> OnPlayerEnterHazard;

    /// <summary>Fired when a player exits this hazard.</summary>
    public static event Action<HazardType> OnPlayerExitHazard;

    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Hazard Settings")]
    [SerializeField] private HazardType hazardType = HazardType.Generic;
    [SerializeField] private float damagePerSecond = 10f;

    // -------------------------------------------------------------------------
    // Collision Callbacks
    // -------------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnterHazard?.Invoke(damagePerSecond, hazardType);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerExitHazard?.Invoke(hazardType);
        }
    }
}
