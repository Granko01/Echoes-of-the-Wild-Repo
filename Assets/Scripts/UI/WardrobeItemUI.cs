using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WardrobeItemUI : MonoBehaviour
{
    [SerializeField] private Image           _previewImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _tierText;
    [SerializeField] private Button          _equipButton;
    [SerializeField] private GameObject      _equippedBadge;
    [SerializeField] private GameObject      _lockedOverlay;

    private string _id;
    private bool   _isSkin;

    private void Awake()
    {
        if (_equipButton != null)
            _equipButton.onClick.AddListener(OnEquipClicked);
    }

    public void SetupCostume(CostumeData data)
    {
        _id = data.costumeId;
        _isSkin = false;
        if (_previewImage != null && data.previewSprite != null) _previewImage.sprite = data.previewSprite;
        if (_nameText != null) _nameText.text = data.displayName;
        if (_tierText != null) _tierText.text = data.tier.ToString();
        Refresh();
    }

    public void SetupSkin(CompanionSkinData data)
    {
        _id = data.skinId;
        _isSkin = true;
        if (_previewImage != null && data.previewSprite != null) _previewImage.sprite = data.previewSprite;
        if (_nameText != null) _nameText.text = data.displayName;
        if (_tierText != null) _tierText.text = "";
        Refresh();
    }

    private void Refresh()
    {
        if (CostumeManager.Instance == null) return;

        bool owned = CostumeManager.Instance.IsOwned(_id);
        bool equipped = _isSkin
            ? CostumeManager.Instance.EquippedCompanionSkin == _id
            : CostumeManager.Instance.EquippedCostume == _id;

        if (_lockedOverlay != null) _lockedOverlay.SetActive(!owned);
        if (_equippedBadge != null) _equippedBadge.SetActive(equipped);
        if (_equipButton != null)   _equipButton.interactable = owned && !equipped;
    }

    private void OnEquipClicked()
    {
        if (CostumeManager.Instance == null) return;
        if (_isSkin) CostumeManager.Instance.EquipCompanionSkin(_id);
        else         CostumeManager.Instance.EquipCostume(_id);
    }
}
