using UnityEngine;

/// <summary>
/// A torch that can be lit by the player's fire ability.
/// Swaps between unlit and lit sprites.
/// 
/// Setup:
/// 1. Create a GameObject with a SpriteRenderer (unlit torch sprite)
/// 2. Add a BoxCollider2D (Is Trigger: ON)
/// 3. Attach this script
/// 4. Drag unlit and lit sprites into Inspector
/// </summary>
public class FireTorch : MonoBehaviour
{
    [Header("Torch Settings")]
    [Tooltip("Is this torch already lit when the level starts?")]
    [SerializeField] private bool startsLit = false;

    [Header("Sprites")]
    [Tooltip("Sprite when torch is NOT lit")]
    [SerializeField] private Sprite unlitSprite;

    [Tooltip("Sprite when torch IS lit")]
    [SerializeField] private Sprite litSprite;

    // Is the torch currently lit?
    private bool isLit = false;

    // Reference to the SpriteRenderer
    private SpriteRenderer spriteRenderer;

    // Reference to the parent puzzle manager
    private TorchPuzzle puzzleManager;

    /// <summary>
    /// Check if this torch is lit.
    /// </summary>
    public bool IsLit => isLit;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Find the puzzle manager in the scene
        puzzleManager = FindFirstObjectByType<TorchPuzzle>();

        if (startsLit)
        {
            LightTorch();
        }
        else
        {
            ExtinguishTorch();
        }
    }

    /// <summary>
    /// Lights the torch.
    /// </summary>
    public void LightTorch()
    {
        if (isLit) return;

        isLit = true;

        // Swap to lit sprite
        if (spriteRenderer != null && litSprite != null)
        {
            spriteRenderer.sprite = litSprite;
        }

        Debug.Log($"Torch lit: {gameObject.name}");

        // Notify the puzzle manager
        if (puzzleManager != null)
        {
            puzzleManager.CheckAllTorches();
        }
    }

    /// <summary>
    /// Extinguishes the torch.
    /// </summary>
    public void ExtinguishTorch()
    {
        isLit = false;

        // Swap to unlit sprite
        if (spriteRenderer != null && unlitSprite != null)
        {
            spriteRenderer.sprite = unlitSprite;
        }
    }

    /// <summary>
    /// Detects when a projectile hits the torch.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it's a player ability projectile
        AbilityProjectile projectile = other.GetComponent<AbilityProjectile>();
        if (projectile != null)
        {
            LightTorch();
            Destroy(other.gameObject);
            return;
        }

        // Also check by tag
        if (other.CompareTag("PlayerProjectile"))
        {
            LightTorch();
            Destroy(other.gameObject);
        }
    }
}