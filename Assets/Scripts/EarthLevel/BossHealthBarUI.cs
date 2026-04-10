using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyHealth bossHealth;  // ✅ Changed from EarthBossHealth
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private GameObject healthBarPanel;

    [Header("Settings")]
    [SerializeField] private string bossName = "EARTH GUARDIAN";
    [SerializeField] private float smoothSpeed = 5f;

    private float targetFill = 1f;
    private bool subscribed = false;

    // -------------------------------------------------------------------------
    // Called when this GameObject is enabled
    // -------------------------------------------------------------------------

    private void OnEnable()
    {
        TrySubscribe();

        if (bossNameText != null)
            bossNameText.text = bossName;

        targetFill = 1f;
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = 1f;
            healthFillImage.color = Color.green;
        }

        if (healthBarPanel != null)
            healthBarPanel.SetActive(true);
    }

    private void TrySubscribe()
    {
        if (subscribed) return;

        // Try to find bossHealth if not assigned
        if (bossHealth == null)
            bossHealth = FindFirstObjectByType<EnemyHealth>();  // ✅ Changed

        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged += UpdateHealth;
            bossHealth.OnDied += HideBar;  // ✅ Uses the new OnDied event
            subscribed = true;

            // Sync immediately to current health
            UpdateHealth(bossHealth.GetCurrentHealth(), bossHealth.GetMaxHealth());  // ✅ Fixed
        }
        else
        {
            Debug.LogWarning("BossHealthBarUI: Could not find EnemyHealth. Assign it in the Inspector.");
        }
    }

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Update()
    {
        if (healthFillImage != null)
            healthFillImage.fillAmount = Mathf.Lerp(
                healthFillImage.fillAmount, 
                targetFill, 
                smoothSpeed * Time.deltaTime
            );
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
            bossHealth.OnDied -= HideBar;
        }
    }
}