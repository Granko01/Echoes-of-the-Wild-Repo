using System;
using UnityEngine;

// Central event bus wiring all GDD implementation triggers.
public static class GameEvents
{
    // ── Entity / Match-3 (existing) ───────────────────────────────────────────
    public static event Action<EntityController>              OnPulseHit;
    public static event Action<TileType, int>                 OnMatchResolve;
    public static event Action<EntityController, EntityState> OnStateChange;
    public static event Action<BiomeArea>                     OnHealComplete;
    public static event Action<EntityType, int>               OnBondLevelUp;
    public static event Action                                OnRealmTransition;
    public static event Action<MiniBossType>                  OnMiniBossActivated;
    public static event Action<MiniBossType>                  OnMiniBossDefeated;
    public static event Action<int, int>                      OnPlayerDamaged;    // (currentHearts, maxHearts)
    public static event Action                                OnPlayerDied;
    public static event Action                                OnLevelComplete;

    // ── Weapon Combat (new) ───────────────────────────────────────────────────
    public static event Action<WeaponBase, Vector2>           OnWeaponAttack;
    public static event Action<WeaponBase>                    OnWeaponSkillUsed;
    public static event Action                                OnPurifyBurstActivated;
    public static event Action<WeaponData>                    OnWeaponUpgraded;
    public static event Action<int>                           OnFragmentCollected;   // total count
    public static event Action<int>                           OnComboChanged;        // current combo

    // ── Boss System (new) ─────────────────────────────────────────────────────
    public static event Action<int>                           OnBossPhaseChanged;    // phase number 1/2/3
    public static event Action<float>                         OnBossHPChanged;       // normalized 0..1
    public static event Action<string>                        OnBossDefeated;        // boss GameObject name

    // ── Environment (new) ────────────────────────────────────────────────────
    public static event Action<Vector3>                       OnSoundSourceCreated;
    public static event Action<float>                         OnColdMeterChanged;    // 0..1
    public static event Action                                OnBossArmorBreak;
    public static event Action                                OnMemoryFlowerTouch;

    // ── Raise helpers (existing) ──────────────────────────────────────────────
    public static void RaisePulseHit(EntityController entity)              => OnPulseHit?.Invoke(entity);
    public static void RaiseMatchResolve(TileType type, int count)         => OnMatchResolve?.Invoke(type, count);
    public static void RaiseStateChange(EntityController e, EntityState s) => OnStateChange?.Invoke(e, s);
    public static void RaiseHealComplete(BiomeArea area)                   => OnHealComplete?.Invoke(area);
    public static void RaiseBondLevelUp(EntityType type, int level)        => OnBondLevelUp?.Invoke(type, level);
    public static void RaiseRealmTransition()                              => OnRealmTransition?.Invoke();
    public static void RaiseMiniBossActivated(MiniBossType type)           => OnMiniBossActivated?.Invoke(type);
    public static void RaiseMiniBossDefeated(MiniBossType type)            => OnMiniBossDefeated?.Invoke(type);
    public static void RaisePlayerDamaged(int current, int max)            => OnPlayerDamaged?.Invoke(current, max);
    public static void RaisePlayerDied()                                   => OnPlayerDied?.Invoke();
    public static void RaiseLevelComplete()                                => OnLevelComplete?.Invoke();

    // ── Raise helpers (new) ───────────────────────────────────────────────────
    public static void RaiseWeaponAttack(WeaponBase w, Vector2 dir)   => OnWeaponAttack?.Invoke(w, dir);
    public static void RaiseWeaponSkillUsed(WeaponBase w)             => OnWeaponSkillUsed?.Invoke(w);
    public static void RaisePurifyBurstActivated()                    => OnPurifyBurstActivated?.Invoke();
    public static void RaiseWeaponUpgraded(WeaponData d)              => OnWeaponUpgraded?.Invoke(d);
    public static void RaiseFragmentCollected(int total)              => OnFragmentCollected?.Invoke(total);
    public static void RaiseComboChanged(int combo)                   => OnComboChanged?.Invoke(combo);
    public static void RaiseBossPhaseChanged(int phase)               => OnBossPhaseChanged?.Invoke(phase);
    public static void RaiseBossHPChanged(float normalized)           => OnBossHPChanged?.Invoke(normalized);
    public static void RaiseBossDefeated(string bossId)               => OnBossDefeated?.Invoke(bossId);
    public static void RaiseSoundSourceCreated(Vector3 pos)           => OnSoundSourceCreated?.Invoke(pos);
    public static void RaiseColdMeterChanged(float value)             => OnColdMeterChanged?.Invoke(value);
    public static void RaiseBossArmorBreak()                          => OnBossArmorBreak?.Invoke();
    public static void RaiseMemoryFlowerTouch()                       => OnMemoryFlowerTouch?.Invoke();
}
