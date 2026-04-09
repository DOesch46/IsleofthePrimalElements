using UnityEngine;

public class ChargedWaveProjectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private ElementType element = ElementType.Water;
    private Vector2 moveDirection = Vector2.right;

    public float lifetime = 3f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(float dmg, float spd, Sprite waveSprite, Vector2 direction, ElementType attackElement = ElementType.Water)
    {
        damage = dmg;
        speed = spd;
        element = attackElement;

        if (direction != Vector2.zero)
            moveDirection = direction.normalized;

        if (spriteRenderer != null && waveSprite != null)
        {
            spriteRenderer.sprite = waveSprite;
        }

        if (spriteRenderer != null && spriteRenderer.sprite == null)
        {
            Debug.LogWarning("ChargedWaveProjectile has no sprite assigned. Set the wave sprites on PlayerCombat or assign a default sprite on the Wave prefab.");
        }

        RotateToDirection();
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    private void RotateToDirection()
    {
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(CalculateDamage(other.gameObject));
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
            Destroy(gameObject);
    }

    private float CalculateDamage(GameObject target)
    {
        ElementalEntity entity = target.GetComponent<ElementalEntity>();
        if (entity == null)
            return damage;

        return damage * GetDamageMultiplier(entity.Element);
    }

    private float GetDamageMultiplier(ElementType targetElement)
    {
        switch (element)
        {
            case ElementType.Fire:
                if (targetElement == ElementType.Earth) return 2f;
                if (targetElement == ElementType.Water) return 0.5f;
                break;
            case ElementType.Water:
                if (targetElement == ElementType.Fire) return 2f;
                if (targetElement == ElementType.Lightning) return 0.5f;
                break;
            case ElementType.Earth:
                if (targetElement == ElementType.Lightning) return 2f;
                if (targetElement == ElementType.Fire) return 0.5f;
                break;
            case ElementType.Lightning:
                if (targetElement == ElementType.Water) return 2f;
                if (targetElement == ElementType.Earth) return 0.5f;
                break;
        }

        return 1f;
    }
}
