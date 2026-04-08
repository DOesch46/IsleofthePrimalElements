using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [Header("Data References")]
    [SerializeField] private PlayerWalletSO wallet;

    // Base stats are constants — upgrades come only from the shop
    private const float BASE_SPEED          = 5f;
    private const float BASE_MAX_HEALTH     = 100f;
    private const float BASE_ATTACK         = 10f;
    private const float BASE_ATTACK_RANGE   = 4f;
    private const float BASE_ATTACK_COOLDOWN = 0.5f;

    private float bonusSpeed;
    private float bonusMaxHealth;
    private float bonusAttack;

    public event Action OnStatsChanged;

    public float Speed     => BASE_SPEED      + bonusSpeed;
    public float MaxHealth => BASE_MAX_HEALTH  + bonusMaxHealth;
    public float Attack    => BASE_ATTACK      + bonusAttack;

    private void Awake()
    {
        ResetBonuses();
        LoadUpgradesFromPrefs();

        // Sync wallet with GameProgressManager so shop shows correct coin count
        if (wallet != null)
            wallet.SyncFromProgress();
    }

    private void Start()
    {
        // Push in Start so all other components have finished Awake
        PushStatsToSystems();
    }

    /// <summary>
    /// Called by ShopManager after a purchase to refresh stats immediately.
    /// </summary>
    public void ApplyUpgrades(ShopManager shop)
    {
        ResetBonuses();

        bonusSpeed     = shop.GetUpgradeLevel(UpgradeType.Speed) * 1f;    // +1 per level
        bonusMaxHealth = shop.GetUpgradeLevel(UpgradeType.MaxHealth) * 50f; // +50 per level
        bonusAttack    = shop.GetUpgradeLevel(UpgradeType.Attack) * 10f;   // +10 per level

        PushStatsToSystems();
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// Loads upgrade levels from PlayerPrefs so stats are correct even
    /// when no ShopManager exists in the current scene.
    /// </summary>
    private void LoadUpgradesFromPrefs()
    {
        int speedLevel  = PlayerPrefs.GetInt("Elementara_Upgrade_Speed", 0);
        int healthLevel = PlayerPrefs.GetInt("Elementara_Upgrade_MaxHealth", 0);
        int attackLevel = PlayerPrefs.GetInt("Elementara_Upgrade_Attack", 0);

        bonusSpeed     = speedLevel  * 1f;
        bonusMaxHealth = healthLevel * 50f;
        bonusAttack    = attackLevel * 10f;
    }

    private void ResetBonuses()
    {
        bonusSpeed = 0f;
        bonusMaxHealth = 0f;
        bonusAttack = 0f;
    }

    private void PushStatsToSystems()
    {
        MovementSystem movement = GetComponent<MovementSystem>();
        if (movement != null)
            movement.SetMoveSpeed(Speed);

        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
        {
            combat.SetAttackDamage(Attack);
            combat.SetAttackRange(BASE_ATTACK_RANGE);
            combat.SetAttackCooldown(BASE_ATTACK_COOLDOWN);
        }

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
            health.SetMaxHealth(MaxHealth);
    }
}
