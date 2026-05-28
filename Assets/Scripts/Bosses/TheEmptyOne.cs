using UnityEngine;

// Chapter 4 — Celestial Realm — Final Boss: The Empty One
// Phase 1: Previous boss abilities.
// Phase 2: Uses Punch's abilities.
// Final Phase: All rescued creatures' voices return; relics glow; Awakened Punch unlocked.
// Full AI: stub — to be implemented after Ch1/Ch2 bosses are complete.
public class TheEmptyOne : ChapterBossController
{
    [Header("Audio")]
    [SerializeField] private AudioClip _battleStartClip;
    [SerializeField] private AudioClip _phase2Clip;
    [SerializeField] private AudioClip _finalPhaseClip;
    [SerializeField] private AudioClip _defeatClip;

    protected override void OnActivated()
    {
        AudioManager.Instance?.PlaySFX(_battleStartClip);
        // Mother appears smiling. Punch runs. Reality breaks. Darkness forms.
        StartCoroutine(BossLoop());
    }

    protected override void OnPhaseChanged(int phase)
    {
        AudioClip clip = phase switch
        {
            2 => _phase2Clip,
            3 => _finalPhaseClip,
            _ => null
        };
        if (clip != null) AudioManager.Instance?.PlaySFX(clip);
    }

    protected override void OnDefeated()
    {
        AudioManager.Instance?.PlaySFX(_defeatClip);
        // Darkness shatters. Silence disappears. World heals.
        // Punch wakes in cave. Mother arrives. Real ending begins.
    }

    private System.Collections.IEnumerator BossLoop()
    {
        // TODO: Phase 1 reuses previous boss coroutines; Phase 2 mirrors player; Phase 3 scripted sequence
        yield break;
    }
}
