using UnityEngine;
using UnityEngine.Rendering.Universal;

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

    [Header("Cutscene")]
    [SerializeField] private BossIntroCutscene introCutscene;

    private bool revealed = false;

    private void Start()
    {
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

        if (earthBoss != null)
            earthBoss.SetActive(true);

        if (bossHealthBar != null)
            bossHealthBar.gameObject.SetActive(true);

        Debug.Log("BOSS FIGHT BEGIN!");

        if (introCutscene != null)
            introCutscene.StartCutscene();
        else
        {
            // If no cutscene, start rocks directly
            if (rockSpawner != null)
                rockSpawner.StartSpawning();
        }
    }

    // Call this to reset everything for re-entry
    public void ResetArena()
    {
        revealed = false;

        if (earthBoss != null)
            earthBoss.SetActive(false);

        if (bossHealthBar != null)
            bossHealthBar.gameObject.SetActive(false);

        if (rockSpawner != null)
            rockSpawner.StopSpawning();

        Debug.Log("Arena reset for re-entry.");
    }
}