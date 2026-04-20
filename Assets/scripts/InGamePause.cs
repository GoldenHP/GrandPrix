using UnityEngine;

public class InGamePause : MonoBehaviour
{
    [Header("UIs")]
    public GameObject RaceUI;
    public GameObject SettingsUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            RaceUI.SetActive(!RaceUI.activeSelf);
            SettingsUI.SetActive(!SettingsUI.activeSelf);
        }    
    }
}
