using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    [SerializeField] private float     _pulseRadius           = 4f;
    [SerializeField] private float     _burstRadius           = 6f;
    [SerializeField] private float     _focusCalmInterval     = 0.4f;
    [SerializeField] private LayerMask _entityLayer;
    [SerializeField] private GameObject _pulseVFXPrefab;
    [SerializeField] private GameObject _burstVFXPrefab;

    private bool _isSuppressed;
    private bool _focusCalmActive;
    private int  _spiritAssistCharges;
    private Coroutine _focusCalmRoutine;

    // Bonus radius from Elephant bond (read each pulse)
    private float PulseRadius => _pulseRadius + (BondSystem.Instance?.GetPulseRadiusBonus() ?? 0f);

    public bool IsSuppressed => _isSuppressed;

    public void UsePulseWave()
    {
        if (_isSuppressed) return;
        if (Match3Manager.Instance != null && Match3Manager.Instance.IsOpen) return;

        float r    = PulseRadius;
        var   hits = Physics2D.OverlapCircleAll(transform.position, r, _entityLayer);
        var   targets = new List<Vector3>();
        foreach (var hit in hits)
            if (hit.TryGetComponent<EntityController>(out var entity))
            {
                entity.OnPulseHit();
                targets.Add(entity.transform.position);
            }

        PulseWaveEffect.Spawn(transform.position, r, targets.ToArray());
        SpawnVFX(_pulseVFXPrefab, r);
    }

    public void SetFocusCalm(bool active)
    {
        if (_isSuppressed && active) return;
        _focusCalmActive = active;

        if (active && _focusCalmRoutine == null)
            _focusCalmRoutine = StartCoroutine(FocusCalmRoutine());
        else if (!active && _focusCalmRoutine != null)
        {
            StopCoroutine(_focusCalmRoutine);
            _focusCalmRoutine = null;
        }
    }

    // Emotional Burst: charged AoE that converts Empty → Receptive
    public void UseEmotionalBurst()
    {
        if (_isSuppressed) return;
        SpawnVFX(_burstVFXPrefab, _burstRadius);
        HitEntities(_burstRadius, e => e.OnEmotionalBurst());
    }

    public void UseSpiritAssist()
    {
        if (_spiritAssistCharges <= 0) return;
        _spiritAssistCharges--;
        BondSystem.Instance?.ActivateAssist(transform.position);
    }

    // Called by SuppressionField trigger
    public void SetSuppressed(bool suppressed)
    {
        _isSuppressed = suppressed;
        if (suppressed) SetFocusCalm(false);
    }

    // Resonance Persistence: brief ability window inside suppression (Act 4 unlock)
    public void UseResonancePersistence()
    {
        if (!_isSuppressed) return;
        StartCoroutine(TemporaryUnsuppress(2f));
    }

    public void AddSpiritAssistCharge(int count = 1) => _spiritAssistCharges += count;

    private IEnumerator FocusCalmRoutine()
    {
        while (_focusCalmActive)
        {
            HitEntities(PulseRadius * 0.75f, e => e.OnPulseHit());
            yield return new WaitForSeconds(_focusCalmInterval);
        }
        _focusCalmRoutine = null;
    }

    private IEnumerator TemporaryUnsuppress(float duration)
    {
        _isSuppressed = false;
        yield return new WaitForSeconds(duration);
        _isSuppressed = true;
    }

    private void HitEntities(float radius, System.Action<EntityController> action)
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, radius, _entityLayer);
        foreach (var hit in hits)
            if (hit.TryGetComponent<EntityController>(out var entity))
                action(entity);
    }

    private void SpawnVFX(GameObject prefab, float radius)
    {
        if (prefab == null) return;
        var vfx = Instantiate(prefab, transform.position, Quaternion.identity);
        vfx.transform.localScale = Vector3.one * (radius / _pulseRadius);
        Destroy(vfx, 2f);
    }
}
