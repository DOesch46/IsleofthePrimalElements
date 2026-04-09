using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Image fill;
    public PlayerHealth player;

    private void Awake()
    {
        // ✅ Auto-find player if not assigned
        if (player == null)
        {
            player = FindObjectOfType<PlayerHealth>();
        }
    }

    private void OnEnable()
    {
        if (player != null)
        {
            player.OnHealthChanged += UpdateHealth;
        }
        else
        {
            Debug.LogWarning("HealthUI: PlayerHealth not assigned!");
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealth;
        }
    }

    private void Start()
    {
        if (player != null && fill != null)
        {
            UpdateHealth(player.GetCurrentHealth(), player.GetMaxHealth());
        }
    }

    private void UpdateHealth(float current, float max)
    {
        if (fill != null && max > 0)
        {
            fill.fillAmount = current / max;
        }
    }
}