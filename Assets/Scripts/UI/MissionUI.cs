using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionUI : MonoBehaviour
{
    public static MissionUI Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject _panelRoot;

    [Header("Tabs")]
    [SerializeField] private Button _dailyTabButton;
    [SerializeField] private Button _weeklyTabButton;
    [SerializeField] private Color  _tabActiveColor   = Color.white;
    [SerializeField] private Color  _tabInactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("Daily Mission Rows")]
    [SerializeField] private GameObject       _dailyContent;
    [SerializeField] private TextMeshProUGUI[] _dailyNameTexts;
    [SerializeField] private Image[]           _dailyProgressFills;
    [SerializeField] private TextMeshProUGUI[] _dailyProgressTexts;
    [SerializeField] private Button[]          _dailyClaimButtons;

    [Header("Daily Completion Bonus")]
    [SerializeField] private Button           _dailyBonusButton;
    [SerializeField] private TextMeshProUGUI  _dailyBonusText;

    [Header("Weekly Mission Rows")]
    [SerializeField] private GameObject       _weeklyContent;
    [SerializeField] private TextMeshProUGUI[] _weeklyNameTexts;
    [SerializeField] private Image[]           _weeklyProgressFills;
    [SerializeField] private TextMeshProUGUI[] _weeklyProgressTexts;
    [SerializeField] private Button[]          _weeklyClaimButtons;

    [Header("Weekly Completion Bonus")]
    [SerializeField] private Button           _weeklyBonusButton;
    [SerializeField] private TextMeshProUGUI  _weeklyBonusText;

    [Header("Close")]
    [SerializeField] private Button _closeButton;

    private bool _showingDaily = true;

    private void Awake()
    {
        Instance = this;
        if (_dailyTabButton != null)  _dailyTabButton.onClick.AddListener(() => SwitchTab(true));
        if (_weeklyTabButton != null) _weeklyTabButton.onClick.AddListener(() => SwitchTab(false));
        if (_closeButton != null)     _closeButton.onClick.AddListener(Close);

        SetupDailyClaimButtons();
        SetupWeeklyClaimButtons();

        if (_dailyBonusButton != null)
            _dailyBonusButton.onClick.AddListener(() => DailyMissionManager.Instance?.ClaimCompletionBonus());
        if (_weeklyBonusButton != null)
            _weeklyBonusButton.onClick.AddListener(() => WeeklyMissionManager.Instance?.ClaimCompletionReward());
    }

    private void OnEnable()
    {
        GameEvents.OnMissionProgress          += HandleProgress;
        GameEvents.OnMissionCompleted         += HandleCompleted;
        GameEvents.OnDailyMissionsAllCompleted  += HandleDailyAllComplete;
        GameEvents.OnWeeklyMissionsAllCompleted += HandleWeeklyAllComplete;
    }

    private void OnDisable()
    {
        GameEvents.OnMissionProgress          -= HandleProgress;
        GameEvents.OnMissionCompleted         -= HandleCompleted;
        GameEvents.OnDailyMissionsAllCompleted  -= HandleDailyAllComplete;
        GameEvents.OnWeeklyMissionsAllCompleted -= HandleWeeklyAllComplete;
    }

    public void Open()
    {
        if (_panelRoot != null) _panelRoot.SetActive(true);
        SwitchTab(true);
        RefreshAll();
    }

    public void Close()
    {
        if (_panelRoot != null) _panelRoot.SetActive(false);
    }

    private void SwitchTab(bool daily)
    {
        _showingDaily = daily;
        if (_dailyContent != null)  _dailyContent.SetActive(daily);
        if (_weeklyContent != null) _weeklyContent.SetActive(!daily);

        SetTabColor(_dailyTabButton,  daily);
        SetTabColor(_weeklyTabButton, !daily);
    }

    private void SetTabColor(Button btn, bool active)
    {
        if (btn == null) return;
        var c = btn.colors;
        c.normalColor   = active ? _tabActiveColor : _tabInactiveColor;
        c.selectedColor = c.normalColor;
        btn.colors = c;
    }

    private void RefreshAll()
    {
        RefreshDaily();
        RefreshWeekly();
    }

    private void RefreshDaily()
    {
        if (DailyMissionManager.Instance == null) return;
        var defs = DailyMissionManager.Instance.GetDefinitions();

        for (int i = 0; i < defs.Length && i < 4; i++)
        {
            var entry = DailyMissionManager.Instance.GetEntry(defs[i].missionId);
            if (_dailyNameTexts != null && i < _dailyNameTexts.Length && _dailyNameTexts[i] != null)
                _dailyNameTexts[i].text = defs[i].displayName;
            if (_dailyProgressTexts != null && i < _dailyProgressTexts.Length && _dailyProgressTexts[i] != null)
                _dailyProgressTexts[i].text = entry != null ? $"{entry.currentProgress}/{defs[i].targetCount}" : $"0/{defs[i].targetCount}";
            if (_dailyProgressFills != null && i < _dailyProgressFills.Length && _dailyProgressFills[i] != null)
                _dailyProgressFills[i].fillAmount = entry != null ? (float)entry.currentProgress / defs[i].targetCount : 0f;
            if (_dailyClaimButtons != null && i < _dailyClaimButtons.Length && _dailyClaimButtons[i] != null)
                _dailyClaimButtons[i].interactable = entry is { completed: true, rewardClaimed: false };
        }

        if (_dailyBonusButton != null)
            _dailyBonusButton.interactable = DailyMissionManager.Instance.AreAllCompleted()
                && !SaveSystem.Data.dailyMissions.completionRewardClaimed;
        if (_dailyBonusText != null)
            _dailyBonusText.text = $"Bonus: {DailyMissionManager.CompletionBonusGems} Gems";
    }

    private void RefreshWeekly()
    {
        if (WeeklyMissionManager.Instance == null) return;
        var defs = WeeklyMissionManager.Instance.GetDefinitions();

        for (int i = 0; i < defs.Length && i < 4; i++)
        {
            var entry = WeeklyMissionManager.Instance.GetEntry(defs[i].missionId);
            if (_weeklyNameTexts != null && i < _weeklyNameTexts.Length && _weeklyNameTexts[i] != null)
                _weeklyNameTexts[i].text = defs[i].displayName;
            if (_weeklyProgressTexts != null && i < _weeklyProgressTexts.Length && _weeklyProgressTexts[i] != null)
                _weeklyProgressTexts[i].text = entry != null ? $"{entry.currentProgress}/{defs[i].targetCount}" : $"0/{defs[i].targetCount}";
            if (_weeklyProgressFills != null && i < _weeklyProgressFills.Length && _weeklyProgressFills[i] != null)
                _weeklyProgressFills[i].fillAmount = entry != null ? (float)entry.currentProgress / defs[i].targetCount : 0f;
            if (_weeklyClaimButtons != null && i < _weeklyClaimButtons.Length && _weeklyClaimButtons[i] != null)
                _weeklyClaimButtons[i].interactable = entry is { completed: true, rewardClaimed: false };
        }

        if (_weeklyBonusButton != null)
            _weeklyBonusButton.interactable = WeeklyMissionManager.Instance.AreAllCompleted()
                && !SaveSystem.Data.weeklyMissions.completionRewardClaimed;
        if (_weeklyBonusText != null)
            _weeklyBonusText.text = "Bonus: Legendary Chest";
    }

    private void SetupDailyClaimButtons()
    {
        if (DailyMissionManager.Instance == null || _dailyClaimButtons == null) return;
        var defs = DailyMissionManager.Instance.GetDefinitions();
        for (int i = 0; i < _dailyClaimButtons.Length && i < defs.Length; i++)
        {
            int idx = i;
            string id = defs[idx].missionId;
            if (_dailyClaimButtons[i] != null)
                _dailyClaimButtons[i].onClick.AddListener(() =>
                {
                    DailyMissionManager.Instance?.ClaimMissionReward(id);
                    RefreshDaily();
                });
        }
    }

    private void SetupWeeklyClaimButtons()
    {
        if (WeeklyMissionManager.Instance == null || _weeklyClaimButtons == null) return;
        var defs = WeeklyMissionManager.Instance.GetDefinitions();
        for (int i = 0; i < _weeklyClaimButtons.Length && i < defs.Length; i++)
        {
            string id = defs[i].missionId;
            if (_weeklyClaimButtons[i] != null)
                _weeklyClaimButtons[i].onClick.AddListener(() =>
                {
                    RefreshWeekly();
                });
        }
    }

    private void HandleProgress(string id, int cur, int target) => RefreshAll();
    private void HandleCompleted(string id) => RefreshAll();
    private void HandleDailyAllComplete()  => RefreshDaily();
    private void HandleWeeklyAllComplete() => RefreshWeekly();
}
