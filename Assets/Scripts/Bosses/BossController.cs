using UnityEngine;

public class BossController : MonoBehaviour
{
    public enum BossState
    {
        Fire,
        Water,
        Lightning,
        Earth
    }

    public BossState currentState;

    public GameObject fireBossPrefab;
    public GameObject waterBossPrefab;
    public GameObject lightningBossPrefab;
    public GameObject earthBossPrefab;

    public BossHealthUI healthBarUI;

    private GameObject currentBoss;
    private float currentHealth = -1f;

    void Start()
    {
        SetState(BossState.Fire);
    }

    public void SetState(BossState newState)
    {
        if (currentState == newState && currentBoss != null)
            return;

        currentState = newState;

        Debug.Log("Boss switched to: " + newState);

        Vector3 spawnPos = transform.position;

        if (currentBoss != null)
        {
            spawnPos = currentBoss.transform.position;

            EnemyHealth oldHealth = currentBoss.GetComponent<EnemyHealth>();
            if (oldHealth != null)
            {
                float hp = oldHealth.GetCurrentHealth();

            // 🔥 Treat near-zero as dead
            if (hp <= 0.1f)
    {
        currentHealth = -1f; // mark as dead
    }
        else
    {
        currentHealth = hp;
    }
            }

            Destroy(currentBoss);
        }

        GameObject prefabToSpawn = null;

        switch (currentState)
        {
            case BossState.Fire:
                prefabToSpawn = fireBossPrefab;
                break;
            case BossState.Water:
                prefabToSpawn = waterBossPrefab;
                break;
            case BossState.Lightning:
                prefabToSpawn = lightningBossPrefab;
                break;
            case BossState.Earth:
                prefabToSpawn = earthBossPrefab;
                break;
        }

        if (prefabToSpawn == null)
        {
            Debug.LogError("BossController: No prefab assigned for state " + currentState);
            return;
        }

        currentBoss = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, transform);

        EnemyHealth health = currentBoss.GetComponent<EnemyHealth>();

if (health != null)
{
    health.isFinalBoss = true; // 👑 ONLY THIS INSTANCE is final boss
}
        EnemyHealth newHealth = currentBoss.GetComponent<EnemyHealth>();
        if (newHealth != null)
        {
            if (currentHealth > 0)
            {
                newHealth.SetCurrentHealth(currentHealth);
            }

            if (healthBarUI != null)
            {
                healthBarUI.SetTarget(newHealth);
            }
        }
    }
}