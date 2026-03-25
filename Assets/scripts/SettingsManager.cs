using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    //Graphics Quality
    public int ResolutionIndex { get; private set; }
    public int QualityIndex { get; private set; }
    public bool IsFullscreen { get; private set; }
    public int TargetFPS { get; private set; }

    //Audio
    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float SFXVloume { get; private set; }


    //Player Prefrence Keys
    const string K_RES = "setting_resolution";
    const string K_QUALITY = "setting_quality";
    const string K_FULLSCREEN = "setting_fullscreen";
    const string K_FPS = "setting_fps";
    const string K_MASTER = "setting_master_vol";
    const string K_MUSIC = "setting_music_vol";
    const string K_SFX = "setting_sfx_vol";

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAll();
    }

    public void SetResolution(int index)
    {
        ResolutionIndex = index;
        Resolution[] resolutions = Screen.resolutions;
        if(index >= 0 && index <= resolutions.Length)
        {
            Screen.SetResolution(resolutions[index].width, resolutions[index].height, IsFullscreen);
        }
        PlayerPrefs.SetInt(K_RES, index);
    }

    public void SetQuality(int index)
    {
        QualityIndex = index;
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt(K_QUALITY, index);
    }

    public void SetFullscreen(bool fullscreen)
    {
        IsFullscreen = fullscreen;
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt(K_FULLSCREEN, fullscreen ? 1 : 0);
    }

    public void SetTargetFPS(int fps)
    {
        TargetFPS = fps;
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt(K_FPS, fps);
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = value;
        ApplyMixerVolume("MasterVolume", value);
        PlayerPrefs.SetFloat(K_MASTER, value);
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = value;
        ApplyMixerVolume("MusicVolume", value);
        PlayerPrefs.SetFloat(K_MUSIC, value);
    }

    public void SetSFXVolume(float value)
    {
        SFXVloume = value;
        ApplyMixerVolume("SFXVolume", value);
        PlayerPrefs.SetFloat(K_SFX, value);
    }

    void LoadAll()
    {
        // Graphics
        SetResolution(PlayerPrefs.GetInt(K_RES, 0));
        SetQuality(PlayerPrefs.GetInt(K_QUALITY, QualitySettings.GetQualityLevel()));
        SetFullscreen(PlayerPrefs.GetInt(K_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1);
        SetTargetFPS(PlayerPrefs.GetInt(K_FPS, 60));


        SetMasterVolume(PlayerPrefs.GetFloat(K_MASTER, 1f));
        SetMusicVolume(PlayerPrefs.GetFloat(K_MUSIC, 1f));
        SetSFXVolume(PlayerPrefs.GetFloat(K_SFX, 1f));
    }

    void ApplyMixerVolume(string parameterName, float linearValue)
    {
        if (audioMixer == null) return;

        float db = linearValue > 0.0001f ? Mathf.Log10(linearValue) * 20f : -80f;
        audioMixer.SetFloat(parameterName, db);
    }
}
    