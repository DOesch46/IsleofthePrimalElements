using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static scene spawn coordinator.
/// Stores a pending spawn ID before loading a scene, then lets the
/// destination scene consume that ID exactly once.
/// </summary>
public static class SpawnManager
{
    private static string pendingSceneName;
    private static string pendingSpawnId;
    private static bool hasPendingPlayerHealth;
    private static float pendingPlayerHealthFraction = 1f;

    public static string PendingSceneName => pendingSceneName;
    public static string PendingSpawnId => pendingSpawnId;

    public static void SetPendingSpawn(string sceneName, string spawnId)
    {
        pendingSceneName = sceneName;
        pendingSpawnId = spawnId;

        Debug.Log(
            $"SpawnManager queued spawn. Scene='{pendingSceneName}', SpawnId='{pendingSpawnId}'.");
    }

    public static void LoadSceneAtSpawn(string sceneName, string spawnId)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("SpawnManager cannot load a scene because sceneName is empty.");
            return;
        }

        CaptureCurrentPlayerHealthForNextScene();
        SetPendingSpawn(sceneName, spawnId);
        Debug.Log($"SpawnManager loading scene '{sceneName}'.");
        SceneManager.LoadScene(sceneName);
    }

    public static void CaptureCurrentPlayerHealthForNextScene()
    {
        PlayerHealth playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("SpawnManager could not find PlayerHealth to persist before scene load.");
            ClearPendingPlayerHealth();
            return;
        }

        float maxHealth = playerHealth.GetMaxHealth();
        float currentHealth = playerHealth.GetCurrentHealth();
        pendingPlayerHealthFraction = maxHealth <= 0f ? 1f : Mathf.Clamp01(currentHealth / maxHealth);
        hasPendingPlayerHealth = true;

        Debug.Log(
            $"SpawnManager captured player health for next scene. Current={currentHealth}, Max={maxHealth}, Fraction={pendingPlayerHealthFraction:F2}.");
    }

    public static bool TryConsumePendingPlayerHealthFraction(out float healthFraction)
    {
        if (!hasPendingPlayerHealth)
        {
            healthFraction = 1f;
            return false;
        }

        healthFraction = pendingPlayerHealthFraction;
        hasPendingPlayerHealth = false;
        pendingPlayerHealthFraction = 1f;

        Debug.Log($"SpawnManager consumed persisted player health fraction {healthFraction:F2}.");
        return true;
    }

    public static bool HasPendingSpawnForScene(string sceneName)
    {
        return !string.IsNullOrWhiteSpace(pendingSceneName) &&
               string.Equals(pendingSceneName, sceneName, System.StringComparison.Ordinal);
    }

    public static string ConsumePendingSpawnId(string sceneName)
    {
        if (!HasPendingSpawnForScene(sceneName))
            return string.Empty;

        string consumedSpawnId = pendingSpawnId;

        Debug.Log(
            $"SpawnManager consumed spawn. Scene='{sceneName}', SpawnId='{consumedSpawnId}'.");

        ClearPendingSpawn();
        return consumedSpawnId;
    }

    public static void ClearPendingSpawn()
    {
        pendingSceneName = string.Empty;
        pendingSpawnId = string.Empty;
    }

    public static void ClearPendingPlayerHealth()
    {
        hasPendingPlayerHealth = false;
        pendingPlayerHealthFraction = 1f;
    }
}
