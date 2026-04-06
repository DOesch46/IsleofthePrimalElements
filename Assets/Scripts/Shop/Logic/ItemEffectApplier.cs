public static class ItemEffectApplier
{
    public static void Apply(ShopItemSO item, PlayerStats stats)
    {
        switch (item.effectType)
        {
            case ItemEffectType.SpeedBoost:
                stats.AddSpeedBonus(item.effectValue);
                break;
            case ItemEffectType.MaxHealthBoost:
                stats.AddMaxHealthBonus(item.effectValue);
                break;
            case ItemEffectType.AttackBoost:
                stats.AddAttackBonus(item.effectValue);
                break;
            case ItemEffectType.AbilityUnlock:
                stats.UnlockAbility(item.abilityId);
                break;
        }
    }
}