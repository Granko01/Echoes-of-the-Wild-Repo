using UnityEngine;

// Attach to the Player root GameObject.
// Reads the equipped costume from SaveSystem and applies the animator override on startup
// and whenever the player equips a new costume.
[RequireComponent(typeof(Animator))]
public class CostumeApplier : MonoBehaviour
{
    [SerializeField] private CostumeDatabase _database;

    private Animator _animator;

    private void Awake() => _animator = GetComponent<Animator>();

    private void Start() => ApplyCostume(SaveSystem.Data.equippedCostume);

    private void OnEnable()  => GameEvents.OnCostumeEquipped += ApplyCostume;
    private void OnDisable() => GameEvents.OnCostumeEquipped -= ApplyCostume;

    private void ApplyCostume(string costumeId)
    {
        if (_database == null || _animator == null) return;

        var data = _database.GetCostume(costumeId);

        // Null override restores the original controller
        _animator.runtimeAnimatorController =
            data?.animatorOverride != null
                ? data.animatorOverride
                : _animator.runtimeAnimatorController;
    }
}
