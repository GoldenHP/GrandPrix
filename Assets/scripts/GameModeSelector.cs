using Unity.VectorGraphics;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameModeSelector : MonoBehaviour
{
    private enum Maps
    {
        OVALOUT, OVALCITY, F1, F8, COASTAL
    }

    private Maps SelectedMap;

    private UnityEngine.SceneManagement.Scene OvalOut;
    private UnityEngine.SceneManagement.Scene OvalCity;
    private UnityEngine.SceneManagement.Scene Formula1;
    private UnityEngine.SceneManagement.Scene Formula8;
    private UnityEngine.SceneManagement.Scene Coastal;
    private UnityEngine.SceneManagement.Scene MainMenu;

    private GameObject GameMode;
    private GameObject MapMenu;

    private Button[] GameButtons;
    private Button[] MapButtons;

    //Process goes
    /*
        User selects Game Mode
        User selects Map
        Game loads ProperMap with Proper Game config. 
     */

    private void Start()
    {
        //Manager = GetComponent<SceneManager>();
        OvalOut = SceneManager.GetSceneByBuildIndex(3);
        OvalCity = SceneManager.GetSceneByBuildIndex(1);
        Formula1 = SceneManager.GetSceneByBuildIndex(2);
        Formula8 = SceneManager.GetSceneByBuildIndex(4);
        Coastal = SceneManager.GetSceneByBuildIndex(5);
        MainMenu = SceneManager.GetSceneByBuildIndex(0);

        GameMode = transform.GetChild(3).gameObject;
        MapMenu = transform.GetChild(5).gameObject;

        GameButtons = GameMode.GetComponentsInChildren<Button>();
        MapButtons = MapMenu.GetComponentsInChildren<Button>();
    }

    public void AISelected()
    {
        GameConfig.SelectedMode = GameConfig.GameMode.VSAI;
        GameMode.SetActive(false);
        MapMenu.SetActive(true);
    }

    public void TimeTrialsSelected()
    {
        GameConfig.SelectedMode = GameConfig.GameMode.TIMETRIALS;
        GameMode.SetActive(false);
        MapMenu.SetActive(true);
    }

    public void Outskirts()
    {
        SelectedMap = Maps.OVALOUT;
        LoadGame();
    }

    public void City()
    {
        SelectedMap = Maps.OVALCITY;
        LoadGame();
    }

    public void F1()
    {
        SelectedMap = Maps.F1;
        LoadGame();
    }

    public void F8()
    {
        SelectedMap = Maps.F8;
        LoadGame();
    }

    public void Coast()
    {
        SelectedMap = Maps.COASTAL;
        LoadGame();
    }

    public void LoadGame()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        switch (SelectedMap) 
        {
            case Maps.OVALOUT:
                SceneManager.LoadSceneAsync(OvalOut.buildIndex);
                SceneManager.SetActiveScene(OvalOut);               
                break;
            case Maps.COASTAL:
                SceneManager.LoadSceneAsync(Coastal.buildIndex);
                SceneManager.SetActiveScene(Coastal);
                break;
            case Maps.F1:
                SceneManager.LoadSceneAsync(Formula1.buildIndex);
                SceneManager.SetActiveScene(Formula1);
                break;
            case Maps.F8:
                SceneManager.LoadSceneAsync(Formula8.buildIndex);
                SceneManager.SetActiveScene(Formula8);
                break;
            case Maps.OVALCITY:
                SceneManager.LoadSceneAsync(OvalCity.buildIndex);
                SceneManager.SetActiveScene(OvalCity);
                break;
        }
        
    }

    public void ExitMapSelection()
    {
        GameMode.SetActive(true);
        MapMenu.SetActive(false);
    }
}
