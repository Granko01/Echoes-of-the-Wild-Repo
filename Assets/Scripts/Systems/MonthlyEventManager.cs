using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public class EventConfig
{
    public string eventId;
    public string displayName;
    public string theme;
    public string bossName;
    public int    totalPuzzlePieces = 100;
    public int    durationDays      = 28;
}

public class MonthlyEventManager : MonoBehaviour
{
    public static MonthlyEventManager Instance { get; private set; }

    [SerializeField] private EventConfig _currentEventConfig;

    public bool IsEventActive =>
        _currentEventConfig != null &&
        SaveSystem.Data.activeEventId == _currentEventConfig.eventId;

    public int  EventCurrency        => SaveSystem.Data.eventCurrency;
    public int  PuzzlePiecesCollected => SaveSystem.Data.puzzlePiecesCollected;
    public bool PuzzleComplete        =>
        _currentEventConfig != null &&
        SaveSystem.Data.puzzlePiecesCollected >= _currentEventConfig.totalPuzzlePieces;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (_currentEventConfig != null && !IsEventActive)
            ActivateEvent(_currentEventConfig.eventId);
    }

    // ── Event Lifecycle ───────────────────────────────────────────────────────

    public void ActivateEvent(string eventId)
    {
        SaveSystem.Data.activeEventId          = eventId;
        SaveSystem.Data.puzzlePiecesCollected  = 0;
        SaveSystem.Data.eventCurrency          = 0;
        SaveSystem.Data.eventPass              = new PassSaveData();
        SaveSystem.Save();
    }

    // ── Event Currency ────────────────────────────────────────────────────────

    public void AddEventCurrency(int amount)
    {
        if (amount <= 0) return;
        SaveSystem.Data.eventCurrency += amount;
        SaveSystem.Save();
        GameEvents.RaiseEventCurrencyChanged(SaveSystem.Data.eventCurrency);
    }

    public bool SpendEventCurrency(int amount)
    {
        if (SaveSystem.Data.eventCurrency < amount) return false;
        SaveSystem.Data.eventCurrency -= amount;
        SaveSystem.Save();
        GameEvents.RaiseEventCurrencyChanged(SaveSystem.Data.eventCurrency);
        return true;
    }

    // ── Puzzle Collection ─────────────────────────────────────────────────────

    public void AddPuzzlePieces(int count)
    {
        if (!IsEventActive || _currentEventConfig == null) return;

        bool wasComplete = PuzzleComplete;
        SaveSystem.Data.puzzlePiecesCollected = Mathf.Min(
            SaveSystem.Data.puzzlePiecesCollected + count,
            _currentEventConfig.totalPuzzlePieces);
        SaveSystem.Save();

        GameEvents.RaisePuzzlePieceCollected(SaveSystem.Data.puzzlePiecesCollected);

        if (!wasComplete && PuzzleComplete)
            OnPuzzleCompleted();
    }

    private void OnPuzzleCompleted()
    {
        // Completion rewards: Costume + Animated Wallpaper + Frame + Title
        string costumeId = $"{SaveSystem.Data.activeEventId}_completion_costume";
        CostumeManager.Instance?.UnlockCosmetic(costumeId);
        Debug.Log($"[MonthlyEventManager] Puzzle complete — event: {SaveSystem.Data.activeEventId}");
    }

    // ── Event Pass ────────────────────────────────────────────────────────────

    // Called by IAPManager after EventPass purchase
    public void ActivateEventPass()
    {
        SaveSystem.Data.eventPass.isPremium  = true;
        SaveSystem.Data.eventPass.expiryDate = DateTime.UtcNow.AddDays(28).ToString("O");
        SaveSystem.Save();
    }

    public bool IsEventPassActive =>
        SaveSystem.Data.eventPass.isPremium &&
        !string.IsNullOrEmpty(SaveSystem.Data.eventPass.expiryDate) &&
        DateTime.TryParse(SaveSystem.Data.eventPass.expiryDate, null,
            DateTimeStyles.RoundtripKind, out var dt) &&
        DateTime.UtcNow < dt;
}
