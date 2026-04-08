using UnityEngine;
using System.Collections.Generic;

// Kept for compatibility. No longer used by the upgrade-based shop system.
[CreateAssetMenu(fileName = "PlayerInventory", menuName = "Player/Inventory")]
public class PlayerInventorySO : ScriptableObject
{
    public List<ShopItemSO> ownedItems = new List<ShopItemSO>();

    public void Clear()
    {
        ownedItems.Clear();
    }
}
