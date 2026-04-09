using UnityEngine;
using System.Collections;

public class EarthPillarSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject pillarPrefab;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 areaMin = new Vector2(-2f, -5f);
    [SerializeField] private Vector2 areaMax = new Vector2(8f, 1f);

    [Header("Pattern Settings")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float damage = 15f;

    private bool isActive = false;
    private float spawnTimer = 0f;
    private Transform playerTransform;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    public void StartSpawning()
    {
        isActive = true;
        spawnTimer = 3f; // delay before first spawn
    }

    public void StopSpawning()
    {
        isActive = false;
    }

    private void Update()
    {
        if (!isActive || playerTransform == null) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;

            // Pick a random pattern
            int pattern = Random.Range(0, 4);
            switch (pattern)
            {
                case 0: SpawnRing(); break;
                case 1: SpawnLine(); break;
                case 2: SpawnAroundPlayer(); break;
                case 3: SpawnRandom(); break;
            }
        }
    }

    // Ring of pillars around the player
    private void SpawnRing()
    {
        Debug.Log("Pillar pattern: RING");
        Vector2 center = (Vector2)playerTransform.position;
        float radius = 1.8f;
        int count = 6;

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i * Mathf.Deg2Rad;
            Vector2 pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            pos = ClampToArea(pos);
            SpawnPillar(pos);
        }
    }

    // Line of pillars across the arena
    private void SpawnLine()
    {
        Debug.Log("Pillar pattern: LINE");
        bool horizontal = Random.value > 0.5f;
        int count = 5;

        float playerX = playerTransform.position.x;
        float playerY = playerTransform.position.y;

        for (int i = 0; i < count; i++)
        {
            Vector2 pos;
            if (horizontal)
            {
                float x = Mathf.Lerp(areaMin.x, areaMax.x, (float)i / (count - 1));
                pos = new Vector2(x, playerY);
            }
            else
            {
                float y = Mathf.Lerp(areaMin.y, areaMax.y, (float)i / (count - 1));
                pos = new Vector2(playerX, y);
            }
            pos = ClampToArea(pos);
            SpawnPillar(pos);
        }
    }

    // Pillars close to player — forces dodging
    private void SpawnAroundPlayer()
    {
        Debug.Log("Pillar pattern: AROUND PLAYER");
        Vector2 center = (Vector2)playerTransform.position;
        int count = Random.Range(3, 5);

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 2f;
            Vector2 pos = center + offset;
            pos = ClampToArea(pos);
            SpawnPillar(pos);
        }
    }

    // Random pillars across arena
    private void SpawnRandom()
    {
        Debug.Log("Pillar pattern: RANDOM");
        int count = Random.Range(3, 6);

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = new Vector2(
                Random.Range(areaMin.x, areaMax.x),
                Random.Range(areaMin.y, areaMax.y)
            );
            SpawnPillar(pos);
        }
    }

    private void SpawnPillar(Vector2 pos)
    {
        if (pillarPrefab == null) return;

        GameObject pillar = Instantiate(pillarPrefab, Vector3.zero, Quaternion.identity);
        EarthPillar ep = pillar.GetComponent<EarthPillar>();
        if (ep != null)
            ep.Initialize(pos);
    }

    private Vector2 ClampToArea(Vector2 pos)
    {
        pos.x = Mathf.Clamp(pos.x, areaMin.x, areaMax.x);
        pos.y = Mathf.Clamp(pos.y, areaMin.y, areaMax.y);
        return pos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.3f, 0f, 0.3f);
        Vector3 center = new Vector3(
            (areaMin.x + areaMax.x) / 2f,
            (areaMin.y + areaMax.y) / 2f, 0f);
        Vector3 size = new Vector3(
            areaMax.x - areaMin.x,
            areaMax.y - areaMin.y, 0f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = new Color(0.8f, 0.4f, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}