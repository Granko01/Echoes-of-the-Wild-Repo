using System.Collections;
using UnityEngine;

// Attach to the boss entity alongside EntityController + EntityStateMachine.
// Activates automatically when the player walks within range.
public class BossController : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField] private float _activationRange = 10f;

    [Header("Charge Attack")]
    [SerializeField] private float _chargeInterval = 5f;

    [Header("Suppression Pulses")]
    [SerializeField] private float          _suppressInterval = 10f;
    [SerializeField] private float          _suppressDuration =  4f;
    [SerializeField] private SuppressionField _suppressionField;

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
        StartCoroutine(ChargeLoop());
        if (_suppressionField != null)
            StartCoroutine(SuppressionPulse());
    }

    private IEnumerator ChargeLoop()
    {
        while (_sm != null && _sm.Current != EntityState.Stable)
        {
            yield return new WaitForSeconds(_chargeInterval);
            if (_sm.Current == EntityState.Turbulent && _player != null && _entity != null)
                _entity.TriggerCharge(_player);
        }
        // Boss defeated — kill any leftover suppression
        if (_suppressionField != null)
            _suppressionField.gameObject.SetActive(false);
    }

    private IEnumerator SuppressionPulse()
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
