using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [SerializeField] private ShopItemData[] _allItems;

    public ShopTab CurrentTab { get; private set; } = ShopTab.Featured;
    public bool    IsOpen     { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Open / Close ──────────────────────────────────────────────────────────

    public void Open(ShopTab tab = ShopTab.Featured)
    {
        IsOpen = true;
        SelectTab(tab);
        GameEvents.RaiseShopOpened();
    }

    public void OpenShop() => Open();

    public void Close()
    {
        IsOpen = false;
        GameEvents.RaiseShopClosed();
    }

    public void SelectTab(ShopTab tab)
    {
        CurrentTab = tab;
        GameEvents.RaiseShopTabChanged(tab);
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public List<ShopItemData> GetItemsForTab(ShopTab tab)
    {
        var result = new List<ShopItemData>();
        foreach (var item in _allItems)
        {
            if (item == null) continue;
            bool matchesTab      = item.tab == tab;
            bool isFeaturedSlot  = tab == ShopTab.Featured && item.isFeatured;
            if (matchesTab || isFeaturedSlot)
                result.Add(item);
        }
        return result;
    }

    public bool CanAfford(ShopItemData item)
    {
        return item.priceType switch
        {
            ShopPriceType.IAP   => true, // store handles affordability
            ShopPriceType.Gems  => CurrencyManager.Instance.Gems  >= item.gemCost,
            ShopPriceType.Coins => CurrencyManager.Instance.Coins >= item.coinCost,
            ShopPriceType.Free  => true,
            _                   => false
        };
    }

    public bool IsOwned(ShopItemData item)
    {
        if (item.priceType != ShopPriceType.IAP) return false;

        switch (item.iapProductId)
        {
            case IAPManager.ProductID.RemoveAds:
                return SaveSystem.Data.adsRemoved;

            case IAPManager.ProductID.VIPMonthly:
                return SaveSystem.Data.vipActive;

            case IAPManager.ProductID.CostumeExplorer:
            case IAPManager.ProductID.CostumeLanternKeeper:
            case IAPManager.ProductID.CostumeForestGuardian:
                return SaveSystem.Data.ownedCostumes.Contains(item.iapProductId);

            case IAPManager.ProductID.SkinGoldenDeer:
            case IAPManager.ProductID.SkinWinterDeer:
            case IAPManager.ProductID.SkinGalaxyMimo:
                return SaveSystem.Data.ownedCompanionSkins.Contains(item.iapProductId);

            default:
                return false;
        }
    }

    // ── Purchase ──────────────────────────────────────────────────────────────

    public void PurchaseItem(ShopItemData item)
    {
        if (item == null || IsOwned(item)) return;

        switch (item.priceType)
        {
            case ShopPriceType.IAP:
                IAPManager.Instance.BuyProduct(item.iapProductId);
                break;

            case ShopPriceType.Gems:
                if (CurrencyManager.Instance.SpendGems(item.gemCost))
                    OnGemPurchaseGranted(item);
                break;

            case ShopPriceType.Coins:
                if (CurrencyManager.Instance.SpendCoins(item.coinCost))
                    OnCoinPurchaseGranted(item);
                break;

            case ShopPriceType.Free:
                OnFreePurchaseGranted(item);
                break;
        }
    }

    private void OnGemPurchaseGranted(ShopItemData item)
    {
        CostumeManager.Instance?.UnlockCosmetic(item.itemId);
    }

    private void OnCoinPurchaseGranted(ShopItemData item)
    {
        Debug.Log($"[ShopManager] Coin purchase granted: {item.displayName}");
    }

    private void OnFreePurchaseGranted(ShopItemData item)
    {
        Debug.Log($"[ShopManager] Free item granted: {item.displayName}");
    }
}
