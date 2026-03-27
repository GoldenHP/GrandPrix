using UnityEngine;

public class RaceTracker : MonoBehaviour
{
    [Header("Outcomes")]
    [SerializeField] GameObject EndRaceWin;
    [SerializeField] GameObject EndRaceLose;

    [Header("User UI")]
    [SerializeField] GameObject RaceUI;

    private int[] LapTrackerAI;
    private int LapTrackerPlayer;
    private bool[] AICompleteRace;
    private int LapsToWin;

    private bool PlayerWin = true;

    private MapLoad Loader;

    private void Start()
    {
        gameObject.TryGetComponent<MapLoad>(out Loader);
        if(!Loader)
        {
            Debug.LogError("Failed to Grap Map Loader");
        }

        if (GameConfig.SelectedMode == GameConfig.GameMode.VSAI)
        {
            LapTrackerAI = new int[7];
            AICompleteRace = new bool[7];
            for (int i = 0; i < LapTrackerAI.Length; i++) 
            {
                LapTrackerAI[i] = 0;
                AICompleteRace[i] = false;
            }
        }

        if(GameConfig.SelectedMode == GameConfig.GameMode.TIMETRIALS)
        {
            LapTrackerAI = new int[0];
            AICompleteRace = new bool[0];
        }

        LapTrackerPlayer = 0;
        LapsToWin = Loader.NumberOfLaps;
    }

    void PlayerLapComplete()
    {
        LapTrackerPlayer++;
        if (LapTrackerPlayer == LapsToWin)
        {
            //AKA Time Trial
            if(GameConfig.SelectedMode == GameConfig.GameMode.VSAI)
            {
                if (PlayerWin)
                {
                    DisplayWinWindow();
                }
                else if (!PlayerWin)
                {
                    DisplayLostWindow();
                }
            }
        }
    }

    void AILapComplete(int RacingNumber)
    {
        if (!AICompleteRace[RacingNumber])
            LapTrackerAI[RacingNumber]++;

        if (LapTrackerAI[RacingNumber] == 3)
        {
            AICompleteRace[RacingNumber] = true;
            if (LapTrackerPlayer != 3)
            {
                PlayerWin = false;
            }
        }
    }

    void DisplayWinWindow()
    {
        RaceUI.SetActive(false);
        EndRaceWin.SetActive(true);
    }

    void DisplayLostWindow()
    {
        RaceUI.SetActive(false);
        EndRaceLose.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        AiScript AI;
        ControlCar Player;
        GamePlay gPlay;

        other.TryGetComponent<AiScript>(out AI);
        other.TryGetComponent<ControlCar>(out Player);
        gameObject.TryGetComponent<GamePlay>(out gPlay);

        if(AI != null)
        {
            AILapComplete(AI.RacingNumber);
        }

        if(Player != null)
        {
            if(gPlay != null && gPlay.HasCompletedLap(LapTrackerPlayer))
                PlayerLapComplete();
        }
    }

}
