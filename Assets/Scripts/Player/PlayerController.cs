using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central MonoBehaviour for the player in a top-down 2D game.
///
/// Receives Input System callbacks via Send Messages behavior on PlayerInput.
/// Stores input values and delegates to MovementSystem, CollisionHandler,
/// and InteractionSystem each frame.
/// </summary>
[RequireComponent(typeof(MovementSystem))]
[RequireComponent(typeof(CollisionHandler))]
[RequireComponent(typeof(InteractionSystem))]
public class PlayerController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Subsystem References
    // -------------------------------------------------------------------------

    private MovementSystem movementSystem;
    private CollisionHandler collisionHandler;
    private InteractionSystem interactionSystem;
    private DashSystem dashSystem;
    private Animator animator;

    // -------------------------------------------------------------------------
    // Input State
    // -------------------------------------------------------------------------

    private Vector2 moveInput;
    private string currentAnimState = "";
    private string characterPrefix = "Blue";

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        movementSystem    = GetComponent<MovementSystem>();
        collisionHandler  = GetComponent<CollisionHandler>();
        interactionSystem = GetComponent<InteractionSystem>();
        dashSystem        = GetComponent<DashSystem>();
        animator          = GetComponent<Animator>();
    }

    private void Start()
    {
        // Re-grab animator after CharacterLoader swaps the controller
        animator = GetComponent<Animator>();

        // Determine character prefix
        int selected = PlayerPrefs.GetInt("SelectedCharacter", 0);
        characterPrefix = selected == 0 ? "Blue" : "Red";

        // Play idle immediately
        PlayAnim(characterPrefix + "Idle");
    }

    private void Update()
    {
        if (GameStateManager.IsUIOpen)
        {
            movementSystem.Stop();
            return;
        }

        // Don't override dash velocity with normal movement
        if (dashSystem != null && dashSystem.IsDashing)
            return;

        movementSystem.Move(moveInput);
        UpdateAnimator();
    }

    // -------------------------------------------------------------------------
    // Input System Callbacks (Send Messages)
    // -------------------------------------------------------------------------

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void OnInteract(InputValue value)
    {
        if (value.isPressed)
            interactionSystem.TriggerInteraction();
    }

    private void OnDash(InputValue value)
    {
        if (value.isPressed && dashSystem != null)
            dashSystem.TryDash();
    }

    // -------------------------------------------------------------------------
    // Animation — uses animator.Play() directly, no transitions needed
    // -------------------------------------------------------------------------

    private void PlayAnim(string stateName)
    {
        if (currentAnimState == stateName) return;
        currentAnimState = stateName;
        animator.Play(stateName, 0, 0f);
    }

    private void UpdateAnimator()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        Vector2 direction = movementSystem.GetDirection();
        bool isMoving = direction.sqrMagnitude > 0.01f;

        if (!isMoving)
        {
            PlayAnim(characterPrefix + "Idle");
            return;
        }

        if (direction.y > 0.1f)
            PlayAnim(characterPrefix + "WalkUp");
        else if (direction.y < -0.1f)
            PlayAnim(characterPrefix + "WalkDown");
        else if (direction.x < -0.1f)
            PlayAnim(characterPrefix + "WalkLeft");
        else if (direction.x > 0.1f)
            PlayAnim(characterPrefix + "WalkRight");
    }
}