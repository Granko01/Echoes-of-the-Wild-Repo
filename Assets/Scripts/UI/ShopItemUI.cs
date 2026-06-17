using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image            _icon;
    [SerializeField] private TextMeshProUGUI  _nameText;
    [SerializeField] private TextMeshProUGUI  _descriptionText;
    [SerializeField] private TextMeshProUGUI  _priceText;
    [SerializeField] private Button           _buyButton;
    [SerializeField] private GameObject       _ownedBadge;
    [SerializeField] private GameObject       _cantAffordOverlay;

    private ShopItemData _data;

    private void Awake()
    {
        if (_buyButton != null)
            _buyButton.onClick.AddListener(OnBuyClicked);
    }

    private void OnEnable()
    {
        GameEvents.OnGemsChanged     += HandleCurrencyChanged;
        GameEvents.OnCoinsChanged    += HandleCurrencyChanged;
        GameEvents.OnPurchaseSuccess += HandlePurchaseSuccess;
    }

    private void OnDisable()
    {
        GameEvents.OnGemsChanged     -= HandleCurrencyChanged;
        GameEvents.OnCoinsChanged    -= HandleCurrencyChanged;
        GameEvents.OnPurchaseSuccess -= HandlePurchaseSuccess;
    }

    public void Setup(ShopItemData data)
    {
        _data = data;

        if (_icon != null && data.icon != null) _icon.sprite    = data.icon;
        if (_nameText != null)                  _nameText.text  = data.displayName;
        if (_descriptionText != null)           _descriptionText.text = data.description;
        if (_priceText != null)                 _priceText.text = BuildPriceLabel(data);

        RefreshState();
    }

    private void OnBuyClicked()
    {
        if (_data != null)
            ShopManager.Instance.PurchaseItem(_data);
    }

    private void HandleCurrencyChanged(int _) => RefreshState();

    private void HandlePurchaseSuccess(string productId)
    {
        if (_data != null && _data.iapProductId == productId)
            RefreshState();
    }

    private void RefreshState()
    {
        if (_data == null) return;

        bool owned     = ShopManager.Instance.IsOwned(_data);
        bool canAfford = ShopManager.Instance.CanAfford(_data);

        if (_ownedBadge != null)        _ownedBadge.SetActive(owned);
        if (_cantAffordOverlay != null) _cantAffordOverlay.SetActive(!owned && !canAfford);
        if (_buyButton != null)         _buyButton.interactable = !owned;
    }

    private static string BuildPriceLabel(ShopItemData data)
    {
        if (!string.IsNullOrEmpty(data.priceLabel)) return data.priceLabel;
        return data.priceType switch
        {
            ShopPriceType.Gems  => $"{data.gemCost} Gems",
            ShopPriceType.Coins => $"{data.coinCost} Coins",
            ShopPriceType.Free  => "FREE",
            _                   => ""  // IAP price label comes from the store at runtime
        };
    }
}
