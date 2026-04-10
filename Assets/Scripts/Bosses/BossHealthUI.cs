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
            target.OnDied -= HandleTargetDied;
        }

        target = newTarget;

        gameObject.SetActive(target != null);

        if (target != null)
        {
            target.OnHealthChanged += UpdateHealth;
            target.OnDied += HandleTargetDied;
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

    private void HandleTargetDied()
    {
        if (target != null)
        {
            target.OnHealthChanged -= UpdateHealth;
            target.OnDied -= HandleTargetDied;
            target = null;
        }

        gameObject.SetActive(false);
    }
}
