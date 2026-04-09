using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class PlayerDeathScreenUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject deathScreenRoot;
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Binding")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private bool hideOnStart = true;

    [Header("Flow")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private bool pauseGameOnDeath = true;
    [SerializeField] private bool pauseAudioOnDeath = true;

    private void Awake()
    {
        if (respawnButton == null)
            Debug.LogWarning("PlayerDeathScreenUI: Respawn button reference is missing.");

        if (mainMenuButton == null)
            Debug.LogWarning("PlayerDeathScreenUI: Main menu button reference is missing.");

        if (hideOnStart)
            SetDeathScreenVisible(false);

        EnsureEventSystemExists();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;

        if (respawnButton != null)
            respawnButton.onClick.AddListener(OnRespawnButtonPressed);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuButtonPressed);

        StartCoroutine(RebindNextFrame());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (respawnButton != null)
            respawnButton.onClick.RemoveListener(OnRespawnButtonPressed);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuButtonPressed);

        UnbindPlayer();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"PlayerDeathScreenUI: Scene loaded '{scene.name}'. Rebinding death screen.");
        EnsureEventSystemExists();
        SetPaused(false);
        SetDeathScreenVisible(false);
        StartCoroutine(RebindNextFrame());
    }

    private IEnumerator RebindNextFrame()
    {
        yield return null;
        RebindPlayer();
    }

    private void RebindPlayer()
    {
        UnbindPlayer();

        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerDeathScreenUI: Could not find PlayerHealth to bind.");
            return;
        }

        playerHealth.OnDied += HandlePlayerDied;
        Debug.Log($"PlayerDeathScreenUI: Bound to player '{playerHealth.name}'.");
    }

    private void UnbindPlayer()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        Debug.Log("PlayerDeathScreenUI: Player died. Showing death screen.");
        SetPaused(true);
        SetDeathScreenVisible(true);
    }

    public void OnRespawnButtonPressed()
    {
        if (playerHealth == null)
            RebindPlayer();

        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerDeathScreenUI: Respawn button pressed, but no PlayerHealth is bound.");
            return;
        }

        Debug.Log("PlayerDeathScreenUI: Respawn button pressed.");

        if (playerHealth.RespawnFromDeathScreen())
        {
            SetPaused(false);
            SetDeathScreenVisible(false);
        }
    }

    public void OnMainMenuButtonPressed()
    {
        Debug.Log($"PlayerDeathScreenUI: Main menu button pressed. Loading '{mainMenuSceneName}'.");
        SetPaused(false);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SetDeathScreenVisible(bool visible)
    {
        if (deathScreenRoot == null)
        {
            Debug.LogWarning("PlayerDeathScreenUI: Death screen root is not assigned.");
            return;
        }

        deathScreenRoot.SetActive(visible);
        Debug.Log($"PlayerDeathScreenUI: Death screen visible = {visible}.");
    }

    private void SetPaused(bool paused)
    {
        if (pauseGameOnDeath)
            Time.timeScale = paused ? 0f : 1f;

        if (pauseAudioOnDeath)
            AudioListener.pause = paused;

        Debug.Log($"PlayerDeathScreenUI: Pause state set to {paused}. Time.timeScale={Time.timeScale} AudioPaused={AudioListener.pause}.");
    }

    private void EnsureEventSystemExists()
    {
        EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>();

        if (existingEventSystem != null)
        {
            Debug.Log($"PlayerDeathScreenUI: Using EventSystem '{existingEventSystem.name}'.");
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
        Debug.Log("PlayerDeathScreenUI: Created fallback EventSystem for UI interaction.");
    }
}
