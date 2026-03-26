using UnityEngine;
using System;

/// <summary>
/// Listens for physics collision and trigger events on the player.
/// Top-down version: no ground detection or landing events.
/// Fires events for hazard zones that other systems can subscribe to.
/// </summary>
public class CollisionHandler : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>Fired when the player enters a hazard zone. Passes the hazard tag.</summary>
    public event Action<string> OnHazardEntered;

    /// <summary>Fired when the player exits a hazard zone.</summary>
    public event Action<string> OnHazardExited;

    /// <summary>Fired when the player collides with a solid object (wall, obstacle).</summary>
    public event Action<GameObject> OnObstacleHit;

    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Layer Names")]
    [SerializeField] private string hazardLayerName = "Hazard";

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private int hazardLayer;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        hazardLayer = LayerMask.NameToLayer(hazardLayerName);
    }

    // -------------------------------------------------------------------------
    // Collision Callbacks
    // -------------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == hazardLayer)
            OnHazardEntered?.Invoke(other.tag);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == hazardLayer)
            OnHazardExited?.Invoke(other.tag);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnObstacleHit?.Invoke(collision.gameObject);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// No-op stub kept so PlayerController compiles without changes.
    /// Top-down games have no grounded state to track.
    /// </summary>
    public void UpdateGroundedState(bool isGrounded) { }
}
