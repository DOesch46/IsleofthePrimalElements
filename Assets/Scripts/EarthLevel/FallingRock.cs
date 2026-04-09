using UnityEngine;
using System.Collections;

public class FallingRock : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fallDuration = 0.8f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float warningDuration = 0.6f;
    [SerializeField] private float impactRadius = 0.4f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer shadowRenderer;
    [SerializeField] private SpriteRenderer rockRenderer;

    [Header("Animation")]
    [SerializeField] private Sprite[] fallingFrames;
    [SerializeField] private Sprite[] impactFrames;
    [SerializeField] private float frameRate = 12f;

    [Header("Scale")]
    [SerializeField] private float rockScale = 0.5f;
    [SerializeField] private float shadowMinScale = 0.05f;
    [SerializeField] private float shadowMaxScale = 0.3f;

    [Header("Fall Height")]
    [SerializeField] private float startHeight = 4f;

    private Vector2 targetPosition;
    private bool isFalling = false;
    private float warningTimer = 0f;
    private float fallTimer = 0f;
    private bool hasImpacted = false;
    private int currentFrame = 0;
    private float frameTimer = 0f;

    public void Initialize(Vector2 targetPos, Sprite rockSprite, float speed, float dmg)
    {
        targetPosition = targetPos;
        damage = dmg;
        transform.position = new Vector3(targetPos.x, targetPos.y, 0f);

        if (shadowRenderer != null)
        {
            shadowRenderer.sortingLayerName = "Enemies";
            shadowRenderer.sortingOrder = 9;
            shadowRenderer.transform.localPosition = Vector3.zero;
            shadowRenderer.color = new Color(0f, 0f, 0f, 0.2f);
            shadowRenderer.transform.localScale = Vector3.one * shadowMinScale;
            shadowRenderer.gameObject.SetActive(true);
        }

        if (rockRenderer != null)
        {
            rockRenderer.sortingLayerName = "Enemies";
            rockRenderer.sortingOrder = 10;
            rockRenderer.transform.localPosition = new Vector3(0f, startHeight, 0f);
            rockRenderer.transform.localScale = Vector3.one * rockScale * 0.3f;

            if (fallingFrames != null && fallingFrames.Length > 0)
                rockRenderer.sprite = fallingFrames[0];
            else if (rockSprite != null)
                rockRenderer.sprite = rockSprite;

            rockRenderer.enabled = false;
        }

        isFalling = false;
        warningTimer = 0f;
        fallTimer = 0f;
        hasImpacted = false;
        currentFrame = 0;
        frameTimer = 0f;
    }

    private void Update()
    {
        if (hasImpacted) return;

        if (!isFalling)
        {
            warningTimer += Time.deltaTime;

            if (shadowRenderer != null)
            {
                float pulse = Mathf.PingPong(warningTimer * 4f, 1f);
                float alpha = Mathf.Lerp(0.1f, 0.45f, pulse);
                shadowRenderer.color = new Color(1f, 0f, 0f, alpha);

                float growT = warningTimer / warningDuration;
                float scale = Mathf.Lerp(shadowMinScale, shadowMaxScale * 0.5f, growT);
                shadowRenderer.transform.localScale = Vector3.one * scale;
            }

            if (warningTimer >= warningDuration)
            {
                isFalling = true;
                if (rockRenderer != null)
                    rockRenderer.enabled = true;
                if (shadowRenderer != null)
                    shadowRenderer.color = new Color(0f, 0f, 0f, 0.2f);
            }
            return;
        }

        fallTimer += Time.deltaTime;
        float t = Mathf.Clamp01(fallTimer / fallDuration);
        float easedT = t * t;

        if (fallingFrames != null && fallingFrames.Length > 0)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= 1f / frameRate)
            {
                frameTimer = 0f;
                currentFrame = (currentFrame + 1) % fallingFrames.Length;
                rockRenderer.sprite = fallingFrames[currentFrame];
            }
        }

        if (rockRenderer != null)
        {
            float currentY = Mathf.Lerp(startHeight, 0f, easedT);
            rockRenderer.transform.localPosition = new Vector3(0f, currentY, 0f);

            float currentScale = Mathf.Lerp(rockScale * 0.3f, rockScale, easedT);
            rockRenderer.transform.localScale = Vector3.one * currentScale;
        }

        if (shadowRenderer != null)
        {
            float shadowScale = Mathf.Lerp(shadowMinScale, shadowMaxScale, easedT);
            shadowRenderer.transform.localScale = Vector3.one * shadowScale;

            float shadowAlpha = Mathf.Lerp(0.1f, 0.4f, easedT);
            shadowRenderer.color = new Color(0f, 0f, 0f, shadowAlpha);
        }

        if (t >= 1f)
            OnImpact();
    }

    private void OnImpact()
    {
        hasImpacted = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float dist = Vector2.Distance(targetPosition, (Vector2)player.transform.position);
            if (dist <= impactRadius)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph == null)
                    ph = player.GetComponentInChildren<PlayerHealth>();
                if (ph == null)
                    ph = player.GetComponentInParent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                    Debug.Log($"Rock hit player for {damage} damage!");
                    
                    Knockback kb = player.GetComponent<Knockback>();
                    if (kb != null)
                        kb.Apply(targetPosition, 5f);
                }
            }
        }

        CameraShake shake = Camera.main?.GetComponent<CameraShake>();
        if (shake != null)
            shake.Shake(0.08f, 0.03f);

        SpawnDustBurst();
        StartCoroutine(PlayImpactAndDestroy());
    }

    private void SpawnDustBurst()
    {
        int dustCount = 6;
        for (int i = 0; i < dustCount; i++)
        {
            StartCoroutine(SpawnSingleDust());
        }
    }

    private IEnumerator SpawnSingleDust()
    {
        GameObject dust = new GameObject("Dust");
        SpriteRenderer sr = dust.AddComponent<SpriteRenderer>();

        int texSize = 8;
        Texture2D tex = new Texture2D(texSize, texSize);
        Color[] colors = new Color[texSize * texSize];
        Vector2 center = new Vector2(texSize / 2f, texSize / 2f);

        for (int x = 0; x < texSize; x++)
        {
            for (int y = 0; y < texSize; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist < texSize / 2f)
                    colors[y * texSize + x] = Color.white;
                else
                    colors[y * texSize + x] = Color.clear;
            }
        }
        tex.SetPixels(colors);
        tex.filterMode = FilterMode.Point;
        tex.Apply();

        sr.sprite = Sprite.Create(tex, new Rect(0, 0, texSize, texSize), Vector2.one * 0.5f, 16f);
        sr.sortingLayerName = "Enemies";
        sr.sortingOrder = 15;
        sr.color = new Color(0.6f, 0.5f, 0.4f, 0.5f);

        Vector2 dir = Random.insideUnitCircle.normalized;
        float speed = Random.Range(1f, 2.5f);
        float size = Random.Range(0.05f, 0.12f);

        dust.transform.position = new Vector3(targetPosition.x, targetPosition.y, 0f);
        dust.transform.localScale = Vector3.one * size;

        float lifetime = 0.4f;
        float elapsed = 0f;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float tt = elapsed / lifetime;

            dust.transform.position += (Vector3)(dir * speed * Time.deltaTime);
            speed *= 0.95f;

            float alpha = Mathf.Lerp(0.5f, 0f, tt);
            sr.color = new Color(0.6f, 0.5f, 0.4f, alpha);

            float scale = Mathf.Lerp(size, size * 0.3f, tt);
            dust.transform.localScale = Vector3.one * scale;

            dust.transform.position += Vector3.up * 0.3f * Time.deltaTime;

            yield return null;
        }

        Destroy(dust);
    }

    private IEnumerator PlayImpactAndDestroy()
    {
        if (impactFrames != null && impactFrames.Length > 0)
        {
            rockRenderer.transform.localPosition = Vector3.zero;
            foreach (Sprite frame in impactFrames)
            {
                rockRenderer.sprite = frame;
                yield return new WaitForSeconds(1f / frameRate);
            }
        }

        float fadeTime = 0.2f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeTime);
            if (rockRenderer != null)
                rockRenderer.color = new Color(1f, 1f, 1f, alpha);
            if (shadowRenderer != null)
                shadowRenderer.color = new Color(0f, 0f, 0f, alpha * 0.3f);
            yield return null;
        }

        Destroy(gameObject);
    }
}