using UnityEngine;

// Single ScriptableObject that indexes all CostumeData and CompanionSkinData assets.
// Assign in Inspector on the CostumeApplier and CompanionSkinApplier components.
[CreateAssetMenu(menuName = "Punch/Costume Database", fileName = "CostumeDatabase")]
public class CostumeDatabase : ScriptableObject
{
    public CostumeData[]      costumes;
    public CompanionSkinData[] companionSkins;

    public CostumeData GetCostume(string costumeId)
    {
        if (string.IsNullOrEmpty(costumeId)) return null;
        foreach (var c in costumes)
            if (c != null && c.costumeId == costumeId) return c;
        return null;
    }

    public CompanionSkinData GetSkin(string skinId)
    {
        if (string.IsNullOrEmpty(skinId)) return null;
        foreach (var s in companionSkins)
            if (s != null && s.skinId == skinId) return s;
        return null;
    }
}
