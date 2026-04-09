using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Boss Health Bar UI — fixes:
///   1. The boss is disabled at scene start, so this script waits until the
///      boss is actually alive before subscribing to its events.
///   2. Manually calls UpdateHealth once on enable so the bar starts full.
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EarthBossHealth bossHealth;
    [SerializeField] private Image           healthFillImage;
    [SerializeField] private TMP_Text        bossNameText;
    [SerializeField] private GameObject      healthBarPanel;

    [Header("Settings")]
    [SerializeField] private string bossName    = "EARTH GUARDIAN";
    [SerializeField] private float  smoothSpeed = 5f;

    private float targetFill = 1f;
    private bool  subscribed = false;

    // -------------------------------------------------------------------------
    // Called by ArenaRevealTrigger when it enables this GameObject
    // -------------------------------------------------------------------------

    private void OnEnable()
    {
        // Subscribe now — boss is guaranteed to be active at this point
        TrySubscribe();

        if (bossNameText != null)
            bossNameText.text = bossName;

        // Reset bar to full
        targetFill = 1f;
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = 1f;
            healthFillImage.color      = Color.green;
        }

        if (healthBarPanel != null)
            healthBarPanel.SetActive(true);
    }

    private void TrySubscribe()
    {
        if (subscribed) return;

        // Try to find bossHealth if not assigned
        if (bossHealth == null)
            bossHealth = FindFirstObjectByType<EarthBossHealth>();

        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged += UpdateHealth;
            bossHealth.OnDied          += HideBar;
            subscribed = true;

            // Sync immediately to current health
            UpdateHealth(bossHealth.HealthFraction * 300f, 300f); // approximate
        }
        else
        {
            Debug.LogWarning("BossHealthBarUI: Could not find EarthBossHealth. Assign it in the Inspector.");
        }
    }

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Update()
    {
        if (healthFillImage != null)
            healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, targetFill, smoothSpeed * Time.deltaTime);
    }

    // -------------------------------------------------------------------------
    // Event Handlers
    // -------------------------------------------------------------------------

    private void UpdateHealth(float current, float max)
    {
        if (max <= 0f) return;
        targetFill = current / max;

        if (healthFillImage != null)
        {
            if (targetFill > 0.5f)
                healthFillImage.color = Color.Lerp(Color.yellow, Color.green, (targetFill - 0.5f) * 2f);
            else
                healthFillImage.color = Color.Lerp(Color.red, Color.yellow, targetFill * 2f);
        }
    }

    private void HideBar()
    {
        if (healthBarPanel != null)
            healthBarPanel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Cleanup
    // -------------------------------------------------------------------------

    private void OnDestroy()
    {
        if (bossHealth != null && subscribed)
        {
            bossHealth.OnHealthChanged -= UpdateHealth;
            bossHealth.OnDied          -= HideBar;
        }
    }
}