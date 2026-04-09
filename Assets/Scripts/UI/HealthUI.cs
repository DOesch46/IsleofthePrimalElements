using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class HealthUI : MonoBehaviour
{
    public Image fill;
    public PlayerHealth player;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        RebindPlayer();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        UnbindPlayer();
    }

    private void Start()
    {
        RebindPlayer();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"HealthUI: Scene loaded '{scene.name}'. Rebinding player health UI.");
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

        if (fill == null)
        {
            Debug.LogWarning("HealthUI: Fill image is not assigned.");
            return;
        }

        if (player == null)
            player = FindFirstObjectByType<PlayerHealth>();

        if (player == null)
        {
            Debug.LogWarning("HealthUI: Could not find PlayerHealth to bind.");
            fill.fillAmount = 0f;
            return;
        }

        player.OnHealthChanged += UpdateHealth;
        Debug.Log($"HealthUI: Bound to player '{player.name}'.");
        UpdateHealth(player.GetCurrentHealth(), player.GetMaxHealth());
    }

    private void UnbindPlayer()
    {
        if (player != null)
            player.OnHealthChanged -= UpdateHealth;
    }

    private void UpdateHealth(float current, float max)
    {
        if (fill == null)
            return;

        float normalized = max <= 0f ? 0f : current / max;
        fill.fillAmount = normalized;
        Debug.Log($"HealthUI: Updated fill to {normalized:F2} ({current}/{max}).");
    }
}
