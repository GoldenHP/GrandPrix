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

    private const int IDX_OVALCITY = 0;
    private const int IDX_FORMULA1 = 1;
    private const int IDX_OVALOUT = 2;
    private const int IDX_FORMULA8 = 3;
    private const int IDX_COASTAL = 4;
    private const int IDX_MAINMENU = 5;


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
        int index;
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        switch (SelectedMap) 
        {
            case Maps.OVALOUT:
                index = IDX_OVALOUT;              
                break;
            case Maps.COASTAL:
                index = IDX_COASTAL;
                break;
            case Maps.F1:
                index = IDX_FORMULA1;
                break;
            case Maps.F8:
                index = IDX_FORMULA8;
                break;
            case Maps.OVALCITY:
                index = IDX_OVALCITY;
                break;
            default:
                index = -1;
                break;
        }

        if (index == -1)
        {
            Debug.LogError("Invalid Map selected!");
            return;
        }

        SceneManager.LoadScene(index);
    }

    public void ExitMapSelection()
    {
        GameMode.SetActive(true);
        MapMenu.SetActive(false);
    }
}
