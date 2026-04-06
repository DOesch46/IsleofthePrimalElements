using UnityEngine;

public class PlayerSpawnHandler : MonoBehaviour
{
    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        SpawnPoint targetSpawn = FindMatchingSpawnPoint();
        if (targetSpawn != null)
            player.transform.position = targetSpawn.transform.position;
    }

    private SpawnPoint FindMatchingSpawnPoint()
    {
        string transitionId = SceneTransitionManager.Instance?.GetLastTransitionId();
        SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        if (!string.IsNullOrEmpty(transitionId))
        {
            foreach (SpawnPoint sp in spawnPoints)
            {
                if (sp.TransitionId == transitionId)
                    return sp;
            }
        }

        foreach (SpawnPoint sp in spawnPoints)
        {
            if (sp.IsDefault)
                return sp;
        }

        return spawnPoints.Length > 0 ? spawnPoints[0] : null;
    }
}