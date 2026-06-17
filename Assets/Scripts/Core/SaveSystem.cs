using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    private static SaveData _data;

    public static SaveData Data
    {
        get
        {
            if (_data == null) Load();
            return _data;
        }
    }

    // ── Core IO ───────────────────────────────────────────────────────────────

    public static void Save()
    {
        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(_data, prettyPrint: true));
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Save failed: {e.Message}");
        }
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(SavePath))
                _data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveSystem] Load failed, starting fresh: {e.Message}");
        }

        if (_data == null)
            _data = new SaveData();
    }

    public static void DeleteAll()
    {
        _data = new SaveData();
        if (File.Exists(SavePath))
            File.Delete(SavePath);
        PlayerPrefs.DeleteAll();
    }

    // ── Legacy API — GameManager + BondSystem callers unchanged ──────────────

    public static void Save(Act act, BondSystem bonds)
    {
        Data.act = (int)act;
        Data.bonds.Clear();
        foreach (EntityType type in Enum.GetValues(typeof(EntityType)))
        {
            if (type == EntityType.Unknown) continue;
            Data.bonds.Add(new BondEntry
            {
                entityType = type.ToString(),
                bondLevel  = bonds.GetBondLevel(type),
                healCount  = bonds.GetHealCount(type)
            });
        }
        Save();
    }

    public static void LoadAct(out Act act)
    {
        // Migrate from PlayerPrefs on first run after upgrade
        if (!File.Exists(SavePath) && PlayerPrefs.HasKey("CurrentAct"))
            MigrateFromPlayerPrefs();

        act = (Act)Data.act;
    }

    public static void LoadBonds(BondSystem bonds)
    {
        foreach (var entry in Data.bonds)
        {
            if (Enum.TryParse<EntityType>(entry.entityType, out var type) && type != EntityType.Unknown)
                bonds.SetBondData(type, entry.bondLevel, entry.healCount);
        }
    }

    // ── Migration ─────────────────────────────────────────────────────────────

    private static void MigrateFromPlayerPrefs()
    {
        Data.act = PlayerPrefs.GetInt("CurrentAct", 0);
        foreach (EntityType type in Enum.GetValues(typeof(EntityType)))
        {
            if (type == EntityType.Unknown) continue;
            int level     = PlayerPrefs.GetInt($"Bond_{type}", 0);
            int healCount = PlayerPrefs.GetInt($"HealCount_{type}", 0);
            if (level > 0 || healCount > 0)
                Data.bonds.Add(new BondEntry
                {
                    entityType = type.ToString(),
                    bondLevel  = level,
                    healCount  = healCount
                });
        }
        Save();
        PlayerPrefs.DeleteAll();
        Debug.Log("[SaveSystem] Migrated from PlayerPrefs to JSON.");
    }
}
