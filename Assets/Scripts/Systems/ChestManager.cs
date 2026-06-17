using UnityEngine;

public enum ChestType { Rare, Epic, Legendary }

public class ChestManager : MonoBehaviour
{
    public static ChestManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OpenChest(ChestType type)
    {
        GrantRewards(type);
        GameEvents.RaiseChestOpened(type);
        PassManager.Instance?.AddPassXP(25);
    }

    private void GrantRewards(ChestType type)
    {
        switch (type)
        {
            case ChestType.Rare:
                CurrencyManager.Instance.AddGems(Random.Range(10, 31));
                CurrencyManager.Instance.AddCoins(Random.Range(100, 301));
                SaveSystem.Data.bombs   += Random.Range(1, 4);
                SaveSystem.Data.rockets += Random.Range(1, 4);
                SaveSystem.Save();
                break;

            case ChestType.Epic:
                CurrencyManager.Instance.AddGems(Random.Range(50, 101));
                CurrencyManager.Instance.AddCoins(Random.Range(300, 601));
                SaveSystem.Data.bombs       += Random.Range(3, 8);
                SaveSystem.Data.rockets     += Random.Range(3, 8);
                SaveSystem.Data.rainbowOrbs += Random.Range(1, 4);
                SaveSystem.Save();
                break;

            case ChestType.Legendary:
                CurrencyManager.Instance.AddGems(Random.Range(100, 301));
                CurrencyManager.Instance.AddCoins(Random.Range(500, 1001));
                SaveSystem.Data.bombs       += Random.Range(8, 16);
                SaveSystem.Data.rockets     += Random.Range(8, 16);
                SaveSystem.Data.rainbowOrbs += Random.Range(4, 9);
                SaveSystem.Save();
                break;
        }
    }
}
