using System;

public enum MissionType
{
    CompleteAdventureLevels,
    CompleteMatch3Levels,
    UseWeaponSkills,
    OpenChests,
    PurifyBoss,
    CollectFragments,
}

[Serializable]
public class MissionDefinition
{
    public string      missionId;
    public string      displayName;
    public MissionType missionType;
    public int         targetCount;
    public int         gemReward;
}
