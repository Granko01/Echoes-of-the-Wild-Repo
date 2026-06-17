using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject _shopRoot;

    [Header("Tab Buttons — assign in order: Featured, Passes, Gems, Costumes, Companions, Boosters, Premium")]
    [SerializeField] private Button[] _tabButtons;
    [SerializeField] private Color    _tabActiveColor   = Color.white;
    [SerializeField] private Color    _tabInactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("Item Grid")]
    [SerializeField] private Transform  _itemContainer;
    [SerializeField] private ShopItemUI _itemPrefab;

    [Header("Currency Display")]
    [SerializeField] private TextMeshProUGUI _gemsText;
    [SerializeField] private TextMeshProUGUI _coinsText;

    [Header("Close")]
    [SerializeField] private Button _closeButton;

    private readonly List<ShopItemUI> _spawnedItems = new();

    private void Awake()
    {
        if (_closeButton != null)
            _closeButton.onClick.AddListener(() => ShopManager.Instance.Close());

        for (int i = 0; i < _tabButtons.Length; i++)
        {
            int index = i;
            if (_tabButtons[i] != null)
                _tabButtons[i].onClick.AddListener(() => ShopManager.Instance.SelectTab((ShopTab)index));
        }
    }

    private void OnEnable()
    {
        GameEvents.OnShopOpened     += HandleShopOpened;
        GameEvents.OnShopClosed     += HandleShopClosed;
        GameEvents.OnShopTabChanged += HandleTabChanged;
        GameEvents.OnGemsChanged    += HandleGemsChanged;
        GameEvents.OnCoinsChanged   += HandleCoinsChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnShopOpened     -= HandleShopOpened;
        GameEvents.OnShopClosed     -= HandleShopClosed;
        GameEvents.OnShopTabChanged -= HandleTabChanged;
        GameEvents.OnGemsChanged    -= HandleGemsChanged;
        GameEvents.OnCoinsChanged   -= HandleCoinsChanged;
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void HandleShopOpened()
    {
        if (_shopRoot != null) _shopRoot.SetActive(true);
        RefreshCurrencyDisplay();
    }

    private void HandleShopClosed()
    {
        if (_shopRoot != null) _shopRoot.SetActive(false);
    }

    private void HandleTabChanged(ShopTab tab)
    {
        RefreshTabHighlights(tab);
        PopulateItems(tab);
    }

    private void HandleGemsChanged(int amount)
    {
        if (_gemsText != null) _gemsText.text = $"{amount}";
    }

    private void HandleCoinsChanged(int amount)
    {
        if (_coinsText != null) _coinsText.text = $"{amount}";
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void RefreshTabHighlights(ShopTab activeTab)
    {
        for (int i = 0; i < _tabButtons.Length; i++)
        {
            if (_tabButtons[i] == null) continue;
            var colors = _tabButtons[i].colors;
            colors.normalColor    = i == (int)activeTab ? _tabActiveColor : _tabInactiveColor;
            colors.selectedColor  = colors.normalColor;
            _tabButtons[i].colors = colors;
        }
    }

    private void PopulateItems(ShopTab tab)
    {
        foreach (var item in _spawnedItems)
            if (item != null) Destroy(item.gameObject);
        _spawnedItems.Clear();

        if (_itemPrefab == null || _itemContainer == null) return;

        foreach (var data in ShopManager.Instance.GetItemsForTab(tab))
        {
            var itemUI = Instantiate(_itemPrefab, _itemContainer);
            itemUI.Setup(data);
            _spawnedItems.Add(itemUI);
        }
    }

    private void RefreshCurrencyDisplay()
    {
        if (_gemsText  != null) _gemsText.text  = $"{SaveSystem.Data.gems}";
        if (_coinsText != null) _coinsText.text = $"{SaveSystem.Data.coins}";
    }
}
