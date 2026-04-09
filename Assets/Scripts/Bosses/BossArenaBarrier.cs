using UnityEngine;

/// <summary>
/// Spawns an invisible wall at the boss arena entrance when the cutscene triggers.
/// Blocks the player from leaving and prevents mini enemies from entering.
///
/// This is automatically created by BossCutsceneTrigger — you do NOT need to
/// add this to anything manually.
/// </summary>
public class BossArenaBarrier : MonoBehaviour
{
    private GameObject trackedBoss;
    private PlayerHealth trackedPlayerHealth;

    public void Setup(GameObject bossObject, PlayerHealth playerHealth)
    {
        trackedBoss = bossObject;

        if (trackedPlayerHealth != null)
            trackedPlayerHealth.OnDied -= HandlePlayerDied;

        trackedPlayerHealth = playerHealth;

        if (trackedPlayerHealth != null)
            trackedPlayerHealth.OnDied += HandlePlayerDied;

        Debug.Log($"BossArenaBarrier: Setup complete. Boss='{trackedBoss?.name ?? "None"}', Player='{trackedPlayerHealth?.name ?? "None"}'.");
    }

    private void Update()
    {
        if (trackedBoss == null)
        {
            Debug.Log("BossArenaBarrier: Boss is gone. Removing arena barrier.");
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If a mini enemy hits the barrier, destroy it
        if (collision.gameObject.GetComponent<EnemyAI>() != null)
        {
            Destroy(collision.gameObject);
        }
    }

    private void HandlePlayerDied()
    {
        Debug.Log("BossArenaBarrier: Player died. Removing arena barrier so the player can re-enter the arena.");
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (trackedPlayerHealth != null)
            trackedPlayerHealth.OnDied -= HandlePlayerDied;
    }
}
