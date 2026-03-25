using UnityEngine;

/// <summary>
/// Manages all portals on the MainIsland hub world.
/// Handles unlocking portals after coin collection.
/// Place ONE of these in the MainIsland scene.
/// </summary>
public class PortalManager : MonoBehaviour
{
    [Header("Portal References")]
    [Tooltip("Drag all BiomePortal objects here")]
    [SerializeField] private BiomePortal[] allPortals;

    [Header("Unlock Settings")]
    [Tooltip("How many coins needed to unlock portals")]
    [SerializeField] private int coinsToUnlock = 10;
    
    [Tooltip("Are portals unlocked from the start? (Set false if coins are required first)")]
    [SerializeField] private bool portalsStartUnlocked = false;

    [Header("Debug Info")]
    [SerializeField] private int currentCoins = 0;
    [SerializeField] private bool portalsUnlocked = false;

    // Singleton for easy access
    private static PortalManager instance;
    public static PortalManager Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // If portals start unlocked, unlock them all immediately
        if (portalsStartUnlocked)
        {
            UnlockAllPortals();
        }
        else
        {
            LockAllPortals();
        }

        // Reset the portal choice flag when returning to hub
        BiomePortal.ResetPortalChoice();
        
        Debug.Log($"PortalManager ready. Portals unlocked: {portalsStartUnlocked}. Need {coinsToUnlock} coins.");
    }

    /// <summary>
    /// Call this when the player collects a coin.
    /// </summary>
    public void AddCoin()
    {
        currentCoins++;
        Debug.Log($"Coin collected! {currentCoins}/{coinsToUnlock}");

        // Check if enough coins to unlock portals
        if (!portalsUnlocked && currentCoins >= coinsToUnlock)
        {
            Debug.Log("Enough coins collected! Portals are now unlocked!");
            Debug.Log("Choose your path by going through any of the portals!");
            UnlockAllPortals();
        }
    }

    /// <summary>
    /// Sets the coin count directly (useful if coins are tracked elsewhere).
    /// </summary>
    public void SetCoinCount(int count)
    {
        currentCoins = count;
        
        if (!portalsUnlocked && currentCoins >= coinsToUnlock)
        {
            UnlockAllPortals();
        }
    }

    /// <summary>
    /// Gets current coin count.
    /// </summary>
    public int GetCoinCount()
    {
        return currentCoins;
    }

    /// <summary>
    /// Unlocks all elemental portals (not boss portal - that checks elements).
    /// </summary>
    public void UnlockAllPortals()
    {
        portalsUnlocked = true;
        
        foreach (BiomePortal portal in allPortals)
        {
            if (portal != null)
            {
                portal.UpdatePortalState();
            }
        }
        
        Debug.Log("All portals updated!");
    }

    /// <summary>
    /// Locks all portals.
    /// </summary>
    public void LockAllPortals()
    {
        foreach (BiomePortal portal in allPortals)
        {
            if (portal != null)
            {
                portal.Lock();
            }
        }
    }

    /// <summary>
    /// Debug: Unlock all portals immediately.
    /// </summary>
    [ContextMenu("Debug: Unlock All Portals")]
    public void DebugUnlockAll()
    {
        currentCoins = coinsToUnlock;
        UnlockAllPortals();
    }

    /// <summary>
    /// Debug: Add 10 coins.
    /// </summary>
    [ContextMenu("Debug: Add 10 Coins")]
    public void DebugAddCoins()
    {
        for (int i = 0; i < 10; i++)
        {
            AddCoin();
        }
    }
}