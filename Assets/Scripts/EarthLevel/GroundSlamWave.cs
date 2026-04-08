using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroundSlamWave : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float travelSpeed = 6f;
    [SerializeField] private float maxDistance = 12f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float segmentSpacing = 0.8f;
    [SerializeField] private float segmentDelay = 0.1f;
    [SerializeField] private float segmentLifetime = 0.6f;

    [Header("Visual")]
    [SerializeField] private Sprite waveSegmentSprite;
    [SerializeField] private Color waveColor = new Color(0.6f, 0.4f, 0.2f, 1f);

    private Vector2 direction;
    private Vector2 origin;
    private HashSet<int> hitPlayers = new HashSet<int>();

    public void Initialize(Vector2 startPos, Vector2 dir, float speed, float dmg, Sprite sprite)
    {
        origin = startPos;
        direction = dir.normalized;
        travelSpeed = speed;
        damage = dmg;
        if (sprite != null) waveSegmentSprite = sprite;

        transform.position = startPos;
        StartCoroutine(PropagateWave());
    }

    private IEnumerator PropagateWave()
    {
        float distanceTraveled = 0f;
        int segmentIndex = 0;

        while (distanceTraveled < maxDistance)
        {
            Vector2 segmentPos = origin + direction * distanceTraveled;

            SpawnSegment(segmentPos, segmentIndex);

            Collider2D[] hits = Physics2D.OverlapCircleAll(segmentPos, 0.5f);
            foreach (Collider2D hit in hits)
            {
                int id = hit.gameObject.GetInstanceID();
                if (hitPlayers.Contains(id)) continue;

                PlayerHealth health = hit.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                    hitPlayers.Add(id);
                }
            }

            distanceTraveled += segmentSpacing;
            segmentIndex++;

            yield return new WaitForSeconds(segmentDelay);
        }

        yield return new WaitForSeconds(segmentLifetime);
        Destroy(gameObject);
    }

    private void SpawnSegment(Vector2 position, int index)
    {
        GameObject segment = new GameObject($"WaveSegment_{index}");
        segment.transform.position = position;
        segment.transform.parent = transform;

        SpriteRenderer sr = segment.AddComponent<SpriteRenderer>();
        sr.sprite = waveSegmentSprite;
        sr.color = waveColor;
        sr.sortingLayerName = "Decor";
        sr.sortingOrder = 5;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        segment.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        StartCoroutine(AnimateSegment(segment, sr));
    }

    private IEnumerator AnimateSegment(GameObject segment, SpriteRenderer sr)
    {
        if (segment == null) yield break;

        float popTime = 0.1f;
        float elapsed = 0f;
        Vector3 startScale = new Vector3(0.3f, 0.1f, 1f);
        Vector3 fullScale = new Vector3(0.8f, 0.8f, 1f);

        segment.transform.localScale = startScale;

        while (elapsed < popTime)
        {
            if (segment == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / popTime;
            segment.transform.localScale = Vector3.Lerp(startScale, fullScale, t);
            yield return null;
        }

        yield return new WaitForSeconds(segmentLifetime - 0.2f);

        elapsed = 0f;
        float fadeTime = 0.1f;
        Color startColor = sr.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsed < fadeTime)
        {
            if (segment == null || sr == null) yield break;
            elapsed += Time.deltaTime;
            sr.color = Color.Lerp(startColor, endColor, elapsed / fadeTime);
            yield return null;
        }

        if (segment != null)
            Destroy(segment);
    }
}