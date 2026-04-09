using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ArenaRevealTrigger — fixed activation order:
///   Boss is enabled BEFORE the health bar so OnEnable() on BossHealthBarUI
///   can find EarthBossHealth and subscribe to events correctly.
/// </summary>
public class ArenaRevealTrigger : MonoBehaviour
{
    [Header("Lighting")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Light2D playerSpotlight;

    [Header("Light Settings")]
    [SerializeField] private float brightIntensity = 1f;
    [SerializeField] private float fadeSpeed = 2f;

    [Header("Boss")]
    [SerializeField] private GameObject earthBoss;
    [SerializeField] private BossHealthBarUI bossHealthBar;

    [Header("Rocks")]
    [SerializeField] private FallingRockSpawner rockSpawner;

    private bool revealed = false;

    private void Start()
    {
        // Keep boss and UI hidden until player enters
        if (earthBoss != null)
            earthBoss.SetActive(false);

        if (bossHealthBar != null)
            bossHealthBar.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (revealed)
        {
            if (globalLight != null)
                globalLight.intensity = Mathf.Lerp(globalLight.intensity, brightIntensity, fadeSpeed * Time.deltaTime);

            if (playerSpotlight != null)
                playerSpotlight.intensity = Mathf.Lerp(playerSpotlight.intensity, 0f, fadeSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (revealed) return;

        revealed = true;

        // FIX: Enable boss FIRST so EarthBossHealth exists when the health bar subscribes
        if (earthBoss != null)
            earthBoss.SetActive(true);

        // THEN enable the health bar — its OnEnable() will find EarthBossHealth
        if (bossHealthBar != null)
            bossHealthBar.gameObject.SetActive(true);

        if (rockSpawner != null)
            rockSpawner.StartSpawning();

        Debug.Log("BOSS FIGHT BEGIN!");
    }
}