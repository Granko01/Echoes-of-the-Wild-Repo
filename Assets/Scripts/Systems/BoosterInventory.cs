using UnityEngine;

public enum BoosterType { Bomb, Rocket, RainbowOrb }

public class BoosterInventory : MonoBehaviour
{
    public static BoosterInventory Instance { get; private set; }

    public int Bombs       => SaveSystem.Data.bombs;
    public int Rockets     => SaveSystem.Data.rockets;
    public int RainbowOrbs => SaveSystem.Data.rainbowOrbs;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Returns false if none left — caller decides whether to show a "buy more" prompt
    public bool UseBooster(BoosterType type)
    {
        switch (type)
        {
            case BoosterType.Bomb:
                if (SaveSystem.Data.bombs <= 0) return false;
                SaveSystem.Data.bombs--;
                break;
            case BoosterType.Rocket:
                if (SaveSystem.Data.rockets <= 0) return false;
                SaveSystem.Data.rockets--;
                break;
            case BoosterType.RainbowOrb:
                if (SaveSystem.Data.rainbowOrbs <= 0) return false;
                SaveSystem.Data.rainbowOrbs--;
                break;
            default:
                return false;
        }

        SaveSystem.Save();
        GameEvents.RaiseBoosterUsed(type);
        GameEvents.RaiseBoosterCountsChanged(Bombs, Rockets, RainbowOrbs);

        // Apply effect in the active Match-3 grid if one is open
        if (Match3Manager.Instance != null && Match3Manager.Instance.IsOpen)
            Match3Manager.Instance.ApplyBooster(type);

        return true;
    }

    public void AddBoosters(int bombs, int rockets, int rainbowOrbs)
    {
        SaveSystem.Data.bombs       += bombs;
        SaveSystem.Data.rockets     += rockets;
        SaveSystem.Data.rainbowOrbs += rainbowOrbs;
        SaveSystem.Save();
        GameEvents.RaiseBoosterCountsChanged(Bombs, Rockets, RainbowOrbs);
    }
}
