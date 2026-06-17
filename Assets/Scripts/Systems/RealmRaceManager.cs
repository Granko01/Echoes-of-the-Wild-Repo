using System;
using System.Globalization;
using UnityEngine;

public enum RealmRaceRewardTier { Participation, Top30, Top10, Top1 }

public class RealmRaceManager : MonoBehaviour
{
    public static RealmRaceManager Instance { get; private set; }

    // Simulated competition pool size (replaced by real backend later)
    private const int SimulatedPlayerCount = 99;

    public int  PlayerScore   => SaveSystem.Data.realmRaceScore;
    public int  PlayerRank    => CalculateRank();
    public float RankPercentile => CalculatePercentile();

    public bool CanClaimWeeklyReward
    {
        get
        {
            if (string.IsNullOrEmpty(SaveSystem.Data.realmRaceRewardClaimedDate)) return true;
            return DateTime.TryParse(SaveSystem.Data.realmRaceRewardClaimedDate, null,
                       DateTimeStyles.RoundtripKind, out var last)
                   && (DateTime.UtcNow - last).TotalDays >= 7;
        }
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Reward Claiming ───────────────────────────────────────────────────────

    public bool ClaimWeeklyReward()
    {
        if (!CanClaimWeeklyReward) return false;

        var tier = GetRewardTier();
        GrantTierReward(tier);

        SaveSystem.Data.realmRaceRewardClaimedDate = DateTime.UtcNow.ToString("O");
        SaveSystem.Save();
        GameEvents.RaiseRealmRaceRewardClaimed(tier);
        return true;
    }

    public RealmRaceRewardTier GetRewardTier()
    {
        float pct = RankPercentile;
        if (pct <= 1f)  return RealmRaceRewardTier.Top1;
        if (pct <= 10f) return RealmRaceRewardTier.Top10;
        if (pct <= 30f) return RealmRaceRewardTier.Top30;
        return RealmRaceRewardTier.Participation;
    }

    // ── Simulated Leaderboard ─────────────────────────────────────────────────
    // Score distribution is seeded by week number so it changes weekly but stays
    // consistent within a week. Swap GetSimulatedScores() for a server call later.

    private int CalculateRank()
    {
        int player = PlayerScore;
        int ahead  = 0;
        foreach (int s in GetSimulatedScores())
            if (s > player) ahead++;
        return ahead + 1; // rank 1 = best
    }

    private float CalculatePercentile()
    {
        int player = PlayerScore;
        int total  = SimulatedPlayerCount + 1; // include the player
        int ahead  = 0;
        foreach (int s in GetSimulatedScores())
            if (s > player) ahead++;
        return (ahead / (float)total) * 100f;
    }

    private int[] GetSimulatedScores()
    {
        int weekSeed = GetCurrentWeekNumber();
        var rng      = new System.Random(weekSeed);
        var scores   = new int[SimulatedPlayerCount];
        for (int i = 0; i < SimulatedPlayerCount; i++)
        {
            // Quadratic distribution — most players cluster around low-mid scores
            double roll = rng.NextDouble() * rng.NextDouble();
            scores[i]   = (int)(roll * 250);
        }
        return scores;
    }

    private static int GetCurrentWeekNumber()
    {
        var epoch = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (int)(DateTime.UtcNow - epoch).TotalDays / 7;
    }

    // ── Tier Rewards ──────────────────────────────────────────────────────────

    private void GrantTierReward(RealmRaceRewardTier tier)
    {
        switch (tier)
        {
            case RealmRaceRewardTier.Top1:
                CostumeManager.Instance?.UnlockCosmetic("frame_realm_race_legendary");
                CurrencyManager.Instance.AddGems(200);
                break;
            case RealmRaceRewardTier.Top10:
                CostumeManager.Instance?.UnlockCosmetic("frame_realm_race_epic");
                CurrencyManager.Instance.AddGems(100);
                break;
            case RealmRaceRewardTier.Top30:
                CostumeManager.Instance?.UnlockCosmetic("frame_realm_race_rare");
                CurrencyManager.Instance.AddGems(50);
                break;
            case RealmRaceRewardTier.Participation:
                CurrencyManager.Instance.AddGems(50);
                break;
        }
    }
}
