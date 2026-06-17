using System;
using UnityEngine;

public enum PassRewardType
{
    Gems, Coins, Costume, CompanionSkin,
    Avatar, Frame, Emote, StoryMemory, Boosters, Energy
}

[Serializable]
public class PassRewardEntry
{
    public PassRewardType rewardType;
    public int            amount;
    public string         itemId;      // for cosmetics / named rewards
    public string         displayName;
    public Sprite         icon;
}

[CreateAssetMenu(menuName = "Punch/Pass Config", fileName = "PassConfig_New")]
public class PassConfig : ScriptableObject
{
    public string          passId;
    public string          displayName;
    public int             totalLevels = 50;
    public int             xpPerLevel  = 100;
    public PassRewardEntry[] freeRewards;     // length should equal totalLevels
    public PassRewardEntry[] premiumRewards;  // length should equal totalLevels
}
