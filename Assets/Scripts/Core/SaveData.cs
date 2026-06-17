using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // Progression
    public int act = 0;

    // Currencies
    public int gems   = 0;
    public int coins  = 0;
    public int leaves = 0;

    // Energy
    public int    energy             = 60;
    public string energyLastRefillTime = "";

    // Ads
    public bool adsRemoved = false;

    // VIP Membership
    public bool   vipActive     = false;
    public string vipExpiryDate = "";

    // Memory Chronicle Pass
    public PassSaveData chroniclePass = new PassSaveData();

    // Cosmetics
    public List<string> ownedCostumes      = new List<string>();
    public string       equippedCostume    = "";
    public List<string> ownedCompanionSkins = new List<string>();
    public string       equippedCompanionSkin = "";

    // Daily Login
    public int    loginStreakDay = 0;
    public string lastLoginDate  = "";

    // Daily & Weekly Missions
    public DailyMissionSaveData  dailyMissions  = new DailyMissionSaveData();
    public WeeklyMissionSaveData weeklyMissions = new WeeklyMissionSaveData();

    // Boosters
    public int bombs       = 0;
    public int rockets     = 0;
    public int rainbowOrbs = 0;

    // Bonds (migrated from PlayerPrefs)
    public List<BondEntry> bonds = new List<BondEntry>();

    // VIP daily reward tracking
    public string vipLastDailyClaimDate = "";

    // Event Pass (separate season track from Chronicle Pass)
    public PassSaveData eventPass = new PassSaveData();

    // Monthly Event
    public string activeEventId         = "";
    public int    eventCurrency         = 0;
    public int    puzzlePiecesCollected = 0;

    // Realm Race (resets weekly)
    public int    realmRaceScore              = 0;
    public string realmRaceWeekStart          = "";
    public string realmRaceRewardClaimedDate  = "";

    // Boss Rush
    public int    bossRushMedals            = 0;
    public int    bossRushBestScore         = 0;
    public string bossRushLastCompletedDate = "";
}

[Serializable]
public class BondEntry
{
    public string entityType;
    public int    bondLevel;
    public int    healCount;
}

[Serializable]
public class PassSaveData
{
    public int        currentLevel          = 0;
    public bool       isPremium             = false;
    public List<int>  claimedFreeRewards    = new List<int>();
    public List<int>  claimedPremiumRewards = new List<int>();
    public string     expiryDate            = "";
}

[Serializable]
public class DailyMissionSaveData
{
    public string            lastResetDate          = "";
    public List<MissionEntry> missions              = new List<MissionEntry>();
    public bool              completionRewardClaimed = false;
}

[Serializable]
public class WeeklyMissionSaveData
{
    public string            lastResetDate          = "";
    public List<MissionEntry> missions              = new List<MissionEntry>();
    public bool              completionRewardClaimed = false;
}

[Serializable]
public class MissionEntry
{
    public string missionId;
    public int    currentProgress;
    public bool   completed;
    public bool   rewardClaimed;
}
