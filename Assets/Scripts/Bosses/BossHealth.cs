using UnityEngine;

// Numeric HP system for chapter bosses (0–100).
// Phase thresholds:  Phase 2 at ≤65%, Phase 3 at ≤25%.
// At Phase 3, PurifyBurstAvailable becomes true — HUD shows the prompt.
// ApplyPurifyBurst() finishes the encounter by transitioning EntityStateMachine to Stable.
[RequireComponent(typeof(EntityStateMachine))]
public class BossHealth : MonoBehaviour
{
    [SerializeField] private float _maxHP          = 100f;
    [SerializeField] private float _phase2Threshold = 65f;
    [SerializeField] private float _phase3Threshold = 25f;

    public float CurrentHP            { get; private set; }
    public float NormalizedHP         => CurrentHP / _maxHP;
    public int   CurrentPhase         { get; private set; } = 1;
    public bool  PurifyBurstAvailable { get; private set; }
    public bool  IsDefeated           { get; private set; }

    private EntityStateMachine _sm;

    private void Awake()
    {
        _sm        = GetComponent<EntityStateMachine>();
        CurrentHP  = _maxHP;
    }

    public void TakeDamage(float amount)
    {
        if (IsDefeated) return;

        CurrentHP = Mathf.Max(0f, CurrentHP - amount);
        GameEvents.RaiseBossHPChanged(NormalizedHP);

        CheckPhaseTransition();
    }

    // Called by StoneGloves Armor Break — signals boss to drop crystal shield
    public void NotifyArmorBreak() => GameEvents.RaiseBossArmorBreak();

    // Called by MemoryFlower touch — boss recognition moment
    public void NotifyMemoryFlowerTouch() => GameEvents.RaiseMemoryFlowerTouch();

    public void ApplyPurifyBurst()
    {
        if (IsDefeated) return;
        IsDefeated           = true;
        PurifyBurstAvailable = false;
        CurrentHP            = 0f;

        GameEvents.RaiseBossHPChanged(0f);
        GameEvents.RaiseBossDefeated(gameObject.name);

        // Transition entity state machine to Stable — triggers existing nuzzle / companion logic
        _sm?.TransitionTo(EntityState.Stable);
    }

    private void CheckPhaseTransition()
    {
        int newPhase = CurrentPhase;

        if (CurrentPhase < 3 && NormalizedHP * 100f <= _phase3Threshold)
        {
            newPhase             = 3;
            PurifyBurstAvailable = true;
        }
        else if (CurrentPhase < 2 && NormalizedHP * 100f <= _phase2Threshold)
        {
            newPhase = 2;
        }

        if (newPhase != CurrentPhase)
        {
            CurrentPhase = newPhase;
            GameEvents.RaiseBossPhaseChanged(CurrentPhase);
        }
    }
}
