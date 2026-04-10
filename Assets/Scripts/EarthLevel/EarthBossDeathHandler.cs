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

    [Header("References")]
    [SerializeField] private EnemyHealth bossHealth;  // ✅ Changed from EarthBossHealth
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

        if (rockSpawner != null)
            rockSpawner.StopSpawning();

        if (pillarSpawner != null)
            pillarSpawner.StopSpawning();

        foreach (FallingRock rock in FindObjectsOfType<FallingRock>())
            Destroy(rock.gameObject);

        foreach (EarthPillar pillar in FindObjectsOfType<EarthPillar>())
            Destroy(pillar.gameObject);

        if (bossHealthBar != null)
            bossHealthBar.SetActive(false);

        CameraShake shake = Camera.main?.GetComponent<CameraShake>();
        if (shake != null)
            shake.Shake(0.5f, 0.15f);

        yield return new WaitForSeconds(0.5f);

        SpawnCoins();

        yield return new WaitForSeconds(0.5f);

        GrantAbility();

        yield return new WaitForSeconds(1f);

        SpawnPortal();

        // ✅ Destroy the boss after the death sequence
        Destroy(bossHealth.gameObject);

        Debug.Log("Boss death sequence complete!");
    }

    private void SpawnCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning("No coin prefab assigned!");
            return;
        }

        Vector3 bossPos = bossHealth != null ? bossHealth.transform.position : transform.position;

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
        if (player != null)
        {
            PlayerGroundShockwave shockwave = player.GetComponent<PlayerGroundShockwave>();
            if (shockwave != null)
            {
                shockwave.UnlockAbility();
                Debug.Log("Ground Slam ability granted! Press T to use.");
                return;
            }
            else
            {
                Debug.LogWarning("PlayerGroundShockwave script not found on player!");
            }
        }

        PlayerPrefs.SetInt("GroundShockwaveUnlocked", 1);
        PlayerPrefs.Save();
        Debug.Log("Ground Slam saved to PlayerPrefs for next scene.");
    }

    private void SpawnPortal()
    {
        if (exitPortal == null)
        {
            Debug.LogWarning("No exit portal assigned!");
            return;
        }

        if (exitPortal.scene.IsValid())
        {
            exitPortal.SetActive(true);
        }
        else
        {
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