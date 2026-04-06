using UnityEngine;

/// <summary>
/// Attached to area effect GameObjects spawned by abilities.
/// Damages enemies within the radius and then cleans itself up.
/// 
/// Your team can create custom area effect prefabs with this script.
/// </summary>
public class AbilityAreaEffect : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private string abilityName;
    [SerializeField] private int damage;
    [SerializeField] private ElementType element;
    [SerializeField] private float radius;
    [SerializeField] private float duration;

    private AbilityData abilityData;

    /// <summary>
    /// Called by AbilityManager when spawning this effect.
    /// </summary>
    public void Initialize(AbilityData ability)
    {
        abilityData = ability;
        abilityName = ability.abilityName;
        damage = ability.damage;
        element = ability.element;
        radius = ability.areaRadius;
        duration = ability.duration > 0 ? ability.duration : 0.5f;

        // Deal damage to everything in radius
        DealAreaDamage();

        // Destroy after duration
        Destroy(gameObject, duration);
    }

    /// <summary>
    /// Finds all enemies in radius and damages them.
    /// </summary>
    private void DealAreaDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float multiplier = 1f;
                if (abilityData != null)
                {
                    ElementalEntity entity = hit.GetComponent<ElementalEntity>();
                    if (entity != null)
                    {
                        multiplier = abilityData.GetDamageMultiplier(entity.Element);
                    }
                }

                int finalDamage = Mathf.RoundToInt(damage * multiplier);
                Debug.Log($"{abilityName} area hit {hit.gameObject.name} for {finalDamage}!");

                hit.gameObject.SendMessage("TakeDamage", finalDamage, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    /// <summary>
    /// Shows the area radius in Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}