using System;
using System.Globalization;
using UnityEngine;

public class VIPManager : MonoBehaviour
{
    public static VIPManager Instance { get; private set; }

    public bool IsVIPActive
    {
        get
        {
            if (!SaveSystem.Data.vipActive) return false;
            if (string.IsNullOrEmpty(SaveSystem.Data.vipExpiryDate)) return false;
            return DateTime.TryParse(SaveSystem.Data.vipExpiryDate, null,
                       DateTimeStyles.RoundtripKind, out var dt)
                   && DateTime.UtcNow < dt;
        }
    }

    public bool CanClaimDailyReward
    {
        get
        {
            if (!IsVIPActive) return false;
            if (string.IsNullOrEmpty(SaveSystem.Data.vipLastDailyClaimDate)) return true;
            return DateTime.TryParse(SaveSystem.Data.vipLastDailyClaimDate, null,
                       DateTimeStyles.RoundtripKind, out var last)
                   && last.Date < DateTime.UtcNow.Date;
        }
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => CheckVIPExpiry();

    public void CheckVIPExpiry()
    {
        if (SaveSystem.Data.vipActive && !IsVIPActive)
        {
            SaveSystem.Data.vipActive = false;
            SaveSystem.Save();
        }
    }

    // Daily VIP rewards: 20 Gems + Energy + Coins (from doc)
    public bool ClaimDailyReward()
    {
        if (!CanClaimDailyReward) return false;

        CurrencyManager.Instance.AddGems(20);
        CurrencyManager.Instance.AddCoins(200);
        EnergyManager.Instance.AddEnergy(10);

        SaveSystem.Data.vipLastDailyClaimDate = DateTime.UtcNow.ToString("O");
        SaveSystem.Save();
        GameEvents.RaiseVIPDailyRewardClaimed();
        return true;
    }
}
