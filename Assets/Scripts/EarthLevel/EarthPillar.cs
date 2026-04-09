using UnityEngine;
using System.Collections;

public class EarthPillar : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float riseDuration = 0.4f;
    [SerializeField] private float stayDuration = 4f;
    [SerializeField] private float crumbleDuration = 0.5f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float damageRadius = 0.6f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer pillarRenderer;
    [SerializeField] private SpriteRenderer warningRenderer;
    [SerializeField] private float pillarHeight = 1.5f;

    [Header("Scale")]
    [SerializeField] private float pillarWidth = 0.5f;
    [SerializeField] private float warningScale = 0.6f;

    private Vector2 targetPosition;
    private bool hasRisen = false;
    private BoxCollider2D pillarCollider;

    public void Initialize(Vector2 pos)
    {
        targetPosition = pos;
        transform.position = new Vector3(pos.x, pos.y, 0f);

        // Set up warning indicator
        if (warningRenderer != null)
        {
            warningRenderer.transform.localPosition = Vector3.zero;
            warningRenderer.transform.localScale = Vector3.one * warningScale;
            warningRenderer.color = new Color(1f, 0.5f, 0f, 0.4f);
            warningRenderer.sortingLayerName = "Enemies";
            warningRenderer.sortingOrder = 8;
            warningRenderer.gameObject.SetActive(true);
        }

        // Set up pillar (hidden at first)
        if (pillarRenderer != null)
        {
            pillarRenderer.transform.localPosition = new Vector3(0f, -pillarHeight, 0f);
            pillarRenderer.transform.localScale = new Vector3(pillarWidth, pillarHeight, 1f);
            pillarRenderer.sortingLayerName = "Enemies";
            pillarRenderer.sortingOrder = 7;
            pillarRenderer.color = new Color(0.45f, 0.35f, 0.25f, 1f); // brown stone
            pillarRenderer.enabled = false;
        }

        // Add collider for blocking movement
        pillarCollider = gameObject.AddComponent<BoxCollider2D>();
        pillarCollider.size = new Vector2(pillarWidth, pillarHeight);
        pillarCollider.offset = new Vector2(0f, pillarHeight * 0.5f);
        pillarCollider.enabled = false;

        StartCoroutine(PillarSequence());
    }

    private IEnumerator PillarSequence()
    {
        // === WARNING PHASE ===
        float warningTime = 0.6f;
        float elapsed = 0f;

        while (elapsed < warningTime)
        {
            elapsed += Time.deltaTime;
            if (warningRenderer != null)
            {
                float pulse = Mathf.PingPong(elapsed * 5f, 1f);
                float alpha = Mathf.Lerp(0.2f, 0.6f, pulse);
                warningRenderer.color = new Color(1f, 0.4f, 0f, alpha);

                float scale = Mathf.Lerp(warningScale * 0.5f, warningScale, elapsed / warningTime);
                warningRenderer.transform.localScale = Vector3.one * scale;
            }
            yield return null;
        }

        // === RISING PHASE ===
        // Hide warning
        if (warningRenderer != null)
            warningRenderer.gameObject.SetActive(false);

        // Show pillar
        if (pillarRenderer != null)
            pillarRenderer.enabled = true;

        // Camera shake
        CameraShake shake = Camera.main?.GetComponent<CameraShake>();
        if (shake != null)
            shake.Shake(0.15f, 0.05f);

        // Damage player if standing on the spot
        DamagePlayer();

        // Animate rising
        elapsed = 0f;
        Vector3 hiddenPos = new Vector3(0f, -pillarHeight, 0f);
        Vector3 shownPos = Vector3.zero;

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / riseDuration);
            // Ease out — fast start, slow end (like bursting from ground)
            float easedT = 1f - (1f - t) * (1f - t);

            if (pillarRenderer != null)
                pillarRenderer.transform.localPosition = Vector3.Lerp(hiddenPos, shownPos, easedT);

            yield return null;
        }

        // Enable collision
        if (pillarCollider != null)
            pillarCollider.enabled = true;

        hasRisen = true;

        // === STAY PHASE ===
        // Slight idle animation
        elapsed = 0f;
        while (elapsed < stayDuration)
        {
            elapsed += Time.deltaTime;

            // Subtle shake/wobble
            if (pillarRenderer != null)
            {
                float wobble = Mathf.Sin(elapsed * 3f) * 0.02f;
                pillarRenderer.transform.localPosition = new Vector3(wobble, 0f, 0f);
            }

            yield return null;
        }

        // === CRUMBLE PHASE ===
        if (pillarCollider != null)
            pillarCollider.enabled = false;

        elapsed = 0f;
        while (elapsed < crumbleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / crumbleDuration);

            if (pillarRenderer != null)
            {
                // Shake more intensely
                float shakeX = Random.Range(-0.05f, 0.05f) * (1f - t);
                pillarRenderer.transform.localPosition = new Vector3(shakeX, 0f, 0f);

                // Fade out
                Color c = pillarRenderer.color;
                c.a = 1f - t;
                pillarRenderer.color = c;

                // Shrink downward
                float scaleY = Mathf.Lerp(pillarHeight, 0f, t);
                pillarRenderer.transform.localScale = new Vector3(pillarWidth, scaleY, 1f);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void DamagePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float dist = Vector2.Distance(targetPosition, (Vector2)player.transform.position);
            if (dist <= damageRadius)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph == null)
                    ph = player.GetComponentInChildren<PlayerHealth>();
                if (ph == null)
                    ph = player.GetComponentInParent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                    Debug.Log($"Pillar hit player for {damage}!");
                    
                    Knockback kb = player.GetComponent<Knockback>();
                    if (kb != null)
                        kb.Apply(targetPosition, 6f);
                }
            }
        }
    }
}