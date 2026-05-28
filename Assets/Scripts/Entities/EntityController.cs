using System.Collections;
using UnityEngine;

public enum EntityType { Deer, Elephant, Fish, Bird, Unknown }

[RequireComponent(typeof(EntityStateMachine), typeof(Rigidbody2D))]
public class EntityController : MonoBehaviour
{
    [SerializeField] private EntityType _entityType     = EntityType.Unknown;
    [SerializeField] private float      _chargeSpeed    = 4f;
    [SerializeField] private float      _skittishRange  = 3f;
    [SerializeField] private float      _patrolDistance = 2f;

    [Header("Platform Navigation")]
    [SerializeField] private float _jumpForce = 9f;

    public EntityType Type => _entityType;

    private EntityStateMachine _stateMachine;
    private Rigidbody2D        _rb;
    private Transform          _patrolOrigin;
    private bool               _isActing;
    private bool               _isCharging;  // true only during ChargeRoutine — used for damage collision

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
        // Always fires — stops any ongoing patrol or previous charge first
        StopAllCoroutines();
        _isActing   = false;
        _isCharging = false;
        StartCoroutine(ChargeRoutine(target));
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
        _isActing   = true;
        _isCharging = true;

        // Lock direction at charge START so the player can dodge — feels like an attack
        float xDir      = target != null
                          ? Mathf.Sign(((Vector2)target.position - (Vector2)transform.position).x)
                          : 1f;
        float dashSpeed = _chargeSpeed * 2.5f;   // much faster than normal — clearly a dash
        float elapsed   = 0f;
        float stuckTime = 0f;
        float lastX     = transform.position.x;
        bool  hitPlayer = false;

        while (elapsed < 0.65f && !hitPlayer)
        {
            _rb.linearVelocity = new Vector2(xDir * dashSpeed, _rb.linearVelocity.y);

            // Proximity damage — works regardless of collider/trigger setup
            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 0.7f);
            foreach (var col in nearby)
            {
                if (col.gameObject == gameObject) continue;
                if (col.TryGetComponent<PlayerHealth>(out var ph))
                {
                    ph.TakeDamage(1);
                    hitPlayer = true;
                    break;
                }
            }

            // Stuck detection + jump
            float movedX    = Mathf.Abs(transform.position.x - lastX);
            float expectedX = dashSpeed * Time.deltaTime;
            stuckTime = movedX < expectedX * 0.25f ? stuckTime + Time.deltaTime : 0f;
            lastX     = transform.position.x;

            if (stuckTime > 0.12f && Mathf.Abs(_rb.linearVelocity.y) < 0.5f)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
                stuckTime = 0f;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Brief stop so it looks like a dash, not a slide
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        yield return new WaitForSeconds(0.3f);

        _isCharging = false;
        _isActing   = false;
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
