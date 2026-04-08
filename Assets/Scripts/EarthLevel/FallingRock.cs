using UnityEngine;
using System.Collections;

public class FallingRock : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fallSpeed = 8f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float warningDuration = 1.0f;
    [SerializeField] private float destroyY = -12f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer shadowRenderer;
    [SerializeField] private SpriteRenderer rockRenderer;

    private Vector2 targetPosition;
    private bool isFalling = false;
    private float warningTimer = 0f;

    public void Initialize(Vector2 targetPos, Sprite rockSprite, float speed, float dmg)
    {
        targetPosition = targetPos;
        fallSpeed = speed;
        damage = dmg;

        if (shadowRenderer != null)
        {
            shadowRenderer.transform.position = targetPos;
            shadowRenderer.color = new Color(1f, 0f, 0f, 0.3f);
        }

        transform.position = new Vector3(targetPos.x, targetPos.y + 12f, 0f);

        if (rockRenderer != null)
        {
            rockRenderer.sprite = rockSprite;
            rockRenderer.enabled = false;
        }

        isFalling = false;
        warningTimer = 0f;
    }

    private void Update()
    {
        if (!isFalling)
        {
            warningTimer += Time.deltaTime;

            if (shadowRenderer != null)
            {
                float alpha = Mathf.PingPong(warningTimer * 4f, 0.5f) + 0.2f;
                shadowRenderer.color = new Color(1f, 0f, 0f, alpha);
            }

            if (warningTimer >= warningDuration)
            {
                isFalling = true;
                if (rockRenderer != null)
                    rockRenderer.enabled = true;
                if (shadowRenderer != null)
                    shadowRenderer.gameObject.SetActive(false);
            }
            return;
        }

        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (transform.position.y <= targetPosition.y)
        {
            OnImpact();
        }

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    private void OnImpact()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPosition, 0.8f);
        foreach (Collider2D hit in hits)
        {
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}