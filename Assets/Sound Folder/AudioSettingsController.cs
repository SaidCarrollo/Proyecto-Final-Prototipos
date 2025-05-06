using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject audioSettingsPanel;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Audio")]
    public AudioMixer audioMixer;

    private bool isPaused = false;

    void Start()
    {
        // Inicializar sliders con los valores actuales del AudioMixer
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

        // Agregar listeners a los sliders
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // Ocultar el panel al inicio
        audioSettingsPanel.SetActive(false);

        // Asegurar que el cursor esté oculto y bloqueado al inicio
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Alternar la visibilidad del panel al presionar la tecla Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", value);
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
        audioSettingsPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPaused = true;
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
        audioSettingsPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;
    }
}