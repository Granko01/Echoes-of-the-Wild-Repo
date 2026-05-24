using System.Collections.Generic;
using UnityEngine;

// Tracks bond levels (I–III) per EntityType and exposes passive buff values.
// Bond levels unlock on heal count milestones and raise OnBondLevelUp.
public class BondSystem : MonoBehaviour
{
    public static BondSystem Instance { get; private set; }

    private readonly Dictionary<EntityType, int> _bondLevels = new();
    private readonly Dictionary<EntityType, int> _healCounts = new();

    private const int HealsPerBondLevel = 3;
    private const int MaxBondLevel      = 3;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        SaveSystem.LoadBonds(this);
    }

    public void RegisterHeal(EntityType type)
    {
        _healCounts[type] = _healCounts.GetValueOrDefault(type) + 1;

        int prevLevel = GetBondLevel(type);
        int newLevel  = Mathf.Min(_healCounts[type] / HealsPerBondLevel, MaxBondLevel);

        if (newLevel > prevLevel)
        {
            _bondLevels[type] = newLevel;
            GameEvents.RaiseBondLevelUp(type, newLevel);
            SaveSystem.Save(GameManager.Instance.CurrentAct, this);
        }
    }

    public int GetBondLevel(EntityType type)   => _bondLevels.GetValueOrDefault(type);
    public int GetHealCount(EntityType type)   => _healCounts.GetValueOrDefault(type);

    public void SetBondData(EntityType type, int level, int healCount)
    {
        _bondLevels[type] = level;
        _healCounts[type] = healCount;
    }

    // ── Passive buff accessors ───────────────────────────────────────────────

    // Deer bond: +stability gain per pulse hit
    public float GetStabilityGainBonus() => GetBondLevel(EntityType.Deer) * 0.15f;

    // Elephant bond: +pulse radius when Spirit Assist is used
    public float GetPulseRadiusBonus() => GetBondLevel(EntityType.Elephant) * 0.5f;

    // Spirit Assist execution (Elephant: barrier pulse)
    public void ActivateAssist(Vector2 origin)
    {
        if (GetBondLevel(EntityType.Elephant) < 1) return;
        var nearby = Physics2D.OverlapCircleAll(origin, 5f + GetPulseRadiusBonus());
        foreach (var col in nearby)
            col.GetComponent<EntityStateMachine>()?.ReduceState(1);
        Debug.Log("[BondSystem] Elephant Spirit Assist activated");
    }
}
