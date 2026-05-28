using System.Collections;
using UnityEngine;

// Chapter 2 weapon — Spiral Nunchaku
// Base  : 3-hit combo
// Lv2   : Vine Spin (area attack around player)
// Lv3   : Air Spiral (extended aerial combo, +1 hit in air)
// MAX   : Forest Cyclone (massive spinning finisher)
public class SpiralNunchaku : WeaponBase
{
    [Header("Combo")]
    [SerializeField] private float _comboHitRadius    = 1.2f;
    [SerializeField] private float _comboHitForward   = 1.0f;
    [SerializeField] private int   _baseComboHits     = 3;

    [Header("Vine Spin — Lv2")]
    [SerializeField] private float _vineSpinRadius    = 2.5f;
    [SerializeField] private float _vineSpinDuration  = 0.6f;

    [Header("Forest Cyclone — MAX")]
    [SerializeField] private float _cycloneRadius     = 4f;
    [SerializeField] private float _cycloneDuration   = 1.2f;

    [Header("Audio")]
    [SerializeField] private AudioClip _swingClip;
    [SerializeField] private AudioClip _spinClip;
    [SerializeField] private AudioClip _cycloneClip;

    private int   _comboStep;
    private float _comboResetTimer;
    private const float ComboResetTime = 1.0f;

    private PlayerController _pc;

    private void Start() => _pc = GetComponentInParent<PlayerController>();

    protected override void Update()
    {
        base.Update();
        if (_comboResetTimer > 0)
        {
            _comboResetTimer -= Time.deltaTime;
            if (_comboResetTimer <= 0) _comboStep = 0;
        }
    }

    protected override void Attack(Vector2 direction)
    {
        _comboResetTimer = ComboResetTime;

        int maxCombo = _baseComboHits + (Level >= 2 && (_pc == null || !_pc.IsGrounded) ? 1 : 0);
        _comboStep = (_comboStep % maxCombo) + 1;

        AudioManager.Instance?.PlaySFX(_swingClip);

        Vector2 center = (Vector2)transform.position + direction.normalized * _comboHitForward;
        DealAoeDamage(center, _comboHitRadius);
        ComboMeter.Instance?.RegisterHit();
    }

    protected override void WeaponSkill(Vector2 direction)
    {
        switch (Level)
        {
            case 1: StartCoroutine(VineSpinRoutine());   break;
            case 2: StartCoroutine(VineSpinRoutine());   break;
            case 3: StartCoroutine(ForestCycloneRoutine()); break;
        }
    }

    private IEnumerator VineSpinRoutine()
    {
        AudioManager.Instance?.PlaySFX(_spinClip);
        float elapsed = 0f;
        while (elapsed < _vineSpinDuration)
        {
            DealAoeDamage(transform.position, _vineSpinRadius);
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ForestCycloneRoutine()
    {
        AudioManager.Instance?.PlaySFX(_cycloneClip);
        float elapsed = 0f;
        while (elapsed < _cycloneDuration)
        {
            DealAoeDamage(transform.position, _cycloneRadius);
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
