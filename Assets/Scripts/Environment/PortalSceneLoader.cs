using System.Collections;
using UnityEngine;

/// <summary>
/// Trigger portal that loads a target scene and tells SpawnManager which
/// SpawnPoint should be used in that destination scene.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PortalSceneLoader : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private string targetSceneName = "MainIsland";
    [SerializeField] private string targetSpawnId = "HubReturn";

    [Header("Optional Progress")]
    [SerializeField] private LevelData completedLevel;

    [Header("Trigger Behaviour")]
    [SerializeField] private float loadDelay = 0f;
    [SerializeField] private bool disableAfterUse = true;

    private bool isLoading;

    private void Reset()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    private void Awake()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null && !triggerCollider.isTrigger)
            triggerCollider.isTrigger = true;
    }

    public void Configure(string sceneName, string spawnId, LevelData levelToComplete = null)
    {
        targetSceneName = sceneName;
        targetSpawnId = spawnId;

        if (levelToComplete != null)
            completedLevel = levelToComplete;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading || !other.CompareTag("Player"))
            return;

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError($"PortalSceneLoader on '{name}' has no target scene assigned.");
            return;
        }

        StartCoroutine(LoadSceneRoutine());
    }

    private IEnumerator LoadSceneRoutine()
    {
        isLoading = true;

        if (disableAfterUse)
        {
            Collider2D triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null)
                triggerCollider.enabled = false;
        }

        if (completedLevel != null && GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.CompleteLevel(completedLevel);
            Debug.Log(
                $"PortalSceneLoader marked level '{completedLevel.levelName}' as complete.");
        }

        Debug.Log(
            $"PortalSceneLoader loading scene '{targetSceneName}' with SpawnId '{targetSpawnId}'.");

        if (loadDelay > 0f)
            yield return new WaitForSeconds(loadDelay);

        SpawnManager.LoadSceneAtSpawn(targetSceneName, targetSpawnId);
    }
}
