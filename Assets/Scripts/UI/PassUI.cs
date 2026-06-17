using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PassUI : MonoBehaviour
{
    public static PassUI Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject _panelRoot;

    [Header("Level Display")]
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Image           _xpFill;
    [SerializeField] private TextMeshProUGUI _xpText;

    [Header("Status")]
    [SerializeField] private TextMeshProUGUI _statusText;

    [Header("Track Scroll")]
    [SerializeField] private ScrollRect      _trackScroll;
    [SerializeField] private Transform       _trackContent;
    [SerializeField] private PassLevelCardUI _cardPrefab;

    [Header("Close")]
    [SerializeField] private Button _closeButton;

    private PassLevelCardUI[] _cards;

    private void Awake()
    {
        Instance = this;
        if (_closeButton != null)
            _closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        GameEvents.OnPassLevelUp      += HandleLevelUp;
        GameEvents.OnPassRewardClaimed += HandleRewardClaimed;
    }

    private void OnDisable()
    {
        GameEvents.OnPassLevelUp      -= HandleLevelUp;
        GameEvents.OnPassRewardClaimed -= HandleRewardClaimed;
    }

    public void Open()
    {
        if (_panelRoot != null) _panelRoot.SetActive(true);
        BuildTrack();
        RefreshAll();
    }

    public void Close()
    {
        if (_panelRoot != null) _panelRoot.SetActive(false);
    }

    private void BuildTrack()
    {
        if (_cards != null || _cardPrefab == null || _trackContent == null) return;
        if (PassManager.Instance == null) return;

        int total = 50;
        _cards = new PassLevelCardUI[total];
        for (int i = 0; i < total; i++)
        {
            var card = Instantiate(_cardPrefab, _trackContent);
            card.Init(i);
            _cards[i] = card;
        }
    }

    private void RefreshAll()
    {
        if (PassManager.Instance == null) return;

        int level = PassManager.Instance.ChronicleLevel;
        bool premium = PassManager.Instance.IsPremium;
        bool active = PassManager.Instance.IsChronicleActive;

        if (_levelText != null) _levelText.text = $"Level {level}/50";
        if (_statusText != null)
            _statusText.text = !active ? "Pass Inactive" : premium ? "PREMIUM" : "FREE";

        if (_cards != null)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i] != null)
                    _cards[i].Refresh(level, premium);
            }
        }
    }

    private void HandleLevelUp(int level)        => RefreshAll();
    private void HandleRewardClaimed(int l, bool p) => RefreshAll();
}
