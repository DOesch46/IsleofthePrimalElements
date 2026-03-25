using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Detects nearby IInteractable objects and triggers interactions on input.
/// Uses OverlapCircle to find candidates, picks the closest valid one,
/// and notifies the UI system to show/hide the interaction prompt.
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("References")]
    [SerializeField] private InteractionPromptUI promptUI;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private PlayerController playerController;
    private IInteractable currentTarget;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        RefreshTarget();
        HandleInteractInput();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the currently targeted interactable, or null if none is in range.
    /// </summary>
    public IInteractable GetCurrentTarget() => currentTarget;

    // -------------------------------------------------------------------------
    // Private Logic
    // -------------------------------------------------------------------------

    /// <summary>
    /// Each frame, scans nearby colliders for IInteractable components.
    /// Selects the closest valid one and updates the prompt UI.
    /// </summary>
    private void RefreshTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, interactableLayer);

        IInteractable closest = FindClosestInteractable(hits);

        if (closest != currentTarget)
        {
            currentTarget = closest;
            UpdatePromptUI();
        }
    }

    /// <summary>
    /// Iterates through collider hits and returns the closest valid IInteractable.
    /// </summary>
    private IInteractable FindClosestInteractable(Collider2D[] hits)
    {
        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();

            if (interactable == null || !interactable.IsInteractable)
                continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        return closest;
    }

    /// <summary>
    /// Reads interact input and calls Interact() on the current target if available.
    /// </summary>
    private void HandleInteractInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentTarget != null && currentTarget.IsInteractable)
        {
            currentTarget.Interact(playerController);
        }
    }

    /// <summary>
    /// Shows or hides the prompt UI based on whether there is a valid target.
    /// </summary>
    private void UpdatePromptUI()
    {
        if (promptUI == null) return;

        if (currentTarget != null)
            promptUI.Show(currentTarget.GetPromptText());
        else
            promptUI.Hide();
    }

    // -------------------------------------------------------------------------
    // Editor Visualization
    // -------------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
