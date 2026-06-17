using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VIPPanelUI : MonoBehaviour
{
    public static VIPPanelUI Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject _panelRoot;

    [Header("Status")]
    [SerializeField] private TextMeshProUGUI _statusText;

    [Header("Benefits")]
    [SerializeField] private TextMeshProUGUI _benefitsText;

    [Header("Daily Reward")]
    [SerializeField] private Button          _claimDailyButton;
    [SerializeField] private TextMeshProUGUI _dailyRewardText;

    [Header("Close")]
    [SerializeField] private Button _closeButton;

    private void Awake()
    {
        Instance = this;
        if (_claimDailyButton != null)
            _claimDailyButton.onClick.AddListener(OnClaimClicked);
        if (_closeButton != null)
            _closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        GameEvents.OnVIPDailyRewardClaimed += HandleVIPClaimed;
    }

    private void OnDisable()
    {
        GameEvents.OnVIPDailyRewardClaimed -= HandleVIPClaimed;
    }

    public void Open()
    {
        if (_panelRoot != null) _panelRoot.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        if (_panelRoot != null) _panelRoot.SetActive(false);
    }

    private void Refresh()
    {
        if (VIPManager.Instance == null) return;

        bool active = VIPManager.Instance.IsVIPActive;

        if (_statusText != null)
            _statusText.text = active ? "VIP Active" : "VIP Inactive";

        if (_benefitsText != null)
            _benefitsText.text = "- 3x Pass XP\n- Faster Energy Recovery\n- Daily: 20 Gems + 200 Coins + 10 Energy\n- Exclusive Shop Deals";

        if (_claimDailyButton != null)
            _claimDailyButton.interactable = VIPManager.Instance.CanClaimDailyReward;

        if (_dailyRewardText != null)
            _dailyRewardText.text = VIPManager.Instance.CanClaimDailyReward
                ? "Claim Daily Reward"
                : "Already Claimed Today";
    }

    private void OnClaimClicked()
    {
        VIPManager.Instance?.ClaimDailyReward();
    }

    private void HandleVIPClaimed()
    {
        Refresh();
    }
}
