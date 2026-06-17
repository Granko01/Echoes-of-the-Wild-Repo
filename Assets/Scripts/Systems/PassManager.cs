using System;
using System.Globalization;
using UnityEngine;

public class PassManager : MonoBehaviour
{
    public static PassManager Instance { get; private set; }

    [SerializeField] private PassConfig _chroniclePassConfig;

    public int  ChronicleLevel => SaveSystem.Data.chroniclePass.currentLevel;
    public bool IsPremium      => SaveSystem.Data.chroniclePass.isPremium;

    public bool IsChronicleActive
    {
        get
        {
            string expiry = SaveSystem.Data.chroniclePass.expiryDate;
            if (string.IsNullOrEmpty(expiry)) return false;
            return DateTime.TryParse(expiry, null, DateTimeStyles.RoundtripKind, out var dt)
                   && DateTime.UtcNow < dt;
        }
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Activation ────────────────────────────────────────────────────────────

    // Called by IAPManager after purchase
    public void ActivateChroniclePass(bool isPassPlus)
    {
        var pass = SaveSystem.Data.chroniclePass;
        pass.isPremium  = true;
        pass.expiryDate = DateTime.UtcNow.AddDays(28).ToString("O");

        if (isPassPlus)
        {
            int maxLevel  = _chroniclePassConfig != null ? _chroniclePassConfig.totalLevels - 1 : 49;
            pass.currentLevel = Mathf.Min(pass.currentLevel + 20, maxLevel);
        }

        SaveSystem.Save();
    }

    // ── XP & Levelling ────────────────────────────────────────────────────────

    // Call after mission completions, level clears, boss defeats
    public void AddPassXP(int xp)
    {
        if (!IsChronicleActive) return;

        var pass       = SaveSystem.Data.chroniclePass;
        int maxLevel   = _chroniclePassConfig != null ? _chroniclePassConfig.totalLevels : 50;
        if (pass.currentLevel >= maxLevel) return;

        int xpPerLevel = _chroniclePassConfig != null ? _chroniclePassConfig.xpPerLevel : 100;
        int newLevel   = Mathf.Min((pass.currentLevel * xpPerLevel + xp) / xpPerLevel, maxLevel);

        if (newLevel > pass.currentLevel)
        {
            pass.currentLevel = newLevel;
            SaveSystem.Save();
            GameEvents.RaisePassLevelUp(newLevel);
        }
    }

    // ── Reward Claims ─────────────────────────────────────────────────────────

    public bool ClaimFreeReward(int level)
    {
        var pass = SaveSystem.Data.chroniclePass;
        if (level > pass.currentLevel)            return false;
        if (pass.claimedFreeRewards.Contains(level)) return false;

        pass.claimedFreeRewards.Add(level);
        GrantReward(_chroniclePassConfig?.freeRewards, level);
        SaveSystem.Save();
        GameEvents.RaisePassRewardClaimed(level, false);
        return true;
    }

    public bool ClaimPremiumReward(int level)
    {
        if (!IsPremium) return false;
        var pass = SaveSystem.Data.chroniclePass;
        if (level > pass.currentLevel)                return false;
        if (pass.claimedPremiumRewards.Contains(level)) return false;

        pass.claimedPremiumRewards.Add(level);
        GrantReward(_chroniclePassConfig?.premiumRewards, level);
        SaveSystem.Save();
        GameEvents.RaisePassRewardClaimed(level, true);
        return true;
    }

    public bool IsFreeRewardClaimed(int level)    => SaveSystem.Data.chroniclePass.claimedFreeRewards.Contains(level);
    public bool IsPremiumRewardClaimed(int level) => SaveSystem.Data.chroniclePass.claimedPremiumRewards.Contains(level);

    // ── Internal ──────────────────────────────────────────────────────────────

    private void GrantReward(PassRewardEntry[] rewards, int level)
    {
        if (rewards == null || level >= rewards.Length || rewards[level] == null) return;
        var entry = rewards[level];

        switch (entry.rewardType)
        {
            case PassRewardType.Gems:        CurrencyManager.Instance.AddGems(entry.amount);  break;
            case PassRewardType.Coins:       CurrencyManager.Instance.AddCoins(entry.amount); break;
            case PassRewardType.Energy:      EnergyManager.Instance.AddEnergy(entry.amount);  break;
            case PassRewardType.Boosters:
                SaveSystem.Data.bombs       += entry.amount;
                SaveSystem.Data.rockets     += entry.amount;
                SaveSystem.Data.rainbowOrbs += Mathf.Max(1, entry.amount / 2);
                SaveSystem.Save();
                break;
            case PassRewardType.Costume:
            case PassRewardType.CompanionSkin:
                CostumeManager.Instance?.UnlockCosmetic(entry.itemId);
                break;
        }
    }
}
