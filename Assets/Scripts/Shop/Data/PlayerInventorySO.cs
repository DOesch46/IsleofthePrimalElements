using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerInventory", menuName = "Player/Inventory")]

public class PlayerInventorySO : ScriptableObject
{
    public List<ShopItemSO> ownedItems = new List<ShopItemSO>();

    public event Action<ShopItemSO> OnItemAdded;

    public bool HasItem(ShopItemSO item) => ownedItems.Contains(item);

    public void AddItem(ShopItemSO item)
    {
        if (HasItem(item)) return;

        ownedItems.Add(item);
        OnItemAdded?.Invoke(item);
    }
    
    public void ResetInventory()
    {
        ownedItems.Clear();
    }

    public void Clear()
    {
        ownedItems.Clear();
    }
}
