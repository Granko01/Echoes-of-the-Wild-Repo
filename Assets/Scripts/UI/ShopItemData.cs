using UnityEngine;

public enum ShopTab       { Featured, Passes, Gems, Costumes, Companions, Boosters, Premium }
public enum ShopPriceType { IAP, Gems, Coins, Free }

[CreateAssetMenu(menuName = "Punch/Shop Item", fileName = "ShopItem_New")]
public class ShopItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemId;
    public string displayName;
    [TextArea(1, 3)]
    public string description;
    public Sprite icon;

    [Header("Tab")]
    public ShopTab tab;
    public bool    isFeatured; // also appears in Featured tab if true

    [Header("Price")]
    public ShopPriceType priceType;
    public string        iapProductId; // used when priceType == IAP
    public int           gemCost;      // used when priceType == Gems
    public int           coinCost;     // used when priceType == Coins

    [Header("Display Override")]
    [Tooltip("Leave empty to auto-format. Set to override, e.g. '$0.99' or 'BEST VALUE'")]
    public string priceLabel;
}
