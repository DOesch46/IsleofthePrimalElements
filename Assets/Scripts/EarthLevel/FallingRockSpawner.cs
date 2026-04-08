using UnityEngine;
using System.Collections;

public class FallingRockSpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private GameObject fallingRockPrefab;
    [SerializeField] private Sprite[] rockSprites;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnIntervalVariance = 0.5f;
    [SerializeField] private int maxConcurrentRocks = 3;

    [Header("Spawn Area — Grass Only")]
    [SerializeField] private Vector2 areaMin = new Vector2(-6f, -3f);
    [SerializeField] private Vector2 areaMax = new Vector2(6f, 3f);

    [Header("Rock Settings")]
    [SerializeField] private float rockFallSpeed = 8f;
    [SerializeField] private float rockDamage = 15f;

    [Header("Targeting")]
    [SerializeField] private float playerTargetChance = 0.4f;
    [SerializeField] private float playerTargetOffset = 1.5f;

    [Header("Difficulty")]
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float difficultyRampTime = 60f;

    private Transform playerTransform;
    private bool isSpawning = false;
    private float elapsedTime = 0f;
    private int currentRockCount = 0;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            elapsedTime = 0f;
            StartCoroutine(SpawnLoop());
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    public void SetDifficulty(float speedMultiplier, float intervalMultiplier)
    {
        rockFallSpeed *= speedMultiplier;
        spawnInterval *= intervalMultiplier;
        spawnInterval = Mathf.Max(spawnInterval, minSpawnInterval);
    }

    private IEnumerator SpawnLoop()
    {
        while (isSpawning)
        {
            elapsedTime += spawnInterval;
            float difficultyT = Mathf.Clamp01(elapsedTime / difficultyRampTime);
            float currentInterval = Mathf.Lerp(spawnInterval, minSpawnInterval, difficultyT);

            float variance = Random.Range(-spawnIntervalVariance, spawnIntervalVariance);
            yield return new WaitForSeconds(currentInterval + variance);

            if (currentRockCount < maxConcurrentRocks)
            {
                SpawnRock();
            }
        }
    }

    private void SpawnRock()
    {
        Vector2 targetPos;

        if (playerTransform != null && Random.value < playerTargetChance)
        {
            Vector2 offset = Random.insideUnitCircle * playerTargetOffset;
            targetPos = (Vector2)playerTransform.position + offset;
        }
        else
        {
            targetPos = new Vector2(
                Random.Range(areaMin.x, areaMax.x),
                Random.Range(areaMin.y, areaMax.y)
            );
        }

        targetPos.x = Mathf.Clamp(targetPos.x, areaMin.x, areaMax.x);
        targetPos.y = Mathf.Clamp(targetPos.y, areaMin.y, areaMax.y);

        GameObject rockObj = Instantiate(fallingRockPrefab, transform);
        FallingRock rock = rockObj.GetComponent<FallingRock>();

        Sprite sprite = rockSprites.Length > 0
            ? rockSprites[Random.Range(0, rockSprites.Length)]
            : null;

        rock.Initialize(targetPos, sprite, rockFallSpeed, rockDamage);

        currentRockCount++;
        StartCoroutine(TrackRockLifetime(rockObj));
    }

    private IEnumerator TrackRockLifetime(GameObject rockObj)
    {
        while (rockObj != null)
            yield return null;
        currentRockCount--;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Vector3 center = new Vector3((areaMin.x + areaMax.x) / 2f, (areaMin.y + areaMax.y) / 2f, 0f);
        Vector3 size = new Vector3(areaMax.x - areaMin.x, areaMax.y - areaMin.y, 0f);
        Gizmos.DrawCube(center, size);
    }
}