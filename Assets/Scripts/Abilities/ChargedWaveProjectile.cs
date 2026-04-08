using UnityEngine;

public class ChargedWaveProjectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private Vector2 moveDirection = Vector2.right;

    public float lifetime = 3f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(float dmg, float spd, Sprite waveSprite, Vector2 direction)
    {
        damage = dmg;
        speed = spd;
        moveDirection = direction.normalized;

        if (spriteRenderer != null && waveSprite != null)
        {
            spriteRenderer.sprite = waveSprite;
        }

        RotateToDirection();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    void RotateToDirection()
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
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}