using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class AudioSettingsController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject audioSettingsPanel;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Audio")]
    public AudioMixer audioMixer;

    private bool isPaused = false;
    [SerializeField] private FirstPersonController playerController; 

    [Header("Input Actions")]
    [Tooltip("Acción de input para pausar el juego.")]
    [SerializeField] private InputActionReference pauseAction;


    void Awake()
    {
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
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);


        audioSettingsPanel.SetActive(false);
    }

    private void OnEnable()
    {
        pauseAction.action.Enable();
        pauseAction.action.performed += TogglePause;
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= TogglePause;
        pauseAction.action.Disable();
    }

    private void TogglePause(InputAction.CallbackContext context)
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
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

        if (playerController != null)
        {
            playerController.SetInputEnabled(false);
        }
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
        audioSettingsPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null)
        {
            playerController.SetInputEnabled(true);
        }
    }
}