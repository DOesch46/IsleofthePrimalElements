using UnityEngine;
using System;

[CreateAssetMenu(fileName = "PlayerWallet", menuName = "Player/Wallet")]
public class PlayerWalletSO : ScriptableObject
{
    [SerializeField] private int coins = 0;

    public int Coins => coins;

    // UI and other systems subscribe to this
    public event Action<int> OnCoinsChanged;

    public bool CanAfford(int amount) => coins >= amount;

    /// <summary>
    /// Syncs wallet coins from GameProgressManager (the real source of truth).
    /// Call this on every scene load so the wallet always matches saved progress.
    /// </summary>
    public void SyncFromProgress()
    {
        if (GameProgressManager.Instance != null)
        {
            coins = GameProgressManager.Instance.GetCoins();
            OnCoinsChanged?.Invoke(coins);
        }
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        // Keep GameProgressManager in sync
        GameProgressManager.Instance?.AddCoins(amount);
        OnCoinsChanged?.Invoke(coins);
    }

    public bool SpendCoins(int amount)
    {
        if (!CanAfford(amount)) return false;

        coins -= amount;
        // Keep GameProgressManager in sync
        GameProgressManager.Instance?.SpendCoins(amount);
        OnCoinsChanged?.Invoke(coins);
        return true;
    }

    public void Reset()
    {
        coins = 0;
        OnCoinsChanged?.Invoke(coins);
    }
}