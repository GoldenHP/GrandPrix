using Unity.Cinemachine;
using UnityEngine;

public class MapLoad : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject Player;
    public GameObject[] AI = new GameObject[7];

    [Header("Lap Details")]
    public int NumberOfLaps;

    [Header("Start Locations")]
    public GameObject[] Locations = new GameObject[8];

    CinemachineCamera CinemachineCamera;

    void Start()
    {
        

        if (GameConfig.SelectedMode == GameConfig.GameMode.VSAI)
        {
            for (int i = 0; i < AI.Length; i++)
            {
                Instantiate(AI[i], Locations[i].transform);
            }

            Instantiate(Player, Locations[7].transform);
            NumberOfLaps = 3;
        }

        if(GameConfig.SelectedMode == GameConfig.GameMode.TIMETRIALS)
        {
            Instantiate(Player, Locations[0].transform);
            //0 = infinite
            NumberOfLaps = 0;
        }

        CinemachineCamera.Follow = Player.transform;
        CinemachineCamera.LookAt = Player.transform;
    }
}
