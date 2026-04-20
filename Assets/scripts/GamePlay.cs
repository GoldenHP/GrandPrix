using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePlay : MonoBehaviour
{
    [Header("Set in Inspector")]
    public GameObject P1Position;
    public GameObject PlayerCar;
    public TMP_Text SectorTime;
    public TMP_Text LapTime;
    public TMP_Text CarSpeed;

    //[Header("Set Dynamically")]
    private float Sector1Time;
    private float Sector2Time;
    private float Sector3Time;
    private float TotalLapTime;
    private int LapCounter;
    private int CarSpeedMilesPerHour;
    private GameObject[] Sectors;
    private string SectorBase = "Sector ";
    private string LapBase = "Lap ";
    private int[] AICarsCheckpoint = { 1, 1, 1, 1, 1, 1, 1};
    

    private bool CheckPoint1Hit;
    private float[] TimeCount = new float[4];

    private RaceTracker tracker;

    void Start()
    {
        SectorTime.text = SectorBase;
        LapTime.text = LapBase;

        CheckPoint1Hit = false;

        LapCounter = 0;

        MapLoad Map = gameObject.GetComponent<MapLoad>();
        PlayerCar = Map.SpawnedPlayer;

        tracker = gameObject.GetComponent<RaceTracker>();
    }

    public void CheckPointHit(int checkpoint)
    {
       if (checkpoint == 1)
        {
            if (CheckPoint1Hit == false) 
            {
                //Begging Lap
                CheckPoint1Hit = true;
                TimeCount[0] = Time.time;
            }
            else if (CheckPoint1Hit == true)
            {
                //Completed Lap
                TimeCount[3] = Time.time;
                Sector3Time = TimeCount[3] - TimeCount[2];
                TotalLapTime = Sector3Time + Sector2Time + Sector1Time;
                ChangeSector(Sector3Time, 3);
                ChangeLap(TotalLapTime);
                ResetLap();

                TimeCount[0] = Time.time;
                tracker.PlayerLapComplete();
            }
        }
       else if(checkpoint == 2)
        {
            //Hit second Checkpoint
            TimeCount[1] = Time.time;
            Sector1Time = TimeCount[1] - TimeCount[0];
            ChangeSector(Sector1Time, 1);
        }
       else if(checkpoint == 3)
        {
            //Hit third Checkpoint
            TimeCount[2] = Time.time;
            Sector2Time = TimeCount[2] - TimeCount[1];
            ChangeSector(Sector2Time, 2);
        }
    }


    private void ResetLap()
    {
        for (int i = 0; i < TimeCount.Length; i++)
        {
            TimeCount[i] = 0;
        }

        TotalLapTime = 0;
        
    }

    private void ChangeSector(float Time, int Sector)
    {
        SectorTime.text = SectorBase + " " + Sector.ToString() + " " + CalMinutes(Time).ToString("F0") 
            + ":" + CalSeconds(Time).ToString("F0");
    }

    private void ChangeLap(float Time)
    {
        LapCounter++;
        LapTime.text = LapBase + " " + LapCounter.ToString() + " " + CalMinutes(Time).ToString("F0")
            + ":" + CalSeconds(Time).ToString("F0");
    }

    private int CalMinutes(float Time)
    {
        int minutes = Convert.ToInt32(Time) / 60;
        return minutes;
    }

    private int CalSeconds(float Time)
    {
        int minutes = CalMinutes(Time);
        int secondsOver = minutes * 60;
        int Seconds = Convert.ToInt32(Time) - secondsOver;
        return Seconds;
    }

    public void CarSpeedChange(Vector3 CarVector)
    {
        int CarSpeedMilesPerHour;
        float InitialVelocityMagnitude = CarVector.magnitude;

        CarSpeedMilesPerHour = Convert.ToInt32(InitialVelocityMagnitude);

        CarSpeed.text = CarSpeedMilesPerHour.ToString();
    }

    public void CarReset()
    {
        ResetLap();
        LapCounter--;
        CheckPoint1Hit = false;
    }

    public bool HasCompletedLap(int lapNumber)
    {
        return LapCounter == lapNumber;
    }
}
