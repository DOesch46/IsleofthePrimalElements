using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Image fill;
    public PlayerHealth player;

    private void OnEnable()
    {
        player.OnHealthChanged += UpdateHealth;
    }

    private void OnDisable()
    {
        player.OnHealthChanged -= UpdateHealth;
    }

    private void Start()
    {
        UpdateHealth(player.GetCurrentHealth(), player.GetMaxHealth());
    }

    private void UpdateHealth(float current, float max)
    {
        fill.fillAmount = current / max;
    }
}