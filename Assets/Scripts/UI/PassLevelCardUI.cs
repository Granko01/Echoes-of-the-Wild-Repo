using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PassLevelCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Image           _freeRewardIcon;
    [SerializeField] private TextMeshProUGUI _freeRewardText;
    [SerializeField] private Button          _claimFreeButton;
    [SerializeField] private GameObject      _freeClaimedBadge;
    [SerializeField] private Image           _premiumRewardIcon;
    [SerializeField] private TextMeshProUGUI _premiumRewardText;
    [SerializeField] private Button          _claimPremiumButton;
    [SerializeField] private GameObject      _premiumClaimedBadge;
    [SerializeField] private GameObject      _lockedOverlay;

    private int _level;

    public void Init(int level)
    {
        _level = level;
        if (_levelText != null) _levelText.text = $"{level + 1}";

        if (_claimFreeButton != null)
            _claimFreeButton.onClick.AddListener(() => {
                PassManager.Instance?.ClaimFreeReward(_level);
                Refresh(PassManager.Instance?.ChronicleLevel ?? 0, PassManager.Instance?.IsPremium ?? false);
            });

        if (_claimPremiumButton != null)
            _claimPremiumButton.onClick.AddListener(() => {
                PassManager.Instance?.ClaimPremiumReward(_level);
                Refresh(PassManager.Instance?.ChronicleLevel ?? 0, PassManager.Instance?.IsPremium ?? false);
            });
    }

    public void Refresh(int currentLevel, bool isPremium)
    {
        bool reached = _level <= currentLevel;
        bool freeClaimed = PassManager.Instance != null && PassManager.Instance.IsFreeRewardClaimed(_level);
        bool premClaimed = PassManager.Instance != null && PassManager.Instance.IsPremiumRewardClaimed(_level);

        if (_lockedOverlay != null) _lockedOverlay.SetActive(!reached);
        if (_freeClaimedBadge != null) _freeClaimedBadge.SetActive(freeClaimed);
        if (_premiumClaimedBadge != null) _premiumClaimedBadge.SetActive(premClaimed);

        if (_claimFreeButton != null)
            _claimFreeButton.interactable = reached && !freeClaimed;
        if (_claimPremiumButton != null)
            _claimPremiumButton.interactable = reached && isPremium && !premClaimed;
    }
}
