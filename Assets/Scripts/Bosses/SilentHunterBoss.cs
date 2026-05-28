using UnityEngine;

// Chapter 3 — Snow Realm — Boss 2: The Silent Hunter
// Invisible boss. Music disappears. UI fades. Player predicts movement via
// footprints, breath clouds, and snow displacement.
// Full AI: stub — core hooks are wired, behavior loop to be implemented.
public class SilentHunterBoss : ChapterBossController
{
    [Header("Audio")]
    [SerializeField] private AudioClip _defeatClip;

    private int _currentPhase = 1;

    protected override void OnActivated()
    {
        // Music disappears — UI fades — snowstorm starts.
        // TODO: trigger MusicManager.FadeOut(), HUD.Hide(), SnowstormVFX.Play()
        StartCoroutine(BossLoop());
    }

    protected override void OnPhaseChanged(int phase) => _currentPhase = phase;

    protected override void OnDefeated()
    {
        AudioManager.Instance?.PlaySFX(_defeatClip);
        // "Those eyes..." — walks away peacefully.
    }

    private System.Collections.IEnumerator BossLoop()
    {
        // TODO: Phase AI — invisible movement, footprint trail, breath detection
        yield break;
    }
}
