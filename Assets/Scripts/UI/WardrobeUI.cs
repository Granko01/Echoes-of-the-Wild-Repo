using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WardrobeUI : MonoBehaviour
{
    public static WardrobeUI Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject _panelRoot;

    [Header("Tabs")]
    [SerializeField] private Button _costumesTabButton;
    [SerializeField] private Button _skinsTabButton;
    [SerializeField] private Color  _tabActiveColor   = Color.white;
    [SerializeField] private Color  _tabInactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("Grid")]
    [SerializeField] private Transform      _gridContent;
    [SerializeField] private WardrobeItemUI _itemPrefab;

    [Header("Data")]
    [SerializeField] private CostumeDatabase _costumeDatabase;

    [Header("Close")]
    [SerializeField] private Button _closeButton;

    private bool _showingCostumes = true;

    private void Awake()
    {
        Instance = this;
        if (_costumesTabButton != null) _costumesTabButton.onClick.AddListener(() => SwitchTab(true));
        if (_skinsTabButton != null)    _skinsTabButton.onClick.AddListener(() => SwitchTab(false));
        if (_closeButton != null)       _closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        GameEvents.OnCostumeUnlocked       += HandleUnlock;
        GameEvents.OnCostumeEquipped       += HandleEquip;
        GameEvents.OnCompanionSkinEquipped += HandleEquip;
    }

    private void OnDisable()
    {
        GameEvents.OnCostumeUnlocked       -= HandleUnlock;
        GameEvents.OnCostumeEquipped       -= HandleEquip;
        GameEvents.OnCompanionSkinEquipped -= HandleEquip;
    }

    public void Open()
    {
        if (_panelRoot != null) _panelRoot.SetActive(true);
        SwitchTab(true);
    }

    public void Close()
    {
        if (_panelRoot != null) _panelRoot.SetActive(false);
    }

    private void SwitchTab(bool costumes)
    {
        _showingCostumes = costumes;
        SetTabColor(_costumesTabButton, costumes);
        SetTabColor(_skinsTabButton, !costumes);
        Populate();
    }

    private void SetTabColor(Button btn, bool active)
    {
        if (btn == null) return;
        var c = btn.colors;
        c.normalColor   = active ? _tabActiveColor : _tabInactiveColor;
        c.selectedColor = c.normalColor;
        btn.colors = c;
    }

    private void Populate()
    {
        if (_gridContent == null || _itemPrefab == null || _costumeDatabase == null) return;

        foreach (Transform child in _gridContent)
            Destroy(child.gameObject);

        if (_showingCostumes)
        {
            foreach (var costume in _costumeDatabase.costumes)
            {
                if (costume == null) continue;
                var item = Instantiate(_itemPrefab, _gridContent);
                item.SetupCostume(costume);
            }
        }
        else
        {
            foreach (var skin in _costumeDatabase.companionSkins)
            {
                if (skin == null) continue;
                var item = Instantiate(_itemPrefab, _gridContent);
                item.SetupSkin(skin);
            }
        }
    }

    private void HandleUnlock(string id) => Populate();
    private void HandleEquip(string id)  => Populate();
}
