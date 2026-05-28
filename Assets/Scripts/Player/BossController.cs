using System.Collections;
using UnityEngine;

// Attach to the boss entity alongside EntityController + EntityStateMachine.
// Activates automatically when the player walks within range.
// Attack pattern: alternates between Dark Pulse (AoE wave) and a charge slam.
public class BossController : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField] private float _activationRange = 10f;

    [Header("Dark Pulse — main attack")]
    [SerializeField] private float _pulseInterval  = 3f;   // seconds between pulses
    [SerializeField] private float _pulseRadius    = 5f;   // damage radius
    [SerializeField] private int   _pulseDamage    = 1;

    [Header("Charge — secondary attack (every N pulses)")]
    [SerializeField] private int   _chargeEveryNPulses = 2; // charge after every 2nd pulse
    [SerializeField] private float _chargeInterval     = 5f;

    [Header("Suppression Field (optional)")]
    [SerializeField] private float           _suppressInterval = 12f;
    [SerializeField] private float           _suppressDuration =  4f;
    [SerializeField] private SuppressionField _suppressionField;

    [Header("Visual Feedback")]
    [SerializeField] private AudioClip _pulseClip;

    private EntityController   _entity;
    private EntityStateMachine _sm;
    private Transform          _player;
    private bool               _active;

    private void Awake()
    {
        _entity = GetComponent<EntityController>();
        _sm     = GetComponent<EntityStateMachine>();
    }

    private void Update()
    {
        if (_active) return;
        if (_player == null)
            _player = Object.FindFirstObjectByType<PlayerController>()?.transform;
        if (_player != null &&
            Vector2.Distance(transform.position, _player.position) < _activationRange)
            Activate();
    }

    private void Activate()
    {
        _active = true;
        StartCoroutine(AttackLoop());
        if (_suppressionField != null)
            StartCoroutine(SuppressionLoop());
    }

    private IEnumerator AttackLoop()
    {
        int pulseCount = 0;

        // Fire immediately on activation
        DarkPulse();

        while (_sm != null && _sm.Current != EntityState.Stable)
        {
            // Interval shortens as the boss is closer to Stable (getting desperate)
            float wait = _sm.Current switch
            {
                EntityState.Turbulent  => _pulseInterval,
                EntityState.Agitated   => _pulseInterval * 0.8f,
                EntityState.Distressed => _pulseInterval * 0.65f,
                _                      => _pulseInterval * 0.5f
            };

            yield return new WaitForSeconds(wait);
            if (_sm.Current == EntityState.Stable || _sm.Current == EntityState.Empty) break;

            pulseCount++;

            // Every N pulses: charge instead
            if (pulseCount % _chargeEveryNPulses == 0 && _player != null && _entity != null)
                _entity.TriggerCharge(_player);
            else
                DarkPulse();
        }

        if (_suppressionField != null)
            _suppressionField.gameObject.SetActive(false);
    }

    private void DarkPulse()
    {
        if (_player == null) return;

        // Spawn the visible expanding red ring — no assets needed
        DarkPulseEffect.Spawn(transform.position, _pulseRadius);

        AudioManager.Instance?.PlaySFX(_pulseClip);

        // Damage player if inside the ring radius
        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist <= _pulseRadius)
            _player.GetComponent<PlayerHealth>()?.TakeDamage(_pulseDamage);
    }

    private IEnumerator SuppressionLoop()
    {
        while (_sm != null && _sm.Current != EntityState.Stable)
        {
            yield return new WaitForSeconds(_suppressInterval);
            if (_suppressionField == null || _sm.Current == EntityState.Stable) break;
            _suppressionField.gameObject.SetActive(true);
            yield return new WaitForSeconds(_suppressDuration);
            if (_suppressionField != null) _suppressionField.gameObject.SetActive(false);
        }
    }
}
