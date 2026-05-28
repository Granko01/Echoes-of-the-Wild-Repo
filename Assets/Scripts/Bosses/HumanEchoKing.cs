using UnityEngine;

// Chapter 3 — Snow Realm — Boss 3: Human Echo King
// Copies player's jump, dodge, combo, and attacks.
// Full AI: stub — mirroring system to be implemented.
public class HumanEchoKing : ChapterBossController
{
    [Header("Audio")]
    [SerializeField] private AudioClip _defeatClip;

    protected override void OnActivated() => StartCoroutine(BossLoop());

    protected override void OnPhaseChanged(int phase) { }

    protected override void OnDefeated()
    {
        AudioManager.Instance?.PlaySFX(_defeatClip);
        // Touches Punch's chest glow. Smiles sadly. Disappears. Leaves memory.
    }

    private System.Collections.IEnumerator BossLoop()
    {
        // TODO: mirror PlayerController inputs with slight delay
        yield break;
    }
}
