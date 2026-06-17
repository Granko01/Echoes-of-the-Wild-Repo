using System;
using UnityEngine;

public enum MiniEventType { None, DoubleCoins, DoubleMaterials, InfiniteMatch3Lives, TreasureHunt, BossFrenzy }

public class MiniEventManager : MonoBehaviour
{
    public static MiniEventManager Instance { get; private set; }

    private static readonly MiniEventType[] Rotation =
    {
        MiniEventType.DoubleCoins,
        MiniEventType.DoubleMaterials,
        MiniEventType.InfiniteMatch3Lives,
        MiniEventType.TreasureHunt,
        MiniEventType.BossFrenzy,
    };

    // Fixed epoch — adjust this to your actual soft-launch date
    public static readonly DateTime RotationEpoch = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
    public const int EventDurationDays = 3;

    public MiniEventType CurrentEvent { get; private set; } = MiniEventType.None;

    // ── Multipliers — pass applyEventMultiplier:true for in-game drops only ──
    public float CoinMultiplier =>
        CurrentEvent == MiniEventType.DoubleCoins ||
        CurrentEvent == MiniEventType.TreasureHunt ? 2f : 1f;

    public float MaterialMultiplier =>
        CurrentEvent == MiniEventType.DoubleMaterials ? 2f : 1f;

    public bool InfiniteMatch3Lives => CurrentEvent == MiniEventType.InfiniteMatch3Lives;
    public bool BossFrenzyActive    => CurrentEvent == MiniEventType.BossFrenzy;

    // How long until this event swaps to the next one
    public TimeSpan TimeUntilNextEvent
    {
        get
        {
            int daysSinceEpoch = (int)(DateTime.UtcNow - RotationEpoch).TotalDays;
            int daysRemaining  = EventDurationDays - (daysSinceEpoch % EventDurationDays);
            return DateTime.UtcNow.Date.AddDays(daysRemaining) - DateTime.UtcNow;
        }
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => RefreshEvent();

    // Call each time the game resumes from background to catch day boundary changes
    public void RefreshEvent()
    {
        var next = DetermineCurrentEvent();
        if (next == CurrentEvent) return;
        CurrentEvent = next;
        GameEvents.RaiseMiniEventChanged(CurrentEvent);
        Debug.Log($"[MiniEventManager] Active: {CurrentEvent}  (next in {TimeUntilNextEvent:hh\\:mm\\:ss})");
    }

    private static MiniEventType DetermineCurrentEvent()
    {
        int days = (int)(DateTime.UtcNow - RotationEpoch).TotalDays;
        if (days < 0) return MiniEventType.None;
        return Rotation[(days / EventDurationDays) % Rotation.Length];
    }
}
