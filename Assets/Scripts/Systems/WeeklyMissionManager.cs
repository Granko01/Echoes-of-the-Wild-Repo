using System;
using System.Globalization;
using UnityEngine;

public class WeeklyMissionManager : MonoBehaviour
{
    public static WeeklyMissionManager Instance { get; private set; }

    private static readonly MissionDefinition[] Definitions =
    {
        new MissionDefinition { missionId="weekly_adventure_20",     displayName="Complete 20 Adventure Levels", missionType=MissionType.CompleteAdventureLevels, targetCount=20  },
        new MissionDefinition { missionId="weekly_match3_40",        displayName="Complete 40 Match-3 Levels",   missionType=MissionType.CompleteMatch3Levels,    targetCount=40  },
        new MissionDefinition { missionId="weekly_weapon_skill_100", displayName="Use Weapon Skills 100 Times",  missionType=MissionType.UseWeaponSkills,         targetCount=100 },
        new MissionDefinition { missionId="weekly_purify_boss_1",    displayName="Purify One Boss",              missionType=MissionType.PurifyBoss,              targetCount=1   },
    };

    public int RealmRaceScore => SaveSystem.Data.realmRaceScore;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => CheckWeeklyReset();

    private void OnEnable()
    {
        GameEvents.OnLevelComplete        += HandleAdventureLevel;
        GameEvents.OnMatch3LevelComplete  += HandleMatch3Level;
        GameEvents.OnWeaponSkillUsed      += HandleWeaponSkill;
        GameEvents.OnPurifyBurstActivated += HandlePurifyBurst;
    }

    private void OnDisable()
    {
        GameEvents.OnLevelComplete        -= HandleAdventureLevel;
        GameEvents.OnMatch3LevelComplete  -= HandleMatch3Level;
        GameEvents.OnWeaponSkillUsed      -= HandleWeaponSkill;
        GameEvents.OnPurifyBurstActivated -= HandlePurifyBurst;
    }

    // ── Reset ─────────────────────────────────────────────────────────────────

    private void CheckWeeklyReset()
    {
        var data = SaveSystem.Data.weeklyMissions;
        bool needsReset = string.IsNullOrEmpty(data.lastResetDate);

        if (!needsReset &&
            DateTime.TryParse(data.lastResetDate, null, DateTimeStyles.RoundtripKind, out var last))
            needsReset = (DateTime.UtcNow - last).TotalDays >= 7;

        if (needsReset) InitializeMissions();
    }

    private void InitializeMissions()
    {
        var data = SaveSystem.Data.weeklyMissions;
        data.missions.Clear();
        data.completionRewardClaimed    = false;
        data.lastResetDate              = DateTime.UtcNow.ToString("O");
        SaveSystem.Data.realmRaceScore  = 0;
        SaveSystem.Data.realmRaceWeekStart = DateTime.UtcNow.ToString("O");

        foreach (var def in Definitions)
            data.missions.Add(new MissionEntry { missionId = def.missionId });

        SaveSystem.Save();
    }

    // ── Progress ──────────────────────────────────────────────────────────────

    public void AddProgress(MissionType type, int amount = 1)
    {
        bool dirty = false;
        foreach (var def in Definitions)
        {
            if (def.missionType != type) continue;
            var entry = GetEntry(def.missionId);
            if (entry == null || entry.completed) continue;

            entry.currentProgress = Mathf.Min(entry.currentProgress + amount, def.targetCount);
            GameEvents.RaiseMissionProgress(def.missionId, entry.currentProgress, def.targetCount);
            dirty = true;

            if (entry.currentProgress >= def.targetCount)
            {
                entry.completed = true;
                GameEvents.RaiseMissionCompleted(def.missionId);
                if (AreAllCompleted()) GameEvents.RaiseWeeklyMissionsAllCompleted();
            }
        }
        if (dirty) SaveSystem.Save();
    }

    public void AddRealmRaceScore(int points)
    {
        SaveSystem.Data.realmRaceScore += points;
        SaveSystem.Save();
    }

    // ── Claims ────────────────────────────────────────────────────────────────

    public bool ClaimCompletionReward()
    {
        if (SaveSystem.Data.weeklyMissions.completionRewardClaimed || !AreAllCompleted()) return false;

        SaveSystem.Data.weeklyMissions.completionRewardClaimed = true;
        ChestManager.Instance?.OpenChest(ChestType.Legendary); // Legendary Chest per doc
        PassManager.Instance?.AddPassXP(300);
        SaveSystem.Save();
        return true;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public bool AreAllCompleted()
    {
        foreach (var def in Definitions)
        {
            var e = GetEntry(def.missionId);
            if (e == null || !e.completed) return false;
        }
        return true;
    }

    public MissionDefinition[] GetDefinitions() => Definitions;

    public MissionEntry GetEntry(string id) =>
        SaveSystem.Data.weeklyMissions.missions.Find(e => e.missionId == id);

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void HandleAdventureLevel()          => AddProgress(MissionType.CompleteAdventureLevels);
    private void HandleMatch3Level()             => AddProgress(MissionType.CompleteMatch3Levels);
    private void HandleWeaponSkill(WeaponBase _) => AddProgress(MissionType.UseWeaponSkills);
    private void HandlePurifyBurst()             => AddProgress(MissionType.PurifyBoss);
}
