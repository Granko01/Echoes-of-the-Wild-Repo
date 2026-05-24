using System.Collections;
using UnityEngine;

public enum EntityType { Deer, Elephant, Fish, Bird, Unknown }

[RequireComponent(typeof(EntityStateMachine), typeof(Rigidbody2D))]
public class EntityController : MonoBehaviour
{
    [SerializeField] private EntityType _entityType    = EntityType.Unknown;
    [SerializeField] private float      _chargeSpeed   = 4f;
    [SerializeField] private float      _skittishRange = 3f;
    [SerializeField] private float      _patrolDistance = 2f;

    public EntityType Type => _entityType;

    private EntityStateMachine _stateMachine;
    private Rigidbody2D        _rb;
    private Transform          _patrolOrigin;
    private bool               _isActing;

    private void Awake()
    {
        _stateMachine = GetComponent<EntityStateMachine>();
        _rb           = GetComponent<Rigidbody2D>();
        _patrolOrigin = new GameObject("PatrolOrigin").transform;
        _patrolOrigin.position = transform.position;
    }

    private void Update()
    {
        if (_isActing || _stateMachine.Current == EntityState.Stable) return;
        if (_stateMachine.Current == EntityState.Agitated || _stateMachine.Current == EntityState.Distressed)
            SkittishBehavior();
    }

    // Called by PlayerAbilities when Pulse Wave overlaps this entity
    public void OnPulseHit()
    {
        if (_stateMachine.Current == EntityState.Empty) return;
        _stateMachine.ReduceState(1);
        GameEvents.RaisePulseHit(this);
    }

    // Called by PlayerAbilities Emotional Burst — unlocks Empty entities
    public void OnEmotionalBurst() => _stateMachine.ConvertEmptyToReceptive();

    // Called by EntityStateMachine when Overwhelmed threshold is crossed
    public void OnOverwhelmed() => Match3Manager.Instance?.OpenGrid(this);

    // Called by EntityStateMachine when entity reaches Stable
    public void OnStabilized()
    {
        StopAllCoroutines();
        _isActing = false;
        _rb.linearVelocity = Vector2.zero;
        BondSystem.Instance?.RegisterHeal(_entityType);
        StartCoroutine(NuzzleAnimation());
    }

    private void SkittishBehavior()
    {
        // Skittish loop: bounce between patrol points
        if (!_isActing) StartCoroutine(PatrolRoutine());
    }

    private void OnDestroy()
    {
        if (_patrolOrigin != null) Destroy(_patrolOrigin.gameObject);
    }

    public void TriggerCharge(Transform target)
    {
        if (!_isActing) StartCoroutine(ChargeRoutine(target));
    }

    private IEnumerator PatrolRoutine()
    {
        _isActing = true;
        float dir = Random.value > 0.5f ? 1f : -1f;
        Vector2 target = (Vector2)_patrolOrigin.position + Vector2.right * _patrolDistance * dir;
        while (Vector2.Distance(transform.position, target) > 0.1f)
        {
            _rb.linearVelocity = ((Vector2)transform.position - target).normalized * -2f;
            yield return null;
        }
        _rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);
        _isActing = false;
    }

    private IEnumerator ChargeRoutine(Transform target)
    {
        _isActing = true;
        Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        float elapsed = 0f;
        while (elapsed < 0.6f)
        {
            _rb.linearVelocity = dir * _chargeSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }
        _rb.linearVelocity = Vector2.zero;
        _isActing = false;
    }

    private IEnumerator NuzzleAnimation()
    {
        Vector3 original = transform.localScale;
        for (float t = 0f; t < 1f; t += Time.deltaTime * 2f)
        {
            transform.localScale = original * (1f + 0.15f * Mathf.Sin(t * Mathf.PI));
            yield return null;
        }
        transform.localScale = original;
    }
}
