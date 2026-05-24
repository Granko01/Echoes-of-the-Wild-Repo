using System;
using UnityEngine;

public static class SaveSystem
{
    private const string KeyAct = "CurrentAct";

    public static void Save(Act act, BondSystem bonds)
    {
        PlayerPrefs.SetInt(KeyAct, (int)act);
        foreach (EntityType type in Enum.GetValues(typeof(EntityType)))
        {
            if (type == EntityType.Unknown) continue;
            PlayerPrefs.SetInt($"Bond_{type}",      bonds.GetBondLevel(type));
            PlayerPrefs.SetInt($"HealCount_{type}", bonds.GetHealCount(type));
        }
        PlayerPrefs.Save();
    }

    public static void LoadAct(out Act act)
    {
        act = (Act)PlayerPrefs.GetInt(KeyAct, (int)Act.Prologue);
    }

    public static void LoadBonds(BondSystem bonds)
    {
        foreach (EntityType type in Enum.GetValues(typeof(EntityType)))
        {
            if (type == EntityType.Unknown) continue;
            int level     = PlayerPrefs.GetInt($"Bond_{type}", 0);
            int healCount = PlayerPrefs.GetInt($"HealCount_{type}", 0);
            if (level > 0 || healCount > 0)
                bonds.SetBondData(type, level, healCount);
        }
    }

    public static void DeleteAll() => PlayerPrefs.DeleteAll();
}
