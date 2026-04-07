using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [Header("Data References")]
    [SerializeField] private PlayerInventorySO inventory;

    [Header("Base Stats")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseAttack = 10f;

    private float bonusSpeed;
    private float bonusMaxHealth;
    private float bonusAttack;
    private HashSet<string> unlockedAbilities = new HashSet<string>();

    public event Action OnStatsChanged;

    public float Speed => baseSpeed + bonusSpeed;
    public float MaxHealth => baseMaxHealth + bonusMaxHealth;
    public float Attack => baseAttack + bonusAttack;

    private void Start()
    {
        ResetBonuses();
        ApplyAllOwnedItems();
        PushStatsToSystems();
    }

    public void AddSpeedBonus(float amount)
    {
        bonusSpeed += amount;
        PushStatsToSystems();
        OnStatsChanged?.Invoke();
    }

    public void AddMaxHealthBonus(float amount)
    {
        bonusMaxHealth += amount;
        PushStatsToSystems();
        OnStatsChanged?.Invoke();
    }

    public void AddAttackBonus(float amount)
    {
        bonusAttack += amount;
        PushStatsToSystems();
        OnStatsChanged?.Invoke();
    }

    public void UnlockAbility(string abilityId)
    {
        if (!string.IsNullOrEmpty(abilityId))
            unlockedAbilities.Add(abilityId);
    }

    public bool HasAbility(string abilityId) => unlockedAbilities.Contains(abilityId);

    private void ResetBonuses()
    {
        bonusSpeed = 0f;
        bonusMaxHealth = 0f;
        bonusAttack = 0f;
        unlockedAbilities.Clear();
    }

    private void ApplyAllOwnedItems()
    {
        if (inventory == null) return;

        foreach (ShopItemSO item in inventory.ownedItems)
        {
            ItemEffectApplier.Apply(item, this);
        }
    }

    private void PushStatsToSystems()
    {
        MovementSystem movement = GetComponent<MovementSystem>();
        if (movement != null)
            movement.SetMoveSpeed(Speed);

        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
            combat.SetAttackDamage(Attack);

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
            health.SetMaxHealth(MaxHealth);
    }
}