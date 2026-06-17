using System;
using System.Globalization;
using UnityEngine;

public class BossRushManager : MonoBehaviour
{
    public static BossRushManager Instance { get; private set; }

    // Ordered boss sequence — mirrors the chapter progression
    private static readonly string[] BossSequence =
    {
        "CaveMawBoss",
        "DistressedMotherBeast",
        "MultiEntityAlpha",
        "MachineMother",
        "SilentHunterBoss",
        "HumanEchoKing",
        "TheEmptyOne",
    };

    public bool   IsActive         { get; private set; }
    public int    CurrentBossIndex { get; private set; }
    public int    SessionScore     { get; private set; }
    public int    TotalMedals      => SaveSystem.Data.bossRushMedals;
    public int    BestScore        => SaveSystem.Data.bossRushBestScore;
    public string NextBossId       => CurrentBossIndex < BossSequence.Length
                                          ? BossSequence[CurrentBossIndex] : "";

    public bool HasCompletedThisWeek
    {
        get
        {
            if (string.IsNullOrEmpty(SaveSystem.Data.bossRushLastCompletedDate)) return false;
            return DateTime.TryParse(SaveSystem.Data.bossRushLastCompletedDate, null,
                       DateTimeStyles.RoundtripKind, out var last)
                   && (DateTime.UtcNow - last).TotalDays < 7;
        }
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()  => GameEvents.OnBossDefeated += HandleBossDefeated;
    private void OnDisable() => GameEvents.OnBossDefeated -= HandleBossDefeated;

    // ── Session Control ───────────────────────────────────────────────────────

    public bool StartBossRush()
    {
        if (IsActive) return false;

        IsActive          = true;
        CurrentBossIndex  = 0;
        SessionScore      = 0;
        GameEvents.RaiseBossRushStarted();
        GameEvents.RaiseBossRushNextBoss(0, BossSequence[0]);
        return true;
    }

    public void AbandonBossRush() => Finalise(completed: false);

    // ── Boss Defeated Hook ────────────────────────────────────────────────────

    private void HandleBossDefeated(string bossId)
    {
        if (!IsActive) return;
        if (CurrentBossIndex >= BossSequence.Length) return;
        if (BossSequence[CurrentBossIndex] != bossId) return;

        SessionScore     += PointsForBoss(CurrentBossIndex);
        CurrentBossIndex++;

        if (CurrentBossIndex >= BossSequence.Length)
            Finalise(completed: true);
        else
            GameEvents.RaiseBossRushNextBoss(CurrentBossIndex, BossSequence[CurrentBossIndex]);
    }

    // ── Finalise ──────────────────────────────────────────────────────────────

    private void Finalise(bool completed)
    {
        if (!IsActive) return;
        IsActive = false;

        if (SessionScore > SaveSystem.Data.bossRushBestScore)
            SaveSystem.Data.bossRushBestScore = SessionScore;

        int medalsEarned = MedalsFromScore(SessionScore);
        SaveSystem.Data.bossRushMedals += medalsEarned;

        if (completed)
            SaveSystem.Data.bossRushLastCompletedDate = DateTime.UtcNow.ToString("O");

        SaveSystem.Save();
        GameEvents.RaiseBossRushEnded(SessionScore, medalsEarned);
        PassManager.Instance?.AddPassXP(medalsEarned * 50);
        WeeklyMissionManager.Instance?.AddRealmRaceScore(SessionScore);
    }

    // ── Scoring ───────────────────────────────────────────────────────────────

    // Later bosses are worth more points
    private static int PointsForBoss(int index) => 10 + (index * 5);

    private static int MedalsFromScore(int score)
    {
        if (score >= 100) return 5;
        if (score >= 70)  return 3;
        if (score >= 40)  return 2;
        if (score >= 10)  return 1;
        return 0;
    }
}
