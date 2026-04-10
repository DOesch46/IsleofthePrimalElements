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

    [Header("Optional Reward Pickup")]
    [SerializeField] private Transform rewardSpawnPoint;
    [SerializeField] private Vector3 rewardSpawnOffset = Vector3.zero;

    [Header("Portal Destination")]
    [SerializeField] private string returnSceneName = "MainIsland";
    [SerializeField] private string returnSpawnId = "HubReturn";

    [Header("Boss Progress")]
    [SerializeField] private string bossProgressId = string.Empty;

    [Header("Optional Progress")]
    [SerializeField] private LevelData completedLevel;

    private bool portalSpawned;
    private bool rewardSpawned;

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

        string resolvedBossProgressId = ResolveBossProgressId();
        if (GameProgressManager.Instance != null && !string.IsNullOrWhiteSpace(resolvedBossProgressId))
        {
            GameProgressManager.Instance.MarkBossDefeated(resolvedBossProgressId);
            Debug.Log($"{name}: Boss defeat recorded with id '{resolvedBossProgressId}'.");
        }

        SpawnPortal();
    }

    private void SpawnPortal()
    {
        if (portalSpawned)
        {
            Debug.Log($"{name}: Portal spawn skipped because it already exists in this scene.");
            return;
        }

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

    public void EnsurePortalExistsForCompletedBoss()
    {
        if (portalSpawned)
        {
            Debug.Log($"{name}: Completed-boss portal already exists.");
            return;
        }

        Debug.Log($"{name}: Boss is already defeated. Ensuring return portal exists.");
        SpawnPortal();
    }

    public void EnsureCompletedBossRewardsExist()
    {
        if (rewardSpawned)
        {
            Debug.Log($"{name}: Completed-boss reward pickup already exists in this scene.");
            return;
        }

        EnemyHealth bossHealth = GetComponent<EnemyHealth>();
        if (bossHealth == null)
        {
            Debug.LogWarning($"{name}: Cannot restore boss reward pickup because EnemyHealth is missing.");
            return;
        }

        GameObject rewardPrefab = bossHealth.RewardPickupPrefab;
        if (rewardPrefab == null)
        {
            Debug.Log($"{name}: Boss has no reward pickup prefab to restore.");
            return;
        }

        ElementType rewardElement = bossHealth.RewardElement;
        if (rewardElement != ElementType.None &&
            GameProgressManager.Instance != null &&
            GameProgressManager.Instance.HasElement(rewardElement))
        {
            Debug.Log($"{name}: Boss reward pickup restore skipped because element '{rewardElement}' is already unlocked.");
            return;
        }

        if (rewardPrefab.GetComponent<TridentPickup>() != null &&
            FindFirstObjectByType<TridentPickup>() != null)
        {
            rewardSpawned = true;
            Debug.Log($"{name}: A trident pickup is already present in the scene.");
            return;
        }

        Vector3 spawnPosition = rewardSpawnPoint != null
            ? rewardSpawnPoint.position
            : transform.position + rewardSpawnOffset;

        Instantiate(rewardPrefab, spawnPosition, Quaternion.identity);
        rewardSpawned = true;

        Debug.Log(
            $"{name}: Restored boss reward pickup '{rewardPrefab.name}' at {spawnPosition} because the boss is already defeated but the reward is still needed.");
    }

    private string ResolveBossProgressId()
    {
        if (!string.IsNullOrWhiteSpace(bossProgressId))
            return bossProgressId;

        if (completedLevel != null && !string.IsNullOrWhiteSpace(completedLevel.sceneName))
            return completedLevel.sceneName + "_Boss";

        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_Boss";
    }
}
