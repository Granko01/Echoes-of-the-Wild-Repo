using System.Collections.Generic;
using UnityEngine;

// Scene-level manager that opens/closes the match-3 mini-game and translates
// match results into entity state reductions and world effects.
public class Match3Manager : MonoBehaviour
{
    public static Match3Manager Instance { get; private set; }

    [SerializeField] private GameObject _gridUI;

    private Match3Grid       _grid;
    private EntityController _target;
    private bool             _isOpen;
    private int              _gridWidth  = 3;
    private int              _gridHeight = 3;

    // Available tile types scale with act (Calm+Trust → +Bond → +Fear)
    private int _availableTileTypes = 2;

    public bool             IsOpen => _isOpen;
    public EntityController Target => _target;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Auto-find the panel if it wasn't wired in the editor
        if (_gridUI == null)
        {
            var ui = FindFirstObjectByType<Match3UI>(FindObjectsInactive.Include);
            if (ui != null) _gridUI = ui.gameObject;
        }
    }

    public void SetGridSize(int w, int h) { _gridWidth = w; _gridHeight = h; }
    public void SetAvailableTileTypes(int count) => _availableTileTypes = count;

    public void OpenGrid(EntityController target)
    {
        if (_isOpen) return;
        _isOpen = true;
        _target = target;
        _grid   = new Match3Grid(_gridWidth, _gridHeight, _availableTileTypes);

        if (_gridUI != null)
        {
            _gridUI.SetActive(true);
            (_gridUI.GetComponent<Match3UI>() ?? _gridUI.GetComponentInChildren<Match3UI>())
                ?.BuildGrid(_gridWidth, _gridHeight);
        }
        Time.timeScale = 0.2f;
    }

    public void CloseGrid()
    {
        _isOpen = false;
        _target = null;
        if (_gridUI != null) _gridUI.SetActive(false);
        Time.timeScale = 1f;
    }

    // Called by Match3UI when player swaps two tiles
    public bool OnSwap(int x1, int y1, int x2, int y2)
    {
        if (!_isOpen) return false;
        _grid.Swap(x1, y1, x2, y2);
        var matches = _grid.FindAndClearMatches();

        if (matches.Count == 0)
        {
            _grid.Swap(x1, y1, x2, y2); // revert invalid swap
            return false;
        }

        foreach (var (type, count) in matches)
        {
            ApplyEffect(type, count);
            GameEvents.RaiseMatchResolve(type, count);
        }

        var sm = _target?.GetComponent<EntityStateMachine>();
        if (sm != null && sm.Current == EntityState.Stable)
            CloseGrid();

        return true;
    }

    // Direct grid read for UI rendering
    public TileType GetTile(int x, int y) => _grid?.Get(x, y) ?? TileType.Calm;

    private void ApplyEffect(TileType type, int count)
    {
        if (_target == null) return;
        var sm = _target.GetComponent<EntityStateMachine>();

        switch (type)
        {
            case TileType.Calm:
                sm?.ReduceState(1);
                break;
            case TileType.Trust:
                sm?.ReduceState(1);
                break;
            case TileType.Bond:
                // AoE stabilization of nearby entities
                if (_target != null)
                {
                    var nearby = Physics2D.OverlapCircleAll(_target.transform.position, 5f);
                    foreach (var col in nearby)
                        col.GetComponent<EntityStateMachine>()?.ReduceState(1);
                }
                break;
            case TileType.Fear:
                // Stabilizes chaos spikes (Act 5) — stronger reduction
                sm?.ReduceState(2);
                break;
        }
    }
}
