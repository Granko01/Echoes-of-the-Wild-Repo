using System.Collections.Generic;
using UnityEngine;

// Pure data model for the match-3 grid — no Unity lifecycle.
public class Match3Grid
{
    public int Width  { get; private set; }
    public int Height { get; private set; }

    private TileType[,] _cells;
    private readonly int _tileTypeCount;

    public Match3Grid(int width, int height, int availableTileTypes = 2)
    {
        Width          = width;
        Height         = height;
        _tileTypeCount = Mathf.Clamp(availableTileTypes, 2, System.Enum.GetValues(typeof(TileType)).Length);
        _cells         = new TileType[width, height];
        Randomize();
    }

    public TileType Get(int x, int y) => _cells[x, y];

    public void Swap(int x1, int y1, int x2, int y2)
        => (_cells[x1, y1], _cells[x2, y2]) = (_cells[x2, y2], _cells[x1, y1]);

    // Returns all matched (type, count) pairs and refills cleared cells.
    public List<(TileType type, int count)> FindAndClearMatches()
    {
        var matched = new bool[Width, Height];

        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width - 2; x++)
                if (_cells[x, y] == _cells[x+1, y] && _cells[x, y] == _cells[x+2, y])
                    matched[x, y] = matched[x+1, y] = matched[x+2, y] = true;

        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height - 2; y++)
                if (_cells[x, y] == _cells[x, y+1] && _cells[x, y] == _cells[x, y+2])
                    matched[x, y] = matched[x, y+1] = matched[x, y+2] = true;

        var totals = new Dictionary<TileType, int>();
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                if (matched[x, y])
                {
                    var t = _cells[x, y];
                    totals[t] = totals.GetValueOrDefault(t) + 1;
                }

        // Gravity: compact surviving tiles downward then fill top with new tiles
        for (int x = 0; x < Width; x++)
        {
            int writeRow = 0;
            for (int y = 0; y < Height; y++)
                if (!matched[x, y])
                    _cells[x, writeRow++] = _cells[x, y];
            while (writeRow < Height)
                _cells[x, writeRow++] = (TileType)Random.Range(0, _tileTypeCount);
        }

        var result = new List<(TileType, int)>();
        foreach (var kv in totals) result.Add((kv.Key, kv.Value));
        return result;
    }

    private void Randomize()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                _cells[x, y] = (TileType)Random.Range(0, _tileTypeCount);
    }
}
