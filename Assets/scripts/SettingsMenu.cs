using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("Tabs")]
    Button GraphicsTabButton;
    Button AudioTabButton;
    GameObject GraphicsPanel;
    GameObject AudioPanel;

    [Header("Graphics ")]
    TMP_Dropdown ResolutionDropDown;
    TMP_Dropdown QualityDropDown;
    Toggle FullscreenToggle;
    TMP_Dropdown FPSDropDown;

    [Header("Audio")]
    Slider MasterSlider;
    Slider MusicSlider;
    Slider SFXSlider;
    TMP_Text MasterValueLabel;
    TMP_Text MusicValueLabel;
    TMP_Text SFXValueLabel;

    [Header("Footer")]
    Button ApplyButton;
    Button CloseButton;

    Resolution[] _resolutions;

    private void Awake()
    {
         BuildResolutionDropDown();
         BuildQualityDropDown();
         BuildFPSDropDown();
         //RegisterListeners();
    }

    private void OnEnable()
    {
        //RefreshUIFromManager();
        //ShowTab(graphicsPanel);
    }

    public void ShowGraphicsTab()
    {
        ShowTab(GraphicsPanel);
    }

    public void ShowAudioTab()
    {
        ShowTab(AudioPanel);
    }

    void ShowTab(GameObject activePanel)
    {
        GraphicsPanel.SetActive(activePanel == GraphicsPanel);
        AudioPanel.SetActive(activePanel == AudioPanel);
    }

    void BuildResolutionDropDown()
    {
        _resolutions = Screen.resolutions;
        ResolutionDropDown.ClearOptions();

        var options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < _resolutions.Length; i++)
        {
            options.Add($"{_resolutions[i].width} x {_resolutions[i].height} @ {_resolutions[i].refreshRateRatio.value:F0}Hz");

            if (_resolutions[i].width == Screen.currentResolution.width && _resolutions[i].height == Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }

        ResolutionDropDown.AddOptions(options);
        ResolutionDropDown.value = currentIndex;
        ResolutionDropDown.RefreshShownValue();
    }

    void BuildQualityDropDown()
    {
        QualityDropDown.ClearOptions();
        QualityDropDown.AddOptions(new List<string>(QualitySettings.names));
    }

    void BuildFPSDropDown()
    {
        FPSDropDown.ClearOptions();
        FPSDropDown.AddOptions(new List<string> { "30", "60", "120", "144", "Unlimited" });
    }

    void RefreshUIFromManagers()
    {
        var s = SettingsManager.Instance;
        if (s == null) return;

        ResolutionDropDown.SetValueWithoutNotify(s.ResolutionIndex);
        QualityDropDown.SetValueWithoutNotify(s.QualityIndex);
        FullscreenToggle.SetIsOnWithoutNotify(s.IsFullscreen);
        //FPSDropDown.SetValueWithoutNotify(FPSToDropdownIndex(s));

        MasterSlider.SetValueWithoutNotify(s.MasterVolume);
        MusicSlider.SetValueWithoutNotify(s.MusicVolume);
        SFXSlider.SetValueWithoutNotify(s.SFXVloume);

        //UpdateVolumeLabels();
    }

    void RegisterEventListeners()
    {
        // Tabs
        GraphicsTabButton.onClick.AddListener(ShowGraphicsTab);
        AudioTabButton.onClick.AddListener(ShowAudioTab);

        // Graphics — live preview as user drags/changes
        ResolutionDropDown.onValueChanged.AddListener(i => SettingsManager.Instance?.SetResolution(i));

        QualityDropDown.onValueChanged.AddListener(i => SettingsManager.Instance?.SetQuality(i));

        FullscreenToggle.onValueChanged.AddListener(v => SettingsManager.Instance?.SetFullscreen(v));

        //FPSDropDown.onValueChanged.AddListener(i => SettingsManager.Instance?.SetTargetFPS(DropdownIndexToFPS(i)));

        // Audio — live preview
        //MasterSlider.onValueChanged.AddListener(v => { SettingsManager.Instance?.SetMasterVolume(v); UpdateVolumeLabels();});
        
        //MusicSlider.onValueChanged.AddListener(v => { SettingsManager.Instance?.SetMusicVolume(v); UpdateVolumeLabels();});

        //SFXSlider.onValueChanged.AddListener(v => { SettingsManager.Instance?.SetSFXVolume(v); UpdateVolumeLabels();});

        // Footer
        //ApplyButton.onClick.AddListener(OnApply);
        //CloseButton.onClick.AddListener(OnClose);
    }

    void OnApply()
    {

    }

    void OnClose()
    {

    }

    void UpdateVolumeLabels()
    {

    }

    static int FPSToDropdownIndex(int fps) => fps switch
    {

    };

    static int DropdownIndexToFPS(int index) => index switch 
    {
    
    };
}
