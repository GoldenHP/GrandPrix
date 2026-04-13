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

    public GameObject SpawnedPlayer;
    public GameObject[] SpawnedAI = new GameObject[7];

    public CinemachineCamera CinemachineCamera;

    void Start()
    {
        for (int i = 0; i < 8; i++) 
        {
            Locations[i] = GameObject.Find("StartingPointP" + (i+1));

            if (Locations[i] == null)
                Debug.LogError($"MapLoad: Could not find 'StartingPointP{i+1}' in scene!");
        }

        if (GameConfig.SelectedMode == GameConfig.GameMode.VSAI)
        {
            for (int i = 0; i < AI.Length; i++)
            {
                SpawnedAI[i] = Instantiate(AI[i], Locations[i].transform.position, Locations[i].transform.rotation);
                //SpawnedAI[i].GetComponent<AiScript>().RacingNumber = i;
                SpawnedAI[i].GetComponent<AiControlCar>().RacingNumber = i;
            }

            SpawnedPlayer = Instantiate(Player, Locations[7].transform.position, Locations[7].transform.rotation);
            SpawnedPlayer.GetComponent<ControlCar>().RacingNumber = 7;
            NumberOfLaps = 3;
        }
        else if (GameConfig.SelectedMode == GameConfig.GameMode.TIMETRIALS)
        {
            SpawnedPlayer = Instantiate(Player, Locations[0].transform.position, Locations[0].transform.rotation);
            NumberOfLaps = 0;
        }

        if (SpawnedPlayer != null && CinemachineCamera != null)
        {
            CinemachineCamera.Follow = SpawnedPlayer.transform;
            CinemachineCamera.LookAt = SpawnedPlayer.transform;
        }
        else
        {
            Debug.LogError("MapLoad: SpawnedPlayer or CinemachineCamera is null!");
        }
    }
}
