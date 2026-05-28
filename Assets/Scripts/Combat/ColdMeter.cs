using UnityEngine;

// Chapter 3 mechanic — Cold Meter.
// Fills when the player is stationary; movement drains it.
// At 1.0: player briefly freezes. Stone Gloves attacks drain cold.
public class ColdMeter : MonoBehaviour
{
    public static ColdMeter Instance { get; private set; }

    [SerializeField] private float _fillRate  = 0.08f;   // per second when still
    [SerializeField] private float _drainRate = 0.2f;    // per second when moving
    [SerializeField] private float _freezeDuration = 1.5f;

    public float Value   { get; private set; }   // 0..1
    public bool  IsFrozen { get; private set; }

    private PlayerController _pc;
    private PlayerHealth     _ph;
    private bool             _active;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _pc = FindFirstObjectByType<PlayerController>();
        _ph = FindFirstObjectByType<PlayerHealth>();
    }

    // Called from outside (e.g., a BiomeArea trigger) to enable Snow Realm mechanic
    public void SetActive(bool active) => _active = active;

    private void Update()
    {
        if (!_active || IsFrozen) return;

        bool moving = _pc != null && _pc.IsMoving;
        if (moving)
            Value = Mathf.Max(0f, Value - _drainRate * Time.deltaTime);
        else
            Value = Mathf.Min(1f, Value + _fillRate * Time.deltaTime);

        GameEvents.RaiseColdMeterChanged(Value);

        if (Value >= 1f) StartCoroutine(FreezeRoutine());
    }

    public void Drain(float amount)
    {
        Value = Mathf.Max(0f, Value - amount);
        GameEvents.RaiseColdMeterChanged(Value);
    }

    private System.Collections.IEnumerator FreezeRoutine()
    {
        IsFrozen = true;
        Value    = 0f;
        GameEvents.RaiseColdMeterChanged(0f);

        // Stun: zero player velocity
        var rb = _pc != null ? _pc.GetComponent<Rigidbody2D>() : null;
        Vector2 saved = rb != null ? rb.linearVelocity : Vector2.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(_freezeDuration);

        IsFrozen = false;
        if (rb != null) rb.linearVelocity = saved;
    }
}
