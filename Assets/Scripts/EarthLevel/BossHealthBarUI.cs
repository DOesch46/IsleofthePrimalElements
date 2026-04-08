using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EarthBossHealth bossHealth;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private GameObject healthBarPanel;

    [Header("Settings")]
    [SerializeField] private string bossName = "EARTH GUARDIAN";
    [SerializeField] private float smoothSpeed = 5f;

    private float targetFill = 1f;

    private void Start()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged += UpdateHealth;
            bossHealth.OnDied += HideBar;
        }

        if (bossNameText != null)
            bossNameText.text = bossName;

        if (healthFillImage != null)
            healthFillImage.fillAmount = 1f;
    }

    private void Update()
    {
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, targetFill, smoothSpeed * Time.deltaTime);
        }
    }

    private void UpdateHealth(float current, float max)
    {
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

    private void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= UpdateHealth;
            bossHealth.OnDied -= HideBar;
        }
    }
}