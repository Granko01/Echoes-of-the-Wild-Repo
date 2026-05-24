using UnityEngine;

public enum Act { Prologue, Act1_Jungle, Act2_Mountain, Act3_River, Act4_Zoo, Act5_Spirit, Epilogue }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Act _startAct = Act.Prologue;

    public Act CurrentAct { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SaveSystem.LoadAct(out Act saved);
        SetAct(saved);
    }

    public void AdvanceAct()
    {
        if ((int)CurrentAct < (int)Act.Epilogue)
            SetAct((Act)((int)CurrentAct + 1));
    }

    private void SetAct(Act act)
    {
        CurrentAct = act;
        ConfigureSystemsForAct(act);
        if (BondSystem.Instance != null)
            SaveSystem.Save(act, BondSystem.Instance);
        Debug.Log($"[GameManager] Entered {act}");
    }

    private void ConfigureSystemsForAct(Act act)
    {
        // Grid size and tile variety scale with act complexity per GDD progression table
        if (Match3Manager.Instance != null)
        {
            switch (act)
            {
                case Act.Prologue:
                case Act.Act1_Jungle:
                    Match3Manager.Instance.SetGridSize(3, 3);
                    Match3Manager.Instance.SetAvailableTileTypes(2); // Calm, Trust
                    break;
                case Act.Act2_Mountain:
                case Act.Act3_River:
                    Match3Manager.Instance.SetGridSize(4, 4);
                    Match3Manager.Instance.SetAvailableTileTypes(3); // + Bond
                    break;
                case Act.Act4_Zoo:
                case Act.Act5_Spirit:
                    Match3Manager.Instance.SetGridSize(5, 5);
                    Match3Manager.Instance.SetAvailableTileTypes(4); // + Fear
                    break;
            }
        }

        if (act == Act.Act5_Spirit)
            GameEvents.RaiseRealmTransition();
    }
}
