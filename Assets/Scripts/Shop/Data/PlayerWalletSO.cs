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
    
    public void ResetWallet(int startingCoins = 200)
    {
        coins = startingCoins;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
    }

    public bool SpendCoins(int amount)
    {
        if (!CanAfford(amount)) return false;

        coins -= amount;
        OnCoinsChanged?.Invoke(coins);
        return true;
    }

    /// <summary>
    /// Call this at game start to reset between play sessions.
    /// ScriptableObject values persist in the editor.
    /// </summary>
    public void Reset(int startingCoins = 0)
    {
        coins = startingCoins;
        OnCoinsChanged?.Invoke(coins);
    }
}