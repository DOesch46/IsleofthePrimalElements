using UnityEngine;
using UnityEngine.SceneManagement;

public class BossTriggerSimple : MonoBehaviour
{
    [SerializeField] private GameObject bossObject;
    [SerializeField] private string bossProgressId = string.Empty;

    private bool activated = false;

    private void Start()
    {
        if (ShouldSuppressBossSpawn())
        {
            Debug.Log($"{name}: Boss spawn suppressed because this boss is already defeated.");

            BossPortalSpawner portalSpawner = bossObject != null
                ? bossObject.GetComponent<BossPortalSpawner>()
                : null;

            if (portalSpawner != null)
            {
                portalSpawner.EnsurePortalExistsForCompletedBoss();
            }
            else
            {
                Debug.LogWarning($"{name}: Boss spawn is suppressed, but no BossPortalSpawner was found to recreate the return portal.");
            }

            if (bossObject != null)
                bossObject.SetActive(false);

            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (ShouldSuppressBossSpawn())
        {
            Debug.Log($"{name}: Boss trigger ignored because boss defeat state is already recorded.");
            gameObject.SetActive(false);
            return;
        }

        if (other.CompareTag("Player"))
        {
            activated = true;
            
            if (bossObject != null)
            {
                bossObject.SetActive(true);
                Debug.Log($"{name}: Water boss spawned for the first encounter.");
            }

            gameObject.SetActive(false); // disables trigger after use
        }
    }

    private bool ShouldSuppressBossSpawn()
    {
        if (GameProgressManager.Instance == null)
            return false;

        string resolvedBossProgressId = ResolveBossProgressId();
        bool levelCompleted = GameProgressManager.Instance.IsLevelCompleted(SceneManager.GetActiveScene().name);
        bool bossDefeated = !string.IsNullOrWhiteSpace(resolvedBossProgressId) &&
                            GameProgressManager.Instance.IsBossDefeated(resolvedBossProgressId);

        if (bossDefeated || levelCompleted)
        {
            Debug.Log(
                $"{name}: Boss spawn suppression check passed. bossDefeated={bossDefeated}, levelCompleted={levelCompleted}, bossId='{resolvedBossProgressId}'.");
        }

        return bossDefeated || levelCompleted;
    }

    private string ResolveBossProgressId()
    {
        if (!string.IsNullOrWhiteSpace(bossProgressId))
            return bossProgressId;

        return SceneManager.GetActiveScene().name + "_Boss";
    }
}
