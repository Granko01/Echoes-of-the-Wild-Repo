using System;
using System.Globalization;
using UnityEngine;

public class DailyLoginManager : MonoBehaviour
{
    public static DailyLoginManager Instance { get; private set; }

    // 7-day cycle from the doc (index 0-6)
    private static readonly (string label, int amount)[] DayRewards =
    {
        ("Coins",            200),  // Day 1
        ("Boosters",           5),  // Day 2
        ("Energy",            20),  // Day 3
        ("WeaponMaterials",   10),  // Day 4
        ("Gems",              20),  // Day 5
        ("RareChest",          1),  // Day 6
        ("EpicChest",          1),  // Day 7
    };

    public int CurrentStreakDay => SaveSystem.Data.loginStreakDay;

    // True if player has not yet claimed today's reward
    public bool HasPendingReward => !HasClaimedToday;

    private bool HasClaimedToday
    {
        get
        {
            if (string.IsNullOrEmpty(SaveSystem.Data.lastLoginDate)) return false;
            return DateTime.TryParse(SaveSystem.Data.lastLoginDate, null,
                       DateTimeStyles.RoundtripKind, out var last)
                   && last.Date == DateTime.UtcNow.Date;
        }
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => CheckStreakReset();

    // Resets streak if more than 1 day has passed — called before UI decides whether to show popup
    private void CheckStreakReset()
    {
        if (HasClaimedToday || string.IsNullOrEmpty(SaveSystem.Data.lastLoginDate)) return;

        if (DateTime.TryParse(SaveSystem.Data.lastLoginDate, null,
                DateTimeStyles.RoundtripKind, out var last))
        {
            double daysMissed = (DateTime.UtcNow.Date - last.Date).TotalDays;
            if (daysMissed > 1)
            {
                SaveSystem.Data.loginStreakDay = 0;
                SaveSystem.Save();
            }
        }
    }

    // Called by UI when player taps the login reward popup
    public bool ClaimDailyReward()
    {
        if (!HasPendingReward) return false;

        int day = SaveSystem.Data.loginStreakDay;
        GrantDayReward(day);

        SaveSystem.Data.loginStreakDay = (day + 1) % DayRewards.Length;
        SaveSystem.Data.lastLoginDate  = DateTime.UtcNow.ToString("O");
        SaveSystem.Save();

        GameEvents.RaiseDailyLoginRewardClaimed(day);
        PassManager.Instance?.AddPassXP(30);
        return true;
    }

    private void GrantDayReward(int dayIndex)
    {
        var (label, amount) = DayRewards[dayIndex];
        switch (label)
        {
            case "Coins":           CurrencyManager.Instance.AddCoins(amount);  break;
            case "Gems":            CurrencyManager.Instance.AddGems(amount);   break;
            case "Energy":          EnergyManager.Instance.AddEnergy(amount);   break;
            case "WeaponMaterials": CurrencyManager.Instance.AddCoins(amount * 10); break;
            case "Boosters":
                SaveSystem.Data.bombs       += amount;
                SaveSystem.Data.rockets     += amount;
                SaveSystem.Data.rainbowOrbs += Mathf.Max(1, amount / 2);
                SaveSystem.Save();
                break;
            case "RareChest":   ChestManager.Instance?.OpenChest(ChestType.Rare);      break;
            case "EpicChest":   ChestManager.Instance?.OpenChest(ChestType.Epic);      break;
        }
    }
}
