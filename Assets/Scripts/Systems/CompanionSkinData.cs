using UnityEngine;

[CreateAssetMenu(menuName = "Punch/Companion Skin Data", fileName = "CompanionSkinData_New")]
public class CompanionSkinData : ScriptableObject
{
    public string   skinId;
    public string   displayName;
    public Sprite   previewSprite;
    public Sprite[] spriteOverrides; // optional per-state sprite replacements
}
