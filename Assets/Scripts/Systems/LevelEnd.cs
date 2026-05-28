using UnityEngine;

// Place one LevelEnd in each chapter scene. Set the two counts to match how many
// mini-boss defeats + biome heals are required to finish the level.
// When both targets are met, GameEvents.OnLevelComplete fires — wire a UI panel or
// scene-load to that event.
public class LevelEnd : MonoBehaviour
{
    [Header("Requirements")]
    [Tooltip("How many mini-bosses must be defeated in this level.")]
    [SerializeField] private int _requiredMiniBossDefeats = 1;

    [Tooltip("How many BiomeAreas must be fully healed in this level.")]
    [SerializeField] private int _requiredBiomeHeals = 1;

    [Header("Completion UI (optional)")]
    [SerializeField] private GameObject _levelCompletePanel;

    private int  _miniBossDefeats;
    private int  _biomeHeals;
    private bool _completed;

    private void OnEnable()
    {
        GameEvents.OnMiniBossDefeated += HandleMiniBossDefeated;
        GameEvents.OnHealComplete     += HandleBiomeHealed;
    }

    private void OnDisable()
    {
        GameEvents.OnMiniBossDefeated -= HandleMiniBossDefeated;
        GameEvents.OnHealComplete     -= HandleBiomeHealed;
    }

    private void HandleMiniBossDefeated(MiniBossType _)
    {
        _miniBossDefeats++;
        CheckComplete();
    }

    private void HandleBiomeHealed(BiomeArea _)
    {
        _biomeHeals++;
        CheckComplete();
    }

    private void CheckComplete()
    {
        if (_completed) return;
        if (_miniBossDefeats < _requiredMiniBossDefeats) return;
        if (_biomeHeals     < _requiredBiomeHeals)      return;

        _completed = true;
        GameEvents.RaiseLevelComplete();

        if (_levelCompletePanel != null)
            _levelCompletePanel.SetActive(true);
    }
}
