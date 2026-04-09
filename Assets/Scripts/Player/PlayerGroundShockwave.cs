using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// PlayerGroundShockwave — granted to the player when the Earth Boss dies.
///
/// SETUP:
///   1. Add this script to the Player GameObject.
///   2. Create an empty GameObject called "GroundSlamPrefab" in your Prefabs folder,
///      add a GroundSlamWave component to it, and assign it to the shockwavePrefab field.
///   3. Assign a wave sprite in the Inspector (or reuse the boss's wave sprite).
///   4. In your Input Actions asset, add an action called "UseShockwave" bound to a key
///      (e.g. Q or Space). Or use the fallback: it also fires on the same attack key
///      when ability is unlocked (see useAttackButtonFallback).
///   5. The ability starts LOCKED. Call UnlockAbility() from EarthBossAI on boss death.
/// </summary>
public class PlayerGroundShockwave : MonoBehaviour
{
    [Header("Ability Prefab")]
    [Tooltip("Prefab with GroundSlamWave component — same one the boss uses, or a new one.")]
    [SerializeField] private GameObject shockwavePrefab;

    [Header("Shockwave Stats")]
    [SerializeField] private float damage       = 20f;
    [SerializeField] private float waveSpeed    = 8f;
    [SerializeField] private Sprite waveSprite;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 4f;

    [Header("Fallback Input")]
    [Tooltip("If true, the ability fires when you press the dedicated 'UseShockwave' Input Action. " +
             "If your Input Actions don't have that action yet, tick 'useManualKeyFallback' below.")]
    [SerializeField] private bool useManualKeyFallback = false;
    [SerializeField] private KeyCode manualKey = KeyCode.Q;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private bool  isUnlocked    = false;
    private float cooldownTimer = 0f;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Update()
    {
        if (!isUnlocked) return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // Manual key fallback (no Input System action needed)
        if (useManualKeyFallback && Input.GetKeyDown(manualKey))
            TryFireShockwave();
    }

    // -------------------------------------------------------------------------
    // Input System Callback (Send Messages — same as PlayerCombat/PlayerController)
    // Add "UseShockwave" action to your Input Actions asset for this to fire.
    // -------------------------------------------------------------------------

    private void OnUseShockwave(InputValue value)
    {
        if (value.isPressed)
            TryFireShockwave();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by EarthBossAI when the boss dies to grant this ability.
    /// </summary>
    public void UnlockAbility()
    {
        if (isUnlocked) return;

        isUnlocked    = true;
        cooldownTimer = 0f;
        Debug.Log("Ground Shockwave UNLOCKED! Press Q (or your bound key) to use it.");

        // Optional: show a UI notification
        StartCoroutine(ShowUnlockMessage());
    }

    public bool IsUnlocked  => isUnlocked;
    public float CooldownRemaining => Mathf.Max(cooldownTimer, 0f);
    public float MaxCooldown       => cooldown;

    // -------------------------------------------------------------------------
    // Shockwave Logic
    // -------------------------------------------------------------------------

    private void TryFireShockwave()
    {
        if (!isUnlocked)   return;
        if (cooldownTimer > 0f) return;
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

        // Fire in 4 cardinal directions for a player-friendly area clear
        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };

        foreach (Vector2 dir in directions)
        {
            Vector2 spawnPos = (Vector2)transform.position + dir * 0.5f;
            GameObject waveObj = Instantiate(shockwavePrefab);
            GroundSlamWave wave = waveObj.GetComponent<GroundSlamWave>();

            if (wave != null)
                wave.Initialize(spawnPos, dir, waveSpeed, damage, waveSprite);
            else
            {
                Debug.LogError("PlayerGroundShockwave: shockwavePrefab is missing GroundSlamWave component!");
                Destroy(waveObj);
            }
        }

        // Camera shake feedback
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
        // Simple console message — hook into your UI here if you have one
        Debug.Log("=== NEW ABILITY: Ground Shockwave! ===");
        yield return new WaitForSeconds(0.1f);
        Debug.Log("Press Q to unleash a shockwave in all directions!");
    }
}
