using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menus")]
    private GameObject Main;
    private GameObject Settings;
    private GameObject GameMode;
    Button[] MainButtons;
    Button[] SettingButtons;
    Button[] GameModeButtons;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Main = transform.GetChild(2).gameObject;
        MainButtons= Main.GetComponentsInChildren<Button>();

        Settings = transform.GetChild(4).gameObject;
        SettingButtons = Settings.GetComponentsInChildren<Button>();

        GameMode = transform.GetChild(3).gameObject;
        GameModeButtons = GameMode.GetComponentsInChildren<Button>();
    }

    public void MainToGame()
    {
        Main.SetActive(false);
        GameMode.SetActive(true);
    }

    public void MainToSettings()
    {
        Main.SetActive(false);
        Settings.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void GameToMain()
    {
        GameMode.SetActive(false);
        Main.SetActive(true);
    }
}
