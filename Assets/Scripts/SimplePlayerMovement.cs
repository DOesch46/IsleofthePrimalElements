using UnityEngine;

/// <summary>
/// Temporary simple player movement for testing.
/// Use WASD or Arrow Keys to move.
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
        // Get the Rigidbody2D component attached to this GameObject
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError("SimplePlayerMovement requires a Rigidbody2D component!");
        }
    }

    private void Update()
    {
        // Read input every frame
        // GetAxisRaw gives -1, 0, or 1 (no smoothing)
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        moveInput = new Vector2(horizontalInput, verticalInput);
        
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