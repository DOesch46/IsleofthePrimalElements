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
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If a mini enemy hits the barrier, destroy it
        if (collision.gameObject.GetComponent<EnemyAI>() != null)
        {
            Destroy(collision.gameObject);
        }
    }
}
