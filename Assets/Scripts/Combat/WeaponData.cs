using UnityEngine;

[CreateAssetMenu(menuName = "EotW/Weapon Data", fileName = "WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponId      = "echo_staff";
    public string displayName   = "Echo Staff";
    [Range(0, 3)]
    public int    currentLevel  = 0;   // 0=Base 1=Lv2 2=Lv3 3=MAX

    [Header("Upgrade Costs (Echo Fragments)")]
    public int costToLevel2 = 5;
    public int costToLevel3 = 10;
    public int costToMax    = 20;

    [Header("Per-level Skill Names (display only)")]
    public string baseSkillName  = "Basic Attack";
    public string level2SkillName = "Skill Lv2";
    public string level3SkillName = "Skill Lv3";
    public string maxSkillName   = "MAX Skill";

    public int UpgradeCostForNext()
    {
        return currentLevel switch
        {
            0 => costToLevel2,
            1 => costToLevel3,
            2 => costToMax,
            _ => int.MaxValue
        };
    }

    public string ActiveSkillName()
    {
        return currentLevel switch
        {
            0 => baseSkillName,
            1 => level2SkillName,
            2 => level3SkillName,
            _ => maxSkillName
        };
    }
}
