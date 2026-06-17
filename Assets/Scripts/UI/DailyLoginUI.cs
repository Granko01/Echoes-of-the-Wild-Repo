using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyLoginUI : MonoBehaviour
{
    [SerializeField] private GameObject       _popupRoot;
    [SerializeField] private TextMeshProUGUI  _dayText;
    [SerializeField] private TextMeshProUGUI  _rewardText;
    [SerializeField] private Button           _claimButton;
    [SerializeField] private Image[]          _dayIndicators;

    private static readonly string[] DayLabels =
    {
        "200 Coins", "5 Boosters", "20 Energy",
        "10 Materials", "20 Gems", "Rare Chest", "Epic Chest"
    };

    private void Awake()
    {
        if (_claimButton != null)
            _claimButton.onClick.AddListener(OnClaimClicked);
    }

    private void OnEnable()
    {
        GameEvents.OnDailyLoginRewardClaimed += HandleClaimed;
    }

    private void OnDisable()
    {
        GameEvents.OnDailyLoginRewardClaimed -= HandleClaimed;
    }

    private void Start()
    {
        if (DailyLoginManager.Instance != null && DailyLoginManager.Instance.HasPendingReward)
            Show();
        else
            Hide();
    }

    public void Show()
    {
        if (_popupRoot != null) _popupRoot.SetActive(true);

        int day = DailyLoginManager.Instance != null ? DailyLoginManager.Instance.CurrentStreakDay : 0;

        if (_dayText != null) _dayText.text = $"Day {day + 1}";
        if (_rewardText != null && day < DayLabels.Length) _rewardText.text = DayLabels[day];
        if (_claimButton != null) _claimButton.interactable = true;

        if (_dayIndicators != null)
        {
            for (int i = 0; i < _dayIndicators.Length; i++)
            {
                if (_dayIndicators[i] != null)
                    _dayIndicators[i].color = i <= day ? Color.white : new Color(1, 1, 1, 0.3f);
            }
        }
    }

    public void Hide()
    {
        if (_popupRoot != null) _popupRoot.SetActive(false);
    }

    private void OnClaimClicked()
    {
        DailyLoginManager.Instance?.ClaimDailyReward();
    }

    private void HandleClaimed(int day)
    {
        if (_claimButton != null) _claimButton.interactable = false;
        if (_rewardText != null) _rewardText.text = "Claimed!";
        Invoke(nameof(Hide), 1.5f);
    }
}
