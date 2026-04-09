using UnityEngine;

public class FallingRockSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject fallingRockPrefab;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 areaMin = new Vector2(-2f, -5f);
    [SerializeField] private Vector2 areaMax = new Vector2(8f, 1f);

    [Header("Rain Settings")]
    [SerializeField] private float minSpawnInterval = 0.08f;
    [SerializeField] private float maxSpawnInterval = 0.25f;
    [SerializeField] private int   rocksPerWave     = 3;
    [SerializeField] private float damage           = 8f;

    [Header("Rock Sprites")]
    [SerializeField] private Sprite[] fallingFrames;
    [SerializeField] private Sprite[] impactFrames;

    private float spawnTimer = 0f;
    private float nextSpawnTime;
    private bool  isActive = false;

    private void Start()
    {
        nextSpawnTime = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    public void StartRain()
    {
        isActive = true;
    }

    public void StopRain()
    {
        isActive = false;
    }

    // Keep old method name so EarthBoss still works
    public void StartSpawning()
    {
        StartRain();
    }

    public void StopSpawning()
    {
        StopRain();
    }

    private void Update()
    {
        if (!isActive) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= nextSpawnTime)
        {
            spawnTimer = 0f;
            nextSpawnTime = Random.Range(minSpawnInterval, maxSpawnInterval);

            // Spawn multiple rocks at once for rain effect
            int count = Random.Range(1, rocksPerWave + 1);
            for (int i = 0; i < count; i++)
            {
                SpawnRock();
            }
        }
    }

    private void SpawnRock()
    {
        Vector2 targetPos = new Vector2(
            Random.Range(areaMin.x, areaMax.x),
            Random.Range(areaMin.y, areaMax.y)
        );

        GameObject rock = Instantiate(fallingRockPrefab, Vector3.zero, Quaternion.identity);
        FallingRock fr = rock.GetComponent<FallingRock>();

        if (fr != null)
        {
            // Pass the first falling frame as the rock sprite
            Sprite rockSprite = (fallingFrames != null && fallingFrames.Length > 0)
                ? fallingFrames[0] : null;

            fr.Initialize(targetPos, rockSprite, 0f, damage);

            // Set animation frames via serialized fields won't work at runtime,
            // so we set them manually
            SetFrames(fr);
        }
    }

    private void SetFrames(FallingRock fr)
    {
        // Use reflection or make fields public — easier: we'll set them on the prefab
        // The frames should be set on the PREFAB in the Inspector
    }
    
    public void SetDifficulty(float spawnRate, float dmg)
    {
        minSpawnInterval = Mathf.Max(0.05f, 1f / spawnRate - 0.1f);
        maxSpawnInterval = Mathf.Max(0.1f, 1f / spawnRate + 0.1f);
        damage = dmg;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Vector3 center = new Vector3(
            (areaMin.x + areaMax.x) / 2f,
            (areaMin.y + areaMax.y) / 2f,
            0f
        );
        Vector3 size = new Vector3(
            areaMax.x - areaMin.x,
            areaMax.y - areaMin.y,
            0f
        );
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }
}