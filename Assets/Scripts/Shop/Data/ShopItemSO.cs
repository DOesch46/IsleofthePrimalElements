using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Shop Item")]
public class ShopItemSO : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public int price;

    [Header("Effect")]
    public ItemEffectType effectType;
    public float effectValue;
    public string abilityId; // Only used when effectType == AbilityUnlock
}