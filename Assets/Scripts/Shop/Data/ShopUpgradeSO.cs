using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Shop/Upgrade")]
public class ShopUpgradeSO : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Upgrade Settings")]
    public UpgradeType upgradeType;
    public float valuePerLevel;     // How much each purchase adds
    public int baseCost;            // Cost of first purchase
    public int costIncrement;       // How much more each purchase costs
    public int maxLevel;            // 0 = unlimited

    /// <summary>
    /// Returns the cost for the next purchase given the current level.
    /// </summary>
    public int GetCost(int currentLevel)
    {
        return baseCost + (costIncrement * currentLevel);
    }

    public bool IsMaxed(int currentLevel)
    {
        return maxLevel > 0 && currentLevel >= maxLevel;
    }
}
