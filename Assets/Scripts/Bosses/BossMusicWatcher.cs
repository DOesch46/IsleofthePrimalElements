using UnityEngine;

/// <summary>
/// Tiny helper that watches for the boss to die, then stops the music,
/// activates the exit portal, notifies the level objective, and cleans itself up.
/// Created automatically by BossCutsceneTrigger.
/// You do NOT need to add this to anything manually.
/// </summary>
public class BossMusicWatcher : MonoBehaviour
{
    private GameObject boss;
    private AudioSource musicSource;
    private GameObject exitPortal;

    public void Setup(GameObject bossObj, AudioSource source)
    {
        boss = bossObj;
        musicSource = source;
    }

    public void SetExitPortal(GameObject portal)
    {
        exitPortal = portal;
    }

    private void Update()
    {
        // Boss has been destroyed (died)
        if (boss == null)
        {
            // Stop boss music
            if (musicSource != null)
                musicSource.Stop();

            // Activate the exit portal
            if (exitPortal != null)
                exitPortal.SetActive(true);

            // Notify level objective system
            if (LevelObjective.Instance != null)
                LevelObjective.Instance.BossDefeated();

            Destroy(gameObject);
        }
    }
}
