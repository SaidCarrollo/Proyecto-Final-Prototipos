using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private const string MasterVolumeKey = "MasterVolume"; 
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    void Start()
    {
        masterSlider.value = AudioManager.Instance.GetVolume(MasterVolumeKey); 
        musicSlider.value = AudioManager.Instance.GetVolume(MusicVolumeKey);
        sfxSlider.value = AudioManager.Instance.GetVolume(SFXVolumeKey);

        masterSlider.onValueChanged.AddListener(SetMasterVolume); 
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMasterVolume(float value)
    {
        AudioManager.Instance.SetVolume(MasterVolumeKey, value, true);
    }

    public void SetMusicVolume(float value)
    {
        AudioManager.Instance.SetVolume(MusicVolumeKey, value, true);
    }

    public void SetSFXVolume(float value)
    {
        AudioManager.Instance.SetVolume(SFXVolumeKey, value, true);
    }
}