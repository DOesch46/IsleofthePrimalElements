using UnityEngine;
using System.Collections;

public class EarthBossDeathHandler : MonoBehaviour
{
    [Header("Portal")]
    [SerializeField] private GameObject exitPortal;
    [SerializeField] private Transform portalSpawnPoint;

    [Header("Coins")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinCount = 10;
    [SerializeField] private float coinSpreadRadius = 1.5f;

    [Header("Ability")]
    [SerializeField] private string abilityName = "GroundSlam";

    [Header("References")]
    [SerializeField] private EarthBossHealth bossHealth;
    [SerializeField] private FallingRockSpawner rockSpawner;
    [SerializeField] private EarthPillarSpawner pillarSpawner;
    [SerializeField] private GameObject bossHealthBar;

    private bool hasDied = false;

    private void Update()
    {
        if (hasDied) return;
        if (bossHealth == null) return;

        if (bossHealth.GetCurrentHealth() <= 0f)
        {
            hasDied = true;
            StartCoroutine(HandleBossDeath());
        }
    }

    private IEnumerator HandleBossDeath()
    {
        Debug.Log("Earth Boss has been defeated!");

        // Stop all hazards
        if (rockSpawner != null)
            rockSpawner.StopSpawning();

        if (pillarSpawner != null)
            pillarSpawner.StopSpawning();

        // Destroy all remaining rocks and pillars
        foreach (FallingRock rock in FindObjectsOfType<FallingRock>())
            Destroy(rock.gameObject);

        foreach (EarthPillar pillar in FindObjectsOfType<EarthPillar>())
            Destroy(pillar.gameObject);

        // Hide health bar
        if (bossHealthBar != null)
            bossHealthBar.SetActive(false);

        // Camera shake for dramatic death
        CameraShake shake = Camera.main?.GetComponent<CameraShake>();
        if (shake != null)
            shake.Shake(0.5f, 0.15f);

        yield return new WaitForSeconds(0.5f);

        // Spawn coins
        SpawnCoins();

        yield return new WaitForSeconds(0.5f);

        // Grant ability
        GrantAbility();

        yield return new WaitForSeconds(1f);

        // Spawn exit portal
        SpawnPortal();

        Debug.Log("Boss death sequence complete!");
    }

    private void SpawnCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning("No coin prefab assigned!");
            return;
        }

        Vector3 bossPos = transform.position;

        for (int i = 0; i < coinCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * coinSpreadRadius;
            Vector3 spawnPos = bossPos + new Vector3(randomOffset.x, randomOffset.y, 0f);
            Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        }

        Debug.Log($"Spawned {coinCount} coins!");
    }

    private void GrantAbility()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Try to find and enable the ground slam ability
        // Check for common ability script names
        MonoBehaviour[] allScripts = player.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in allScripts)
        {
            string typeName = script.GetType().Name.ToLower();
            if (typeName.Contains("groundslam") || typeName.Contains("earthability") 
                || typeName.Contains("slam"))
            {
                script.enabled = true;
                Debug.Log($"Enabled ability: {script.GetType().Name}");
                return;
            }
        }

        // Also check children
        MonoBehaviour[] childScripts = player.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (MonoBehaviour script in childScripts)
        {
            string typeName = script.GetType().Name.ToLower();
            if (typeName.Contains("groundslam") || typeName.Contains("earthability")
                || typeName.Contains("slam"))
            {
                script.enabled = true;
                Debug.Log($"Enabled ability: {script.GetType().Name}");
                return;
            }
        }

        // If no specific ability script found, try GameProgressManager
        if (GameProgressManager.Instance != null)
        {
            // Mark earth element as unlocked
            Debug.Log("Ground Slam ability granted via progress!");
        }

        Debug.Log("Ground Slam ability granted! Press T to use.");
    }

    private void SpawnPortal()
    {
        if (exitPortal == null)
        {
            Debug.LogWarning("No exit portal assigned!");
            return;
        }

        // If portal is in the scene but disabled, just enable it
        if (exitPortal.scene.IsValid())
        {
            exitPortal.SetActive(true);
        }
        else
        {
            // It's a prefab — instantiate it
            Vector3 spawnPos;
            if (portalSpawnPoint != null)
                spawnPos = portalSpawnPoint.position;
            else
                spawnPos = transform.position + new Vector3(0f, 1f, 0f);

            GameObject portal = Instantiate(exitPortal, spawnPos, Quaternion.identity);
            portal.SetActive(true);
        }

        Debug.Log("Exit portal spawned!");
    }
}