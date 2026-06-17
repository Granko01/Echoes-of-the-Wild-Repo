using UnityEngine;

public class CostumeManager : MonoBehaviour
{
    public static CostumeManager Instance { get; private set; }

    public string EquippedCostume      => SaveSystem.Data.equippedCostume;
    public string EquippedCompanionSkin => SaveSystem.Data.equippedCompanionSkin;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Used by IAPManager, PassManager, and MonthlyEventManager to grant cosmetics
    public void UnlockCosmetic(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        bool isSkin = id.StartsWith("punch_skin_");

        if (isSkin)
        {
            if (SaveSystem.Data.ownedCompanionSkins.Contains(id)) return;
            SaveSystem.Data.ownedCompanionSkins.Add(id);
        }
        else
        {
            if (SaveSystem.Data.ownedCostumes.Contains(id)) return;
            SaveSystem.Data.ownedCostumes.Add(id);
        }

        SaveSystem.Save();
        GameEvents.RaiseCostumeUnlocked(id);
    }

    public bool IsOwned(string id) =>
        SaveSystem.Data.ownedCostumes.Contains(id) ||
        SaveSystem.Data.ownedCompanionSkins.Contains(id);

    public void EquipCostume(string costumeId)
    {
        if (!IsOwned(costumeId)) return;
        SaveSystem.Data.equippedCostume = costumeId;
        SaveSystem.Save();
        GameEvents.RaiseCostumeEquipped(costumeId);
    }

    public void EquipCompanionSkin(string skinId)
    {
        if (!IsOwned(skinId)) return;
        SaveSystem.Data.equippedCompanionSkin = skinId;
        SaveSystem.Save();
        GameEvents.RaiseCompanionSkinEquipped(skinId);
    }
}
