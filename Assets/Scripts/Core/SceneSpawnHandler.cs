using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Scene-level handler that moves the player to the requested SpawnPoint
/// after the scene finishes loading.
/// </summary>
public class SceneSpawnHandler : MonoBehaviour
{
    [Header("Spawn Handler")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private int playerLookupFrames = 10;

    protected virtual void Start()
    {
        StartCoroutine(PositionPlayerAfterSceneLoad());
    }

    private IEnumerator PositionPlayerAfterSceneLoad()
    {
        GameObject player = null;

        for (int i = 0; i < Mathf.Max(1, playerLookupFrames); i++)
        {
            player = GameObject.FindWithTag(playerTag);
            if (player != null)
                break;

            yield return null;
        }

        if (player == null)
        {
            Debug.LogWarning(
                $"SceneSpawnHandler could not find a player with tag '{playerTag}' in scene '{SceneManager.GetActiveScene().name}'.");
            yield break;
        }

        SpawnPoint targetSpawn = FindTargetSpawnPoint(SceneManager.GetActiveScene().name);
        if (targetSpawn == null)
        {
            Debug.LogWarning(
                $"SceneSpawnHandler found no SpawnPoint in scene '{SceneManager.GetActiveScene().name}'.");
            yield break;
        }

        player.transform.position = targetSpawn.transform.position;

        Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
        if (playerBody != null)
            playerBody.linearVelocity = Vector2.zero;

        Debug.Log(
            $"SceneSpawnHandler placed player at SpawnPoint '{targetSpawn.SpawnId}' in scene '{SceneManager.GetActiveScene().name}'.");
    }

    protected virtual SpawnPoint FindTargetSpawnPoint(string sceneName)
    {
        string requestedSpawnId = SpawnManager.ConsumePendingSpawnId(sceneName);
        SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        if (spawnPoints.Length == 0)
            return null;

        if (!string.IsNullOrWhiteSpace(requestedSpawnId))
        {
            foreach (SpawnPoint spawnPoint in spawnPoints)
            {
                if (spawnPoint.Matches(requestedSpawnId))
                {
                    Debug.Log(
                        $"SceneSpawnHandler matched requested SpawnId '{requestedSpawnId}'.");
                    return spawnPoint;
                }
            }

            Debug.LogWarning(
                $"SceneSpawnHandler could not find SpawnId '{requestedSpawnId}' in scene '{sceneName}'. Falling back.");
        }

        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.IsDefault)
            {
                Debug.Log(
                    $"SceneSpawnHandler is using default SpawnPoint '{spawnPoint.SpawnId}'.");
                return spawnPoint;
            }
        }

        Debug.Log(
            $"SceneSpawnHandler is using the first SpawnPoint '{spawnPoints[0].SpawnId}' because no default was marked.");
        return spawnPoints[0];
    }
}
