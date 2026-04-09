using UnityEngine;

/// <summary>
/// Attach this to a boss with EnemyHealth.
/// When the boss dies, a portal is spawned that can return the player
/// to the main world at a configured SpawnPoint ID.
/// </summary>
[DisallowMultipleComponent]
public class BossPortalSpawner : MonoBehaviour
{
    [Header("Portal Spawn")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private Transform portalSpawnPoint;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    [Header("Portal Destination")]
    [SerializeField] private string returnSceneName = "MainIsland";
    [SerializeField] private string returnSpawnId = "HubReturn";

    [Header("Optional Progress")]
    [SerializeField] private LevelData completedLevel;

    private bool portalSpawned;

    private void OnEnable()
    {
        EnemyHealth.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        EnemyHealth.OnEnemyDied -= HandleEnemyDied;
    }

    private void HandleEnemyDied(GameObject deadEnemy)
    {
        if (portalSpawned || deadEnemy != gameObject)
            return;

        SpawnPortal();
    }

    private void SpawnPortal()
    {
        if (portalPrefab == null)
        {
            Debug.LogWarning($"BossPortalSpawner on '{name}' has no portal prefab assigned.");
            return;
        }

        Vector3 spawnPosition = portalSpawnPoint != null
            ? portalSpawnPoint.position
            : transform.position + spawnOffset;

        GameObject portalInstance = Instantiate(portalPrefab, spawnPosition, Quaternion.identity);
        portalSpawned = true;

        PortalSceneLoader portalLoader = portalInstance.GetComponent<PortalSceneLoader>();
        if (portalLoader != null)
        {
            portalLoader.Configure(returnSceneName, returnSpawnId, completedLevel);
        }
        else
        {
            Debug.LogWarning(
                $"BossPortalSpawner spawned '{portalInstance.name}', but it has no PortalSceneLoader component.");
        }

        Debug.Log(
            $"BossPortalSpawner spawned a portal at {spawnPosition} for scene '{returnSceneName}' and SpawnId '{returnSpawnId}'.");
    }
}
