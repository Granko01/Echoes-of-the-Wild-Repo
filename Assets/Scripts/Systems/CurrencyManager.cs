using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public int Gems   => SaveSystem.Data.gems;
    public int Coins  => SaveSystem.Data.coins;
    public int Leaves => SaveSystem.Data.leaves;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Broadcast initial balances so HUD is correct on scene load
        GameEvents.RaiseGemsChanged(Gems);
        GameEvents.RaiseCoinsChanged(Coins);
        GameEvents.RaiseLeavesChanged(Leaves);
    }

    // ── Gems ──────────────────────────────────────────────────────────────────

    public void AddGems(int amount)
    {
        if (amount <= 0) return;
        SaveSystem.Data.gems += amount;
        SaveSystem.Save();
        GameEvents.RaiseGemsChanged(SaveSystem.Data.gems);
    }

    public bool SpendGems(int amount)
    {
        if (SaveSystem.Data.gems < amount) return false;
        SaveSystem.Data.gems -= amount;
        SaveSystem.Save();
        GameEvents.RaiseGemsChanged(SaveSystem.Data.gems);
        return true;
    }

    // ── Coins ─────────────────────────────────────────────────────────────────

    // Pass applyEventMultiplier:true for in-game drops (enemies, collectibles, level end).
    // Leave false for IAP grants, chest rewards, and mission rewards.
    public void AddCoins(int amount, bool applyEventMultiplier = false)
    {
        if (amount <= 0) return;
        if (applyEventMultiplier && MiniEventManager.Instance != null)
            amount = Mathf.RoundToInt(amount * MiniEventManager.Instance.CoinMultiplier);
        SaveSystem.Data.coins += amount;
        SaveSystem.Save();
        GameEvents.RaiseCoinsChanged(SaveSystem.Data.coins);
    }

    public bool SpendCoins(int amount)
    {
        if (SaveSystem.Data.coins < amount) return false;
        SaveSystem.Data.coins -= amount;
        SaveSystem.Save();
        GameEvents.RaiseCoinsChanged(SaveSystem.Data.coins);
        return true;
    }

    // ── Leaves (in-game collectible / weapon upgrade material) ───────────────

    public void AddLeaves(int amount, bool applyEventMultiplier = false)
    {
        if (amount <= 0) return;
        if (applyEventMultiplier && MiniEventManager.Instance != null)
            amount = Mathf.RoundToInt(amount * MiniEventManager.Instance.MaterialMultiplier);
        SaveSystem.Data.leaves += amount;
        SaveSystem.Save();
        GameEvents.RaiseLeavesChanged(SaveSystem.Data.leaves);
    }

    public bool SpendLeaves(int amount)
    {
        if (SaveSystem.Data.leaves < amount) return false;
        SaveSystem.Data.leaves -= amount;
        SaveSystem.Save();
        GameEvents.RaiseLeavesChanged(SaveSystem.Data.leaves);
        return true;
    }
}
