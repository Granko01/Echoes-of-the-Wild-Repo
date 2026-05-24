using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("Currency")]
    [SerializeField] private TextMeshProUGUI _leavesText;

    [Header("Bond Indicators — one Image per EntityType")]
    [SerializeField] private Image _deerBondFill;
    [SerializeField] private Image _elephantBondFill;

    [Header("Ability State")]
    [SerializeField] private GameObject _suppressionOverlay;
    [SerializeField] private GameObject _focusCalmIndicator;
    [SerializeField] private GameObject _burstReadyIndicator;

    private int _leaves;

    private void OnEnable()
    {
        GameEvents.OnBondLevelUp  += HandleBondLevelUp;
        GameEvents.OnHealComplete += HandleHealComplete;
        GameEvents.OnStateChange  += HandleStateChange;
    }

    private void OnDisable()
    {
        GameEvents.OnBondLevelUp  -= HandleBondLevelUp;
        GameEvents.OnHealComplete -= HandleHealComplete;
        GameEvents.OnStateChange  -= HandleStateChange;
    }

    public void AddLeaves(int amount)
    {
        _leaves += amount;
        if (_leavesText != null) _leavesText.text = $"Leaves : x{_leaves}";
    }

    public void SetSuppressionOverlay(bool active)
    {
        if (_suppressionOverlay != null) _suppressionOverlay.SetActive(active);
    }

    private void HandleBondLevelUp(EntityType type, int level)
    {
        float fill = level / 3f;
        switch (type)
        {
            case EntityType.Deer:     if (_deerBondFill != null)     _deerBondFill.fillAmount     = fill; break;
            case EntityType.Elephant: if (_elephantBondFill != null) _elephantBondFill.fillAmount = fill; break;
        }
    }

    private void HandleHealComplete(BiomeArea area)
    {
        AddLeaves(10);
    }

    private void HandleStateChange(EntityController entity, EntityState state)
    {
        // Future: per-entity health bar updates
    }
}
