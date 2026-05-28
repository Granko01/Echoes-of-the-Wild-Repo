using UnityEngine;

// Tracks Echo Fragments collected by the player and handles weapon upgrades.
// Call AddFragments when player picks up a CourageFragment or reward.
// Call TryUpgrade to spend fragments and level up a weapon.
public class WeaponUpgradeSystem : MonoBehaviour
{
    public static WeaponUpgradeSystem Instance { get; private set; }

    private int _echoFragments;

    public int EchoFragments => _echoFragments;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddFragments(int amount)
    {
        _echoFragments += amount;
        GameEvents.RaiseFragmentCollected(_echoFragments);
    }

    // Returns true if the upgrade succeeded.
    public bool TryUpgrade(WeaponData data)
    {
        if (data == null || data.currentLevel >= 3) return false;
        int cost = data.UpgradeCostForNext();
        if (_echoFragments < cost) return false;

        _echoFragments  -= cost;
        data.currentLevel++;
        GameEvents.RaiseWeaponUpgraded(data);
        GameEvents.RaiseFragmentCollected(_echoFragments);
        return true;
    }
}
