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

        SetPendingSpawn(sceneName, spawnId);
        Debug.Log($"SpawnManager loading scene '{sceneName}'.");
        SceneManager.LoadScene(sceneName);
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
}
