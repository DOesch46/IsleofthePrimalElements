using UnityEngine;

/// <summary>
/// Displays a simple health bar above the enemy using screen-space GUI.
/// Automatically listens to EnemyHealth for damage updates.
///
/// SETUP:
/// 1. Attach to the enemy GameObject (alongside EnemyHealth).
/// 2. No manual setup needed — the bar appears after the enemy takes damage.
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Health Bar Settings")]
    [SerializeField] private float barWidth = 60f;
    [SerializeField] private float barHeight = 8f;
    [SerializeField] private float yOffset = 1f;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private EnemyHealth enemyHealth;
    private float healthFraction = 1f;
    private bool hasBeenHit = false;
    private Camera mainCamera;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
            enemyHealth.OnHealthChanged += UpdateHealth;
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
            enemyHealth.OnHealthChanged -= UpdateHealth;
    }

    private void UpdateHealth(float current, float max)
    {
        hasBeenHit = true;
        healthFraction = Mathf.Clamp01(current / max);
    }

    // -------------------------------------------------------------------------
    // GUI Drawing
    // -------------------------------------------------------------------------

    private void OnGUI()
    {
        if (!hasBeenHit) return;
        if (mainCamera == null) return;

        Vector3 worldPos = transform.position + new Vector3(0f, yOffset, 0f);
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        // Don't draw if behind camera
        if (screenPos.z < 0f) return;

        // Unity GUI has Y flipped (0 = top)
        float x = screenPos.x - barWidth / 2f;
        float y = Screen.height - screenPos.y;

        // Background (dark)
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(x - 1, y - 1, barWidth + 2, barHeight + 2), Texture2D.whiteTexture);

        // Red background (shows missing health)
        GUI.color = Color.red;
        GUI.DrawTexture(new Rect(x, y, barWidth, barHeight), Texture2D.whiteTexture);

        // Green fill (current health)
        GUI.color = healthFraction > 0.5f ? Color.green : Color.yellow;
        GUI.DrawTexture(new Rect(x, y, barWidth * healthFraction, barHeight), Texture2D.whiteTexture);

        // Reset color
        GUI.color = Color.white;
    }
}
