using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestOpeningUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject _panelRoot;

    [Header("Display")]
    [SerializeField] private TextMeshProUGUI _chestTypeText;
    [SerializeField] private Image           _chestImage;
    [SerializeField] private TextMeshProUGUI _rewardsText;

    [Header("Collect")]
    [SerializeField] private Button _collectButton;

    private void Awake()
    {
        if (_collectButton != null)
            _collectButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        GameEvents.OnChestOpened += HandleChestOpened;
    }

    private void OnDisable()
    {
        GameEvents.OnChestOpened -= HandleChestOpened;
    }

    private void HandleChestOpened(ChestType type)
    {
        if (_panelRoot != null) _panelRoot.SetActive(true);

        if (_chestTypeText != null)
            _chestTypeText.text = $"{type} Chest";

        if (_rewardsText != null)
        {
            string rewards = type switch
            {
                ChestType.Rare      => "Gems, Coins, Bombs & Rockets",
                ChestType.Epic      => "Gems, Coins, Bombs, Rockets & Rainbow Orbs",
                ChestType.Legendary => "Lots of Gems, Coins, Bombs, Rockets & Rainbow Orbs!",
                _                   => "Rewards!"
            };
            _rewardsText.text = rewards;
        }
    }

    private void Close()
    {
        if (_panelRoot != null) _panelRoot.SetActive(false);
    }
}
