using System;

// Central event bus wiring all GDD implementation triggers.
// OnPulseHit, OnMatchResolve, OnStateChange, OnHealComplete, OnBondLevelUp, OnRealmTransition
public static class GameEvents
{
    public static event Action<EntityController>             OnPulseHit;
    public static event Action<TileType, int>                OnMatchResolve;
    public static event Action<EntityController, EntityState> OnStateChange;
    public static event Action<BiomeArea>                    OnHealComplete;
    public static event Action<EntityType, int>               OnBondLevelUp;
    public static event Action                               OnRealmTransition;

    public static void RaisePulseHit(EntityController entity)              => OnPulseHit?.Invoke(entity);
    public static void RaiseMatchResolve(TileType type, int count)         => OnMatchResolve?.Invoke(type, count);
    public static void RaiseStateChange(EntityController e, EntityState s) => OnStateChange?.Invoke(e, s);
    public static void RaiseHealComplete(BiomeArea area)                   => OnHealComplete?.Invoke(area);
    public static void RaiseBondLevelUp(EntityType type, int level)         => OnBondLevelUp?.Invoke(type, level);
    public static void RaiseRealmTransition()                              => OnRealmTransition?.Invoke();
}
