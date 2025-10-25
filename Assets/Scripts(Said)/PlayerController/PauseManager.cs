using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    // <<< NUEVO: raíz del HUD móvil (el GO que contiene sticks y botones)
    [SerializeField] private GameObject mobileControlsRoot;

    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction;

    [Header("Player")]
    [SerializeField] private FirstPersonController playerController;

    private bool isPaused = false;

    // guardamos si el HUD móvil estaba activo antes de pausar
    private bool mobileControlsWasActive = false;

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += TogglePause;
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= TogglePause;
            pauseAction.action.Disable();
        }
    }

    private void TogglePause(InputAction.CallbackContext _)
    {
        if (!isPaused) PauseGame();
        else ResumeGame();
    }

    // Llama a este desde un botón de la UI si quieres (Resume / Pause)
    public void TogglePauseFromUI()
    {
        if (!isPaused) PauseGame();
        else ResumeGame();
    }

    private void PauseGame()
    {
        isPaused = true;

        // Oculta HUD móvil y recuerda su estado anterior
        if (mobileControlsRoot != null)
        {
            mobileControlsWasActive = mobileControlsRoot.activeSelf;
            mobileControlsRoot.SetActive(false);
        }

        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);

        // Mostrar cursor solo si hay mouse (PC)
        if (Mouse.current != null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (playerController != null)
            playerController.SetInputEnabled(false);
    }

    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;
        // Si no animas el panel de pausa, descomenta:
        // if (pausePanel != null) pausePanel.SetActive(false);

        // Restaura el HUD móvil a como estaba antes
        if (mobileControlsRoot != null)
            mobileControlsRoot.SetActive(mobileControlsWasActive);

        // Re-bloquea cursor solo en PC
        if (Mouse.current != null)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (playerController != null)
            playerController.SetInputEnabled(true);
    }
}
