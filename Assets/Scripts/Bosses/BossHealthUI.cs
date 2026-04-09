using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public Image fill;

    private EnemyHealth target;

    public void SetTarget(EnemyHealth newTarget)
    {
        if (target != null)
        {
            target.OnHealthChanged -= UpdateHealth;
        }

        target = newTarget;

        if (target != null)
        {
            target.OnHealthChanged += UpdateHealth;
            UpdateHealth(target.GetCurrentHealth(), target.GetMaxHealth());
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