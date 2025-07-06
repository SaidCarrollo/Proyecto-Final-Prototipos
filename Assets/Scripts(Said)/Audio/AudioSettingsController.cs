using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start() 
    {
        musicSlider.value = AudioManager.Instance.GetVolume("MusicVolume");
        sfxSlider.value = AudioManager.Instance.GetVolume("SFXVolume");
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float value)
    {

        AudioManager.Instance.SetVolume("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        AudioManager.Instance.SetVolume("SFXVolume", value);
    }
}