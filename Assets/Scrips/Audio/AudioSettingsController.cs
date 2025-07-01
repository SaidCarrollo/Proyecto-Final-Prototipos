using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Audio")]
    public AudioMixer audioMixer;

    void Awake()
    {
        // Inicializa los sliders con los valores actuales del mixer
        float musicVolume;
        if (audioMixer.GetFloat("MusicVolume", out musicVolume))
        {
            musicSlider.value = musicVolume;
        }

        float sfxVolume;
        if (audioMixer.GetFloat("SFXVolume", out sfxVolume))
        {
            sfxSlider.value = sfxVolume;
        }

        // Añade los listeners para cuando cambien los valores
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", value);
    }
}