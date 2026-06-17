using System;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
    public static EnergyManager Instance { get; private set; }

    public const int MaxEnergy        = 60;
    public const int NormalRefillSecs = 300; // 1 energy per 5 min
    public const int VIPRefillSecs    = 180; // 1 energy per 3 min (VIP benefit)

    public int  Energy => SaveSystem.Data.energy;
    public bool IsFull => SaveSystem.Data.energy >= MaxEnergy;

    // Seconds elapsed in current refill tick — used by HUD for countdown display
    private float _refillTimer;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        RecoverOfflineEnergy();
        GameEvents.RaiseEnergyChanged(Energy, MaxEnergy);
    }

    private void Update()
    {
        if (IsFull) return;

        _refillTimer += Time.unscaledDeltaTime;
        float interval = RefillInterval();

        if (_refillTimer >= interval)
        {
            int ticks = Mathf.FloorToInt(_refillTimer / interval);
            _refillTimer -= ticks * interval;
            Grant(ticks);
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    // Pass isMatch3:true when spending energy for a Match-3 level.
    // During the InfiniteMatch3Lives mini event the deduction is skipped.
    public bool SpendEnergy(int amount, bool isMatch3 = false)
    {
        if (isMatch3 && MiniEventManager.Instance != null &&
            MiniEventManager.Instance.InfiniteMatch3Lives)
            return true;

        if (SaveSystem.Data.energy < amount) return false;
        bool wasFull = IsFull;
        SaveSystem.Data.energy -= amount;
        if (wasFull)
        {
            // Start tracking refill from the moment energy drops below max
            SaveSystem.Data.energyLastRefillTime = DateTime.UtcNow.ToString("O");
            _refillTimer = 0f;
        }
        SaveSystem.Save();
        GameEvents.RaiseEnergyChanged(Energy, MaxEnergy);
        return true;
    }

    // Used by rewarded ads and VIP daily rewards
    public void AddEnergy(int amount) => Grant(amount);

    // Returns how long until the next +1 energy tick (for HUD countdown)
    public TimeSpan TimeUntilNextEnergy()
    {
        if (IsFull) return TimeSpan.Zero;
        float remaining = RefillInterval() - _refillTimer;
        return TimeSpan.FromSeconds(Mathf.Max(0f, remaining));
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private float RefillInterval() =>
        SaveSystem.Data.vipActive ? VIPRefillSecs : NormalRefillSecs;

    private void Grant(int amount)
    {
        SaveSystem.Data.energy = Mathf.Min(SaveSystem.Data.energy + amount, MaxEnergy);

        if (IsFull)
            SaveSystem.Data.energyLastRefillTime = "";
        else if (string.IsNullOrEmpty(SaveSystem.Data.energyLastRefillTime))
            SaveSystem.Data.energyLastRefillTime = DateTime.UtcNow.ToString("O");

        SaveSystem.Save();
        GameEvents.RaiseEnergyChanged(Energy, MaxEnergy);
    }

    // Calculates how much energy recovered while the app was closed
    private void RecoverOfflineEnergy()
    {
        if (IsFull || string.IsNullOrEmpty(SaveSystem.Data.energyLastRefillTime)) return;

        if (!DateTime.TryParse(SaveSystem.Data.energyLastRefillTime,
                               null,
                               System.Globalization.DateTimeStyles.RoundtripKind,
                               out DateTime lastRefill)) return;

        double elapsed  = (DateTime.UtcNow - lastRefill).TotalSeconds;
        float  interval = RefillInterval();
        int    recovered = Mathf.FloorToInt((float)elapsed / interval);

        if (recovered > 0)
        {
            // Position the in-session timer at the fractional tick already elapsed
            float leftover = (float)elapsed - (recovered * interval);
            _refillTimer = leftover;
            Grant(recovered);
        }
        else
        {
            // Less than one tick elapsed — restore timer position so no time is lost
            _refillTimer = (float)elapsed;
        }
    }
}
