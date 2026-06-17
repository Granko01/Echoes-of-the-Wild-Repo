using UnityEngine;

// Attach to the DeerCompanion (or any companion) root GameObject.
// Swaps the first sprite in the SpriteRenderer when a companion skin is equipped.
[RequireComponent(typeof(SpriteRenderer))]
public class CompanionSkinApplier : MonoBehaviour
{
    [SerializeField] private CostumeDatabase _database;

    private SpriteRenderer _renderer;
    private Sprite         _defaultSprite;

    private void Awake()
    {
        _renderer      = GetComponent<SpriteRenderer>();
        _defaultSprite = _renderer.sprite;
    }

    private void Start() => ApplySkin(SaveSystem.Data.equippedCompanionSkin);

    private void OnEnable()  => GameEvents.OnCompanionSkinEquipped += ApplySkin;
    private void OnDisable() => GameEvents.OnCompanionSkinEquipped -= ApplySkin;

    private void ApplySkin(string skinId)
    {
        if (_renderer == null) return;

        if (string.IsNullOrEmpty(skinId))
        {
            _renderer.sprite = _defaultSprite;
            return;
        }

        var skin = _database?.GetSkin(skinId);
        _renderer.sprite = (skin != null && skin.spriteOverrides != null && skin.spriteOverrides.Length > 0)
            ? skin.spriteOverrides[0]
            : _defaultSprite;
    }
}
