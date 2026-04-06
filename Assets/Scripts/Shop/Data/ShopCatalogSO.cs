using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewShopCatalog", menuName = "Shop/Shop Catalog")]
public class ShopCatalogSO : ScriptableObject
{
    public List<ShopItemSO> items = new List<ShopItemSO>();
}