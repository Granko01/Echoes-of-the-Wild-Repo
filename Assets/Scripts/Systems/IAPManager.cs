using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static IAPManager Instance { get; private set; }

    // ── Product IDs ───────────────────────────────────────────────────────────
    public static class ProductID
    {
        // Consumables — Bundles
        public const string BabyBundle        = "punch_baby_bundle";

        // Consumables — Gem Packs
        public const string GemsTiny          = "punch_gems_tiny";
        public const string GemsSmall         = "punch_gems_small";
        public const string GemsMedium        = "punch_gems_medium";
        public const string GemsLarge         = "punch_gems_large";

        // Consumables — Booster Packs
        public const string BoostersSmall     = "punch_boosters_small";
        public const string BoostersMedium    = "punch_boosters_medium";
        public const string BoostersLarge     = "punch_boosters_large";

        // Consumables — Passes (one per season/event)
        public const string ChroniclePass     = "punch_chronicle_pass";
        public const string PassPlus          = "punch_pass_plus";
        public const string EventPass         = "punch_event_pass";

        // Non-Consumables — Utility
        public const string RemoveAds         = "punch_remove_ads";

        // Non-Consumables — Launch Costumes
        public const string CostumeExplorer      = "punch_costume_explorer_punch";
        public const string CostumeLanternKeeper = "punch_costume_lantern_keeper";
        public const string CostumeForestGuardian= "punch_costume_forest_guardian";

        // Non-Consumables — Launch Companion Skins
        public const string SkinGoldenDeer    = "punch_skin_golden_baby_deer";
        public const string SkinWinterDeer    = "punch_skin_winter_baby_deer";
        public const string SkinGalaxyMimo    = "punch_skin_galaxy_mimo";

        // Subscription
        public const string VIPMonthly        = "punch_vip_monthly";
    }

    private IStoreController _store;

    public bool IsInitialized => _store != null;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => InitializeStore();

    // ── Initialization ────────────────────────────────────────────────────────

    private void InitializeStore()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Consumables
        builder.AddProduct(ProductID.BabyBundle,         ProductType.Consumable);
        builder.AddProduct(ProductID.GemsTiny,           ProductType.Consumable);
        builder.AddProduct(ProductID.GemsSmall,          ProductType.Consumable);
        builder.AddProduct(ProductID.GemsMedium,         ProductType.Consumable);
        builder.AddProduct(ProductID.GemsLarge,          ProductType.Consumable);
        builder.AddProduct(ProductID.BoostersSmall,      ProductType.Consumable);
        builder.AddProduct(ProductID.BoostersMedium,     ProductType.Consumable);
        builder.AddProduct(ProductID.BoostersLarge,      ProductType.Consumable);
        builder.AddProduct(ProductID.ChroniclePass,      ProductType.Consumable);
        builder.AddProduct(ProductID.PassPlus,           ProductType.Consumable);
        builder.AddProduct(ProductID.EventPass,          ProductType.Consumable);

        // Non-Consumables
        builder.AddProduct(ProductID.RemoveAds,            ProductType.NonConsumable);
        builder.AddProduct(ProductID.CostumeExplorer,      ProductType.NonConsumable);
        builder.AddProduct(ProductID.CostumeLanternKeeper, ProductType.NonConsumable);
        builder.AddProduct(ProductID.CostumeForestGuardian,ProductType.NonConsumable);
        builder.AddProduct(ProductID.SkinGoldenDeer,       ProductType.NonConsumable);
        builder.AddProduct(ProductID.SkinWinterDeer,       ProductType.NonConsumable);
        builder.AddProduct(ProductID.SkinGalaxyMimo,       ProductType.NonConsumable);

        // Subscription
        builder.AddProduct(ProductID.VIPMonthly, ProductType.Subscription);

        UnityPurchasing.Initialize(this, builder);
    }

    // ── Public Purchase API ───────────────────────────────────────────────────

    public void BuyProduct(string productId)
    {
        if (!IsInitialized)
        {
            GameEvents.RaisePurchaseFailed(productId, "Store not ready.");
            return;
        }

        var product = _store.products.WithID(productId);
        if (product != null && product.availableToPurchase)
            _store.InitiatePurchase(product);
        else
            GameEvents.RaisePurchaseFailed(productId, "Product unavailable.");
    }

    // Convenience helpers for common purchases
    public void BuyBabyBundle()    => BuyProduct(ProductID.BabyBundle);
    public void BuyRemoveAds()     => BuyProduct(ProductID.RemoveAds);
    public void BuyVIP()           => BuyProduct(ProductID.VIPMonthly);
    public void BuyChroniclePass() => BuyProduct(ProductID.ChroniclePass);
    public void BuyPassPlus()      => BuyProduct(ProductID.PassPlus);
    public void BuyEventPass()     => BuyProduct(ProductID.EventPass);

    // ── IDetailedStoreListener ────────────────────────────────────────────────

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _store = controller;
        Debug.Log("[IAPManager] Store initialized.");
        RestoreEntitlements();
    }

    public void OnInitializeFailed(InitializationFailureReason error) =>
        Debug.LogError($"[IAPManager] Init failed: {error}");

    public void OnInitializeFailed(InitializationFailureReason error, string message) =>
        Debug.LogError($"[IAPManager] Init failed: {error} — {message}");

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string id = args.purchasedProduct.definition.id;
        GrantPurchase(id);
        GameEvents.RaisePurchaseSuccess(id);
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.LogWarning($"[IAPManager] Purchase failed: {product.definition.id} — {reason}");
        GameEvents.RaisePurchaseFailed(product.definition.id, reason.ToString());
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
    {
        Debug.LogWarning($"[IAPManager] Purchase failed: {product.definition.id} — {description.message}");
        GameEvents.RaisePurchaseFailed(product.definition.id, description.message);
    }

    // ── Grant Rewards ─────────────────────────────────────────────────────────

    private void GrantPurchase(string productId)
    {
        switch (productId)
        {
            // ── Baby Punch Bundle $0.99 ───────────────────────────────────────
            case ProductID.BabyBundle:
                CurrencyManager.Instance.AddGems(200);
                CurrencyManager.Instance.AddCoins(500);
                SaveSystem.Data.bombs       += 5;
                SaveSystem.Data.rockets     += 5;
                SaveSystem.Data.rainbowOrbs += 3;
                SaveSystem.Save();
                CostumeManager.Instance.UnlockCosmetic(ProductID.CostumeExplorer);
                break;

            // ── Gem Packs ─────────────────────────────────────────────────────
            case ProductID.GemsTiny:   CurrencyManager.Instance.AddGems(100);  break;
            case ProductID.GemsSmall:  CurrencyManager.Instance.AddGems(550);  break;
            case ProductID.GemsMedium: CurrencyManager.Instance.AddGems(1200); break;
            case ProductID.GemsLarge:  CurrencyManager.Instance.AddGems(2600); break;

            // ── Booster Packs ─────────────────────────────────────────────────
            case ProductID.BoostersSmall:
                SaveSystem.Data.bombs       += 10;
                SaveSystem.Data.rockets     += 10;
                SaveSystem.Data.rainbowOrbs += 5;
                SaveSystem.Save();
                break;
            case ProductID.BoostersMedium:
                SaveSystem.Data.bombs       += 25;
                SaveSystem.Data.rockets     += 25;
                SaveSystem.Data.rainbowOrbs += 12;
                SaveSystem.Save();
                break;
            case ProductID.BoostersLarge:
                SaveSystem.Data.bombs       += 60;
                SaveSystem.Data.rockets     += 60;
                SaveSystem.Data.rainbowOrbs += 30;
                SaveSystem.Save();
                break;

            // ── Remove Ads $7.99 ──────────────────────────────────────────────
            case ProductID.RemoveAds:
                SaveSystem.Data.adsRemoved = true;
                SaveSystem.Save();
                break;

            // ── VIP Monthly $9.99 ─────────────────────────────────────────────
            case ProductID.VIPMonthly:
                SaveSystem.Data.vipActive     = true;
                SaveSystem.Data.vipExpiryDate = DateTime.UtcNow.AddDays(30).ToString("O");
                SaveSystem.Save();
                break;

            // ── Passes ───────────────────────────────────────────────────────────
            case ProductID.ChroniclePass:
                PassManager.Instance.ActivateChroniclePass(isPassPlus: false);
                break;
            case ProductID.PassPlus:
                PassManager.Instance.ActivateChroniclePass(isPassPlus: true);
                break;
            case ProductID.EventPass:
                MonthlyEventManager.Instance?.ActivateEventPass();
                break;

            // ── Cosmetics ─────────────────────────────────────────────────────
            case ProductID.CostumeExplorer:
            case ProductID.CostumeLanternKeeper:
            case ProductID.CostumeForestGuardian:
            case ProductID.SkinGoldenDeer:
            case ProductID.SkinWinterDeer:
            case ProductID.SkinGalaxyMimo:
                CostumeManager.Instance.UnlockCosmetic(productId);
                break;
        }
    }

    // Re-applies non-consumable entitlements on session start
    // (catches cases where a previous grant was interrupted)
    private void RestoreEntitlements()
    {
        if (_store == null) return;

        var removeAds = _store.products.WithID(ProductID.RemoveAds);
        if (removeAds != null && removeAds.hasReceipt && !SaveSystem.Data.adsRemoved)
        {
            SaveSystem.Data.adsRemoved = true;
            SaveSystem.Save();
        }

        // Cosmetics restore: check receipt presence and unlock if not already owned
        string[] cosmeticIds =
        {
            ProductID.CostumeExplorer, ProductID.CostumeLanternKeeper,
            ProductID.CostumeForestGuardian, ProductID.SkinGoldenDeer,
            ProductID.SkinWinterDeer, ProductID.SkinGalaxyMimo
        };
        foreach (var id in cosmeticIds)
        {
            var p = _store.products.WithID(id);
            if (p != null && p.hasReceipt)
                GrantPurchase(id);
        }
    }
}
