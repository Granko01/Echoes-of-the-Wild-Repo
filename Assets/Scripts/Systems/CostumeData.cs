using UnityEngine;

public enum CostumeTier { Standard, Rare, Epic, Legendary }

[CreateAssetMenu(menuName = "Punch/Costume Data", fileName = "CostumeData_New")]
public class CostumeData : ScriptableObject
{
    public string      costumeId;
    public string      displayName;
    public Sprite      previewSprite;
    public CostumeTier tier;
    public RuntimeAnimatorController animatorOverride; // null = use default animator
}
