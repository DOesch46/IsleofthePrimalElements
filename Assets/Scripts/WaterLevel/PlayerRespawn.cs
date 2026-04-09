using UnityEngine;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    private Rigidbody2D rb;
    private Vector3 fallbackRespawnPosition;
    private bool hasFallbackRespawnPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (spawnPoint == null)
        {
            Debug.LogWarning($"{name}: PlayerRespawn has no spawn point assigned. A fallback respawn position will be captured from the player's scene spawn.");
            StartCoroutine(CaptureFallbackRespawnPosition());
        }
        else
        {
            fallbackRespawnPosition = spawnPoint.position;
            hasFallbackRespawnPosition = true;
            Debug.Log($"{name}: PlayerRespawn is using assigned spawn point '{spawnPoint.name}' at {spawnPoint.position}.");
        }
    }

    public bool Respawn()
    {
        Vector3 targetPosition;
        string sourceName;

        if (spawnPoint != null)
        {
            targetPosition = spawnPoint.position;
            sourceName = spawnPoint.name;
        }
        else if (hasFallbackRespawnPosition)
        {
            targetPosition = fallbackRespawnPosition;
            sourceName = "captured fallback start position";
        }
        else
        {
            Debug.LogWarning($"{name}: Respawn failed because no spawn point or fallback respawn position is available.");
            return false;
        }

        transform.position = targetPosition;
        Debug.Log($"{name}: Respawned to '{sourceName}' at {targetPosition}.");

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        return true;
    }

    public void SetRespawnPosition(Vector3 position, string sourceLabel = null)
    {
        fallbackRespawnPosition = position;
        hasFallbackRespawnPosition = true;
        Debug.Log($"{name}: Updated respawn position to {position} from '{sourceLabel ?? "runtime assignment"}'.");
    }

    private IEnumerator CaptureFallbackRespawnPosition()
    {
        yield return null;
        yield return null;

        fallbackRespawnPosition = transform.position;
        hasFallbackRespawnPosition = true;
        Debug.Log($"{name}: Captured fallback respawn position at {fallbackRespawnPosition}.");
    }
}
