using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerGroundShockwave : MonoBehaviour
{
    [Header("Ability Prefab")]
    [SerializeField] private GameObject shockwavePrefab;

    [Header("Shockwave Stats")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float waveSpeed = 8f;
    [SerializeField] private Sprite waveSprite;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 4f;

    [Header("Input")]
    [SerializeField] private KeyCode abilityKey = KeyCode.T;

    // State
    private bool isUnlocked = false;
    private float cooldownTimer = 0f;
    private const string UNLOCK_KEY = "GroundShockwaveUnlocked";

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        // Check if ability was previously unlocked
        if (PlayerPrefs.GetInt(UNLOCK_KEY, 0) == 1)
        {
            isUnlocked = true;
            Debug.Log("Ground Shockwave already unlocked from previous session.");
        }

        // Auto-generate sprite if none assigned
        if (waveSprite == null)
        {
            ShockwaveSpriteGenerator generator = gameObject.AddComponent<ShockwaveSpriteGenerator>();
            waveSprite = generator.CreateShockwaveSprite();
            Destroy(generator);
            Debug.Log("Auto-generated shockwave sprite.");
        }
    }

    private void Update()
    {
        if (!isUnlocked) return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(abilityKey))
        {
            Debug.Log($"T key pressed! Unlocked={isUnlocked}, Cooldown={cooldownTimer:F1}");
            TryFireShockwave();
        }
    }

    // -------------------------------------------------------------------------
    // Input System Callback (optional)
    // -------------------------------------------------------------------------

    private void OnUseShockwave(InputValue value)
    {
        if (value.isPressed)
            TryFireShockwave();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void UnlockAbility()
    {
        if (isUnlocked) return;

        isUnlocked = true;
        cooldownTimer = 0f;

        PlayerPrefs.SetInt(UNLOCK_KEY, 1);
        PlayerPrefs.Save();

        Debug.Log("Ground Shockwave UNLOCKED! Press T to use it.");
        StartCoroutine(ShowUnlockMessage());
    }

    public bool IsUnlocked => isUnlocked;
    public float CooldownRemaining => Mathf.Max(cooldownTimer, 0f);
    public float MaxCooldown => cooldown;

    public static void ResetUnlock()
    {
        PlayerPrefs.DeleteKey("GroundShockwaveUnlocked");
        PlayerPrefs.Save();
    }

    // -------------------------------------------------------------------------
    // Shockwave Logic
    // -------------------------------------------------------------------------

    private void TryFireShockwave()
    {
        if (!isUnlocked)
        {
            Debug.Log("Shockwave not unlocked yet!");
            return;
        }
        if (cooldownTimer > 0f)
        {
            Debug.Log($"Shockwave on cooldown: {cooldownTimer:F1}s remaining");
            return;
        }
        if (shockwavePrefab == null)
        {
            Debug.LogWarning("PlayerGroundShockwave: shockwavePrefab is not assigned!");
            return;
        }

        FireShockwave();
    }

    private void FireShockwave()
    {
        cooldownTimer = cooldown;

        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };

        foreach (Vector2 dir in directions)
        {
            Vector2 spawnPos = (Vector2)transform.position + dir * 0.5f;
            GameObject waveObj = Instantiate(shockwavePrefab);
            GroundSlamWave wave = waveObj.GetComponent<GroundSlamWave>();

            if (wave != null)
                wave.InitializeAsPlayer(spawnPos, dir, waveSpeed, damage, waveSprite);  // ✅ Changed
            else
            {
                Debug.LogError("shockwavePrefab missing GroundSlamWave component!");
                Destroy(waveObj);
            }
        }

        CameraShake shake = Camera.main?.GetComponent<CameraShake>();
        if (shake != null)
            shake.Shake(0.25f, 0.12f);

        Debug.Log("Player fired Ground Shockwave!");
    }

    // -------------------------------------------------------------------------
    // UI Feedback
    // -------------------------------------------------------------------------

    private IEnumerator ShowUnlockMessage()
    {
        Debug.Log("=== NEW ABILITY: Ground Shockwave! ===");
        yield return new WaitForSeconds(0.1f);
        Debug.Log("Press T to unleash a shockwave in all directions!");
    }
}