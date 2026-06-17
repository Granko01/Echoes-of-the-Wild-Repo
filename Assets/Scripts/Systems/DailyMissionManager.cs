using System;
using System.Globalization;
using UnityEngine;

public class DailyMissionManager : MonoBehaviour
{
    public static DailyMissionManager Instance { get; private set; }

    public const int CompletionBonusGems = 50;

    private static readonly MissionDefinition[] Definitions =
    {
        new MissionDefinition { missionId="daily_adventure_3",     displayName="Complete 3 Adventure Levels",  missionType=MissionType.CompleteAdventureLevels, targetCount=3,  gemReward=10 },
        new MissionDefinition { missionId="daily_match3_5",        displayName="Complete 5 Match-3 Levels",    missionType=MissionType.CompleteMatch3Levels,    targetCount=5,  gemReward=10 },
        new MissionDefinition { missionId="daily_weapon_skill_20", displayName="Use Weapon Skills 20 Times",   missionType=MissionType.UseWeaponSkills,         targetCount=20, gemReward=10 },
        new MissionDefinition { missionId="daily_open_chest_3",    displayName="Open 3 Chests",                missionType=MissionType.OpenChests,              targetCount=3,  gemReward=10 },
    };

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()    => CheckDailyReset();

    private void OnEnable()
    {
        GameEvents.OnLevelComplete       += HandleAdventureLevel;
        GameEvents.OnMatch3LevelComplete += HandleMatch3Level;
        GameEvents.OnWeaponSkillUsed     += HandleWeaponSkill;
        GameEvents.OnChestOpened         += HandleChestOpened;
    }

    private void OnDisable()
    {
        GameEvents.OnLevelComplete       -= HandleAdventureLevel;
        GameEvents.OnMatch3LevelComplete -= HandleMatch3Level;
        GameEvents.OnWeaponSkillUsed     -= HandleWeaponSkill;
        GameEvents.OnChestOpened         -= HandleChestOpened;
    }

    // ── Reset ─────────────────────────────────────────────────────────────────

    private void CheckDailyReset()
    {
        var data = SaveSystem.Data.dailyMissions;
        bool needsReset = string.IsNullOrEmpty(data.lastResetDate);

        if (!needsReset &&
            DateTime.TryParse(data.lastResetDate, null, DateTimeStyles.RoundtripKind, out var last))
            needsReset = last.Date < DateTime.UtcNow.Date;

        if (needsReset) InitializeMissions();
    }

    private void InitializeMissions()
    {
        var data = SaveSystem.Data.dailyMissions;
        data.missions.Clear();
        data.completionRewardClaimed = false;
        data.lastResetDate           = DateTime.UtcNow.ToString("O");

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
                if (AreAllCompleted()) GameEvents.RaiseDailyMissionsAllCompleted();
            }
        }
        if (dirty) SaveSystem.Save();
    }

    // ── Claims ────────────────────────────────────────────────────────────────

    public bool ClaimMissionReward(string missionId)
    {
        var def   = GetDefinition(missionId);
        var entry = GetEntry(missionId);
        if (def == null || entry == null || !entry.completed || entry.rewardClaimed) return false;

        entry.rewardClaimed = true;
        CurrencyManager.Instance.AddGems(def.gemReward);
        PassManager.Instance?.AddPassXP(50);
        SaveSystem.Save();
        return true;
    }

    public bool ClaimCompletionBonus()
    {
        if (SaveSystem.Data.dailyMissions.completionRewardClaimed || !AreAllCompleted()) return false;
        SaveSystem.Data.dailyMissions.completionRewardClaimed = true;
        CurrencyManager.Instance.AddGems(CompletionBonusGems);
        PassManager.Instance?.AddPassXP(100);
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
        SaveSystem.Data.dailyMissions.missions.Find(e => e.missionId == id);

    public MissionDefinition GetDefinition(string id)
    {
        foreach (var def in Definitions)
            if (def.missionId == id) return def;
        return null;
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void HandleAdventureLevel()              => AddProgress(MissionType.CompleteAdventureLevels);
    private void HandleMatch3Level()                 => AddProgress(MissionType.CompleteMatch3Levels);
    private void HandleWeaponSkill(WeaponBase _)     => AddProgress(MissionType.UseWeaponSkills);
    private void HandleChestOpened(ChestType _)      => AddProgress(MissionType.OpenChests);
}
