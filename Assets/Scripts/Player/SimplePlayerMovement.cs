using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple player movement for testing using the New Input System.
/// Use WASD or Arrow Keys to move.
/// Attach to Player with a Rigidbody2D.
/// </summary>
public class SimplePlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How fast the player moves")]
    [SerializeField] private float moveSpeed = 5f;

    // Reference to the Rigidbody2D component
    private Rigidbody2D rb;
    
    // Stores the movement input each frame
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError("SimplePlayerMovement requires a Rigidbody2D component!");
        }
    }

    private void Update()
    {
        // Read input using the New Input System
        var keyboard = Keyboard.current;
        
        if (keyboard == null)
        {
            return;
        }

        // Build the movement vector from key presses
        Vector2 input = Vector2.zero;
        
        // Horizontal movement (A/D or Left/Right arrows)
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            input.x = -1f;
        }
        else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            input.x = 1f;
        }
        
        // Vertical movement (W/S or Up/Down arrows)
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            input.y = 1f;
        }
        else if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            input.y = -1f;
        }

        moveInput = input;
        
        // Normalize so diagonal movement isn't faster
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }
    }

    private void FixedUpdate()
    {
        // Apply movement in FixedUpdate for physics consistency
        rb.linearVelocity = moveInput * moveSpeed;
    }
}