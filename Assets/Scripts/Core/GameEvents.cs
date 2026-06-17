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

    // ── Currency ──────────────────────────────────────────────────────────────
    public static event Action<int>                           OnGemsChanged;
    public static event Action<int>                           OnCoinsChanged;
    public static event Action<int>                           OnLeavesChanged;

    // ── Energy ────────────────────────────────────────────────────────────────
    public static event Action<int, int>                      OnEnergyChanged;  // (current, max)

    // ── IAP ───────────────────────────────────────────────────────────────────
    public static event Action<string>                        OnPurchaseSuccess; // productId
    public static event Action<string, string>                OnPurchaseFailed;  // (productId, reason)

    // ── Ads ───────────────────────────────────────────────────────────────────
    public static event Action<AdRewardType>                  OnRewardedAdCompleted;

    // ── Shop ──────────────────────────────────────────────────────────────────
    public static event Action                                OnShopOpened;
    public static event Action                                OnShopClosed;
    public static event Action<ShopTab>                       OnShopTabChanged;

    // ── Pass ──────────────────────────────────────────────────────────────────
    public static event Action<int>                           OnPassLevelUp;
    public static event Action<int, bool>                     OnPassRewardClaimed;    // (level, isPremium)

    // ── VIP ───────────────────────────────────────────────────────────────────
    public static event Action                                OnVIPDailyRewardClaimed;

    // ── Daily Login ───────────────────────────────────────────────────────────
    public static event Action<int>                           OnDailyLoginRewardClaimed; // streakDay 0-6

    // ── Missions ──────────────────────────────────────────────────────────────
    public static event Action<string, int, int>              OnMissionProgress;        // (id, current, target)
    public static event Action<string>                        OnMissionCompleted;
    public static event Action                                OnDailyMissionsAllCompleted;
    public static event Action                                OnWeeklyMissionsAllCompleted;
    public static event Action                                OnMatch3LevelComplete;

    // ── Cosmetics ─────────────────────────────────────────────────────────────
    public static event Action<string>                        OnCostumeUnlocked;
    public static event Action<string>                        OnCostumeEquipped;
    public static event Action<string>                        OnCompanionSkinEquipped;

    // ── Chests ────────────────────────────────────────────────────────────────
    public static event Action<ChestType>                     OnChestOpened;

    // ── Monthly Event ─────────────────────────────────────────────────────────
    public static event Action<int>                           OnPuzzlePieceCollected;   // total pieces
    public static event Action<int>                           OnEventCurrencyChanged;

    // ── Mini Events ───────────────────────────────────────────────────────────
    public static event Action<MiniEventType>                 OnMiniEventChanged;

    // ── Boss Rush ─────────────────────────────────────────────────────────────
    public static event Action                                OnBossRushStarted;
    public static event Action<int, string>                   OnBossRushNextBoss;       // (index, bossId)
    public static event Action<int, int>                      OnBossRushEnded;          // (score, medals)

    // ── Realm Race ────────────────────────────────────────────────────────────
    public static event Action<RealmRaceRewardTier>           OnRealmRaceRewardClaimed;

    // ── Boosters ──────────────────────────────────────────────────────────────
    public static event Action<BoosterType>                   OnBoosterUsed;
    public static event Action<int, int, int>                 OnBoosterCountsChanged;   // (bombs, rockets, rainbowOrbs)

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

    public static void RaiseGemsChanged(int amount)                   => OnGemsChanged?.Invoke(amount);
    public static void RaiseCoinsChanged(int amount)                  => OnCoinsChanged?.Invoke(amount);
    public static void RaiseLeavesChanged(int amount)                 => OnLeavesChanged?.Invoke(amount);
    public static void RaiseEnergyChanged(int current, int max)       => OnEnergyChanged?.Invoke(current, max);
    public static void RaisePurchaseSuccess(string productId)         => OnPurchaseSuccess?.Invoke(productId);
    public static void RaisePurchaseFailed(string id, string reason)  => OnPurchaseFailed?.Invoke(id, reason);
    public static void RaiseRewardedAdCompleted(AdRewardType type)    => OnRewardedAdCompleted?.Invoke(type);
    public static void RaiseShopOpened()                              => OnShopOpened?.Invoke();
    public static void RaiseShopClosed()                              => OnShopClosed?.Invoke();
    public static void RaiseShopTabChanged(ShopTab tab)               => OnShopTabChanged?.Invoke(tab);

    public static void RaisePassLevelUp(int level)                    => OnPassLevelUp?.Invoke(level);
    public static void RaisePassRewardClaimed(int level, bool prem)   => OnPassRewardClaimed?.Invoke(level, prem);
    public static void RaiseVIPDailyRewardClaimed()                   => OnVIPDailyRewardClaimed?.Invoke();
    public static void RaiseDailyLoginRewardClaimed(int day)          => OnDailyLoginRewardClaimed?.Invoke(day);
    public static void RaiseMissionProgress(string id, int cur, int t)=> OnMissionProgress?.Invoke(id, cur, t);
    public static void RaiseMissionCompleted(string id)               => OnMissionCompleted?.Invoke(id);
    public static void RaiseDailyMissionsAllCompleted()               => OnDailyMissionsAllCompleted?.Invoke();
    public static void RaiseWeeklyMissionsAllCompleted()              => OnWeeklyMissionsAllCompleted?.Invoke();
    public static void RaiseMatch3LevelComplete()                     => OnMatch3LevelComplete?.Invoke();
    public static void RaiseCostumeUnlocked(string id)                => OnCostumeUnlocked?.Invoke(id);
    public static void RaiseCostumeEquipped(string id)                => OnCostumeEquipped?.Invoke(id);
    public static void RaiseCompanionSkinEquipped(string id)          => OnCompanionSkinEquipped?.Invoke(id);
    public static void RaiseChestOpened(ChestType type)               => OnChestOpened?.Invoke(type);
    public static void RaisePuzzlePieceCollected(int total)                   => OnPuzzlePieceCollected?.Invoke(total);
    public static void RaiseEventCurrencyChanged(int amount)                  => OnEventCurrencyChanged?.Invoke(amount);
    public static void RaiseMiniEventChanged(MiniEventType type)              => OnMiniEventChanged?.Invoke(type);
    public static void RaiseBossRushStarted()                                 => OnBossRushStarted?.Invoke();
    public static void RaiseBossRushNextBoss(int index, string bossId)        => OnBossRushNextBoss?.Invoke(index, bossId);
    public static void RaiseBossRushEnded(int score, int medals)              => OnBossRushEnded?.Invoke(score, medals);
    public static void RaiseRealmRaceRewardClaimed(RealmRaceRewardTier tier)  => OnRealmRaceRewardClaimed?.Invoke(tier);
    public static void RaiseBoosterUsed(BoosterType type)                     => OnBoosterUsed?.Invoke(type);
    public static void RaiseBoosterCountsChanged(int b, int r, int rb)        => OnBoosterCountsChanged?.Invoke(b, r, rb);
}
