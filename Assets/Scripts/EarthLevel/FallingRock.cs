using UnityEngine;
using System.Collections;

/// <summary>
/// FallingRock — fixes:
///   1. Rock and shadow SpriteRenderers are set to the correct sorting layer so they
///      appear above ground tiles (not underneath them).
///   2. Uses the sprite passed from the spawner instead of requiring a pre-assigned one.
/// </summary>
public class FallingRock : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fallSpeed       = 8f;
    [SerializeField] private float damage          = 15f;
    [SerializeField] private float warningDuration = 1.0f;
    [SerializeField] private float destroyY        = -12f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer shadowRenderer;
    [SerializeField] private SpriteRenderer rockRenderer;

    [Header("Rendering — must be above ground tiles")]
    [Tooltip("Match this to the sorting layer your player uses, e.g. 'Characters'")]
    [SerializeField] private string sortingLayerName = "Enemies";  // was "Characters"
    [SerializeField] private int    rockSortOrder    = 10;
    [SerializeField] private int    shadowSortOrder  = 9;

    private Vector2 targetPosition;
    private bool    isFalling    = false;
    private float   warningTimer = 0f;

    // -------------------------------------------------------------------------
    // Public Init (called by FallingRockSpawner)
    // -------------------------------------------------------------------------

    public void Initialize(Vector2 targetPos, Sprite rockSprite, float speed, float dmg)
    {
        targetPosition = targetPos;
        fallSpeed      = speed;
        damage         = dmg;

        // FIX: Set sorting so rocks appear above ground
        if (shadowRenderer != null)
        {
            shadowRenderer.sortingLayerName = sortingLayerName;
            shadowRenderer.sortingOrder     = shadowSortOrder;
            shadowRenderer.transform.position = targetPos;
            shadowRenderer.color = new Color(1f, 0f, 0f, 0.3f);
        }

        // Start rock high above target, hidden until warning ends
        transform.position = new Vector3(targetPos.x, targetPos.y + 12f, 0f);

        if (rockRenderer != null)
        {
            rockRenderer.sortingLayerName = sortingLayerName;
            rockRenderer.sortingOrder     = rockSortOrder;
            if (rockSprite != null)
                rockRenderer.sprite = rockSprite;
            rockRenderer.enabled = false;
        }

        isFalling    = false;
        warningTimer = 0f;
    }

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Update()
    {
        if (!isFalling)
        {
            warningTimer += Time.deltaTime;

            // Pulse the shadow red as warning
            if (shadowRenderer != null)
            {
                float alpha = Mathf.PingPong(warningTimer * 4f, 0.5f) + 0.2f;
                shadowRenderer.color = new Color(1f, 0f, 0f, alpha);
            }

            if (warningTimer >= warningDuration)
            {
                isFalling = true;
                if (rockRenderer  != null) rockRenderer.enabled = true;
                if (shadowRenderer != null) shadowRenderer.gameObject.SetActive(false);
            }
            return;
        }

        // Fall downward
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (transform.position.y <= targetPosition.y)
            OnImpact();

        if (transform.position.y < destroyY)
            Destroy(gameObject);
    }

    // -------------------------------------------------------------------------
    // Impact
    // -------------------------------------------------------------------------

    private void OnImpact()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPosition, 0.8f);
        foreach (Collider2D hit in hits)
        {
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}