using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    [Header("Paneles del tutorial")]
    public CanvasGroup[] panelesTutorial;

    [Header("Preferencias")]
    public Toggle noMostrarDeNuevoToggle;
    public string tutorialKey = "TutorialNivel1Visto";

    [Header("Canvas del tutorial")]
    public CanvasGroup tutorialCanvasGroup;

    [Header("Control de jugador")]
    [Tooltip("Si está activo, desactiva la entrada del jugador mientras se muestra el tutorial.")]
    public bool desactivarInputsAlIniciar = true;
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private PlayerInteraction playerInteraction;

    [Header("Coordinación con GameManager")]
    [SerializeField] private GameManager gameManager;

    private int panelActual = 0;
    private bool tutorialVisto = false;

    void Awake()
    {
        tutorialVisto = PlayerPrefs.GetInt(tutorialKey, 0) == 1;

        if (tutorialVisto)
        {
            gameObject.SetActive(false);
            return;
        }

        if (tutorialCanvasGroup == null)
        {
            tutorialCanvasGroup = GetComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        if (tutorialVisto) return;

        if (desactivarInputsAlIniciar)
            SetPlayerInputActive(false);

        MostrarTutorial(true);
        MostrarPanelActual();
    }

    private void MostrarPanelActual()
    {
        for (int i = 0; i < panelesTutorial.Length; i++)
        {
            bool activo = (i == panelActual);
            panelesTutorial[i].DOFade(activo ? 1f : 0f, 0.5f).SetUpdate(true);
            panelesTutorial[i].interactable = activo;
            panelesTutorial[i].blocksRaycasts = activo;
        }
    }

    public void SiguientePanel()
    {
        if (panelActual < panelesTutorial.Length - 1)
        {
            panelActual++;
            MostrarPanelActual();
        }
    }

    public void CerrarTutorial()
    {
        if (noMostrarDeNuevoToggle != null && noMostrarDeNuevoToggle.isOn)
        {
            PlayerPrefs.SetInt(tutorialKey, 1);
        }

        MostrarTutorial(false);
        SetPlayerInputActive(true);
    }

    private void MostrarTutorial(bool mostrar)
    {
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.DOFade(mostrar ? 1f : 0f, 0.5f).SetUpdate(true);
            tutorialCanvasGroup.interactable = mostrar;
            tutorialCanvasGroup.blocksRaycasts = mostrar;
        }

        if (gameManager != null)
            gameManager.SetGamePaused(mostrar);
    }

    public void MostrarTutorialManual()
    {
        PlayerPrefs.SetInt(tutorialKey, 0);
        PlayerPrefs.Save();

        gameObject.SetActive(true);
        panelActual = 0;
        MostrarPanelActual();
        SetPlayerInputActive(false);
        MostrarTutorial(true);
    }

    private void SetPlayerInputActive(bool activo)
    {
        if (playerController != null)
            playerController.SetInputEnabled(activo);

        if (playerInteraction != null)
            playerInteraction.enabled = activo;
    }
}




