using System.Collections.Generic;
using UnityEngine;

// Singleton registry of all active SoundSources in the scene.
// CaveMawBoss polls GetNearestSource() each frame to decide where to investigate.
public class SoundDetector : MonoBehaviour
{
    public static SoundDetector Instance { get; private set; }

    private readonly List<SoundSource> _sources = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        // Age out expired sources
        for (int i = _sources.Count - 1; i >= 0; i--)
        {
            _sources[i].Tick(Time.deltaTime);
            if (_sources[i].IsExpired) _sources.RemoveAt(i);
        }
    }

    public void RegisterSource(SoundSource source)
    {
        _sources.Add(source);
        GameEvents.RaiseSoundSourceCreated(source.Position);
    }

    public void DeregisterSource(SoundSource source) => _sources.Remove(source);

    // Returns the strongest (most recent / highest-priority) source near a given position.
    // Prefers non-player sources (decoys) to give the stealth mechanic weight.
    public SoundSource GetNearestSource(Vector2 fromPos, float maxRange = float.MaxValue)
    {
        SoundSource best     = null;
        float       bestDist = float.MaxValue;

        foreach (var s in _sources)
        {
            float dist = Vector2.Distance(fromPos, s.Position);
            if (dist > maxRange) continue;

            // Prefer decoys (non-player) — they should draw attention more strongly
            bool betterPriority = best == null
                || (!s.IsPlayer && best.IsPlayer)
                || (s.IsPlayer == best.IsPlayer && dist < bestDist);

            if (betterPriority)
            {
                best     = s;
                bestDist = dist;
            }
        }

        return best;
    }

    public bool HasAnySources() => _sources.Count > 0;
}
