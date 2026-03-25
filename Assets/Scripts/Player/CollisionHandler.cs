using UnityEngine;
using System;

/// <summary>
/// Listens for physics collision and trigger events on the player.
/// Separates collision concern from movement and interaction.
/// Fires events that other systems (e.g. health, progression) can subscribe to.
/// </summary>
public class CollisionHandler : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Events — other systems subscribe, CollisionHandler just fires
    // -------------------------------------------------------------------------

    /// <summary>Fired when the player enters a hazard zone. Passes the hazard tag.</summary>
    public event Action<string> OnHazardEntered;

    /// <summary>Fired when the player exits a hazard zone.</summary>
    public event Action<string> OnHazardExited;

    /// <summary>Fired when the player lands on the ground after being airborne.</summary>
    public event Action OnLanded;

    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Layer Names")]
    [SerializeField] private string hazardLayerName = "Hazard";
    [SerializeField] private string groundLayerName = "Ground";

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private int hazardLayer;
    private int groundLayer;
    private bool wasGroundedLastFrame;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        hazardLayer = LayerMask.NameToLayer(hazardLayerName);
        groundLayer = LayerMask.NameToLayer(groundLayerName);
    }

<<<<<<< HEAD
=======
    private void Update()
    {
        //CheckLanding();
    }

>>>>>>> 6b926bfbf79a68d9a269668a7632a556cba44737
    // -------------------------------------------------------------------------
    // Collision Callbacks
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called when the player enters a trigger collider (e.g. hazard zones,
    /// item pickups, interaction areas).
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == hazardLayer)
        {
            OnHazardEntered?.Invoke(other.tag);
        }
    }

    /// <summary>
    /// Called when the player exits a trigger collider.
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == hazardLayer)
        {
            OnHazardExited?.Invoke(other.tag);
        }
    }

    /// <summary>
    /// Called when the player makes physical contact (e.g. walls, platforms).
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Additional collision logic can be handled here in future tasks
        // e.g. bouncy platforms, pushable crates, etc.
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Allows external systems to register the player's grounded state
    /// so CollisionHandler can detect the moment of landing.
    /// Called each frame by PlayerController.
    /// </summary>
    public void UpdateGroundedState(bool isGrounded)
    {
        if (isGrounded && !wasGroundedLastFrame)
        {
            OnLanded?.Invoke();
        }
        wasGroundedLastFrame = isGrounded;
    }
}
