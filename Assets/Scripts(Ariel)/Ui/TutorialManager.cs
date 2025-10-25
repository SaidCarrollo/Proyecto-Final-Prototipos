using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

// Si usas el nuevo Input System, habilita esta using:
using UnityEngine.InputSystem.UI;

[DisallowMultipleComponent]
public class TutorialManager : MonoBehaviour
{
    [Header("Paneles del tutorial")]
    [Tooltip("Lista de paneles (CanvasGroup) del tutorial en orden.")]
    [SerializeField] private CanvasGroup[] panelesTutorial;

    [Header("Canvas del tutorial")]
    [Tooltip("CanvasGroup raíz del tutorial para hacer fade in/out general.")]
    [SerializeField] private CanvasGroup tutorialCanvasGroup;

    [Header("Preferencias")]
    [Tooltip("Si el toggle está ON al cerrar, no volver a mostrar el tutorial.")]
    [SerializeField] private Toggle noMostrarDeNuevoToggle;
    [Tooltip("Clave para PlayerPrefs que marca si el tutorial ya fue visto.")]
    [SerializeField] private string tutorialKey = "TutorialNivel1Visto";

    [Header("Referencias")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private FirstPersonController playerController;

    // Nota: usa Behaviour para poder arrastrar cualquier script (ej: 'PlayerIntereaction' con typo).
    [SerializeField] private Behaviour playerInteraction;

    [Header("Configuración")]
    [SerializeField, Tooltip("Desactiva inputs del jugador al inicio si el tutorial no se ha visto.")]
    private bool desactivarInputsAlIniciar = true;
    [SerializeField, Tooltip("Duración del fade (s) para el canvas del tutorial.")]
    private float fadeDuration = 0.35f;
    [SerializeField, Tooltip("Duración de fade por panel (s).")]
    private float panelFadeDuration = 0.25f;

    [Header("Infra de UI (auto)")]
    [Tooltip("Canvas que contiene el tutorial (si se deja vacío, se busca con el CanvasGroup).")]
    [SerializeField] private Canvas tutorialCanvas;
    [Tooltip("Orden alto para dejar el tutorial por encima de overlays (transiciones, pause, etc.)")]
    [SerializeField] private int tutorialSortingOrder = 5000;
    [Tooltip("Seleccionable inicial para navegación y confirmación de foco UI.")]
    [SerializeField] private Selectable firstSelectable;

    private int panelActual = 0;
    private bool tutorialVisto = false;
    private EventSystem eventSystem;
    private BaseInputModule inputModule;
    private GraphicRaycaster raycaster;

    private void Start()
    {
        tutorialVisto = PlayerPrefs.GetInt(tutorialKey, 0) == 1;

        EnsureUIInfrastructure();   // 🔧 Se asegura EventSystem, Raycaster y sorting del Canvas
        InitPanelsImmediate();

        if (!tutorialVisto)
        {
            if (desactivarInputsAlIniciar)
                SetPlayerInputActive(false);

            MostrarTutorial(true);
            SetCursorForTutorial(true);

            panelActual = 0;
            MostrarPanelActual();
            FocusFirstSelectable();
        }
        else
        {
            OcultarTutorialInmediato();
            SetPlayerInputActive(true);
            SetCursorForTutorial(false);
            gameObject.SetActive(false);
        }
    }

    // ======================
    //  Botones UI
    // ======================
    public void BtnSiguiente()
    {
        if (panelesTutorial == null || panelesTutorial.Length == 0) return;
        int nuevo = Mathf.Clamp(panelActual + 1, 0, panelesTutorial.Length - 1);
        if (nuevo != panelActual)
        {
            panelActual = nuevo;
            MostrarPanelActual();
        }
    }

    public void BtnAnterior()
    {
        if (panelesTutorial == null || panelesTutorial.Length == 0) return;
        int nuevo = Mathf.Clamp(panelActual - 1, 0, panelesTutorial.Length - 1);
        if (nuevo != panelActual)
        {
            panelActual = nuevo;
            MostrarPanelActual();
        }
    }

    public void BtnCerrar() => CerrarTutorial();

    public void IrAPanel(int index)
    {
        if (panelesTutorial == null || panelesTutorial.Length == 0) return;
        index = Mathf.Clamp(index, 0, panelesTutorial.Length - 1);
        if (index != panelActual)
        {
            panelActual = index;
            MostrarPanelActual();
        }
    }

    // ======================
    //  Núcleo del tutorial
    // ======================
    private void MostrarPanelActual()
    {
        if (panelesTutorial == null) return;

        for (int i = 0; i < panelesTutorial.Length; i++)
        {
            var cg = panelesTutorial[i];
            if (cg == null) continue;

            bool activo = (i == panelActual);
            cg.DOKill();

            // ⏱️ Tiempo real (independiente de Time.timeScale)
            cg.DOFade(activo ? 1f : 0f, panelFadeDuration)
              .SetUpdate(true);

            cg.interactable = activo;
            cg.blocksRaycasts = activo;
        }
    }

    private void MostrarTutorial(bool mostrar)
    {
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.DOKill();
            tutorialCanvasGroup.interactable = mostrar;
            tutorialCanvasGroup.blocksRaycasts = mostrar;
            tutorialCanvasGroup.DOFade(mostrar ? 1f : 0f, fadeDuration).SetUpdate(true);
        }

        if (gameManager != null)
            gameManager.SetGamePaused(mostrar); // pausa/reanuda el juego (Time.timeScale)
    }

    private void CerrarTutorial()
    {
        if (noMostrarDeNuevoToggle != null && noMostrarDeNuevoToggle.isOn)
        {
            PlayerPrefs.SetInt(tutorialKey, 1);
            PlayerPrefs.Save();
        }

        MostrarTutorial(false);
        SetPlayerInputActive(true);
        SetCursorForTutorial(false);

        if (tutorialCanvasGroup != null)
        {
            DOVirtual.DelayedCall(fadeDuration, () =>
            {
                gameObject.SetActive(false);
            }).SetUpdate(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // ======================
    //  Helpers
    // ======================
    private void InitPanelsImmediate()
    {
        if (panelesTutorial != null)
        {
            for (int i = 0; i < panelesTutorial.Length; i++)
            {
                var cg = panelesTutorial[i];
                if (cg == null) continue;
                bool activo = (i == 0 && !tutorialVisto);
                cg.DOKill(true);
                cg.alpha = activo ? 1f : 0f;
                cg.interactable = activo;
                cg.blocksRaycasts = activo;
            }
        }

        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.DOKill(true);
            bool visible = !tutorialVisto;
            tutorialCanvasGroup.alpha = visible ? 1f : 0f;
            tutorialCanvasGroup.interactable = visible;
            tutorialCanvasGroup.blocksRaycasts = visible;
        }
    }

    private void OcultarTutorialInmediato()
    {
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.DOKill(true);
            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.interactable = false;
            tutorialCanvasGroup.blocksRaycasts = false;
        }

        if (panelesTutorial != null)
        {
            foreach (var cg in panelesTutorial)
            {
                if (cg == null) continue;
                cg.DOKill(true);
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
        }
    }

    private void SetPlayerInputActive(bool activo)
    {
        if (playerController != null)
            playerController.SetInputEnabled(activo);

        if (playerInteraction != null)
            playerInteraction.enabled = activo;
    }

    private void SetCursorForTutorial(bool tutorialActivo)
    {
        if (tutorialActivo)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void FocusFirstSelectable()
    {
        if (firstSelectable == null) return;

        // Asegura que el EventSystem esté listo y enfoca un botón inicial
        if (eventSystem == null) eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(firstSelectable.gameObject);
        }
    }

    private void EnsureUIInfrastructure()
    {
        // 1) Canvas + Raycaster + Sorting alto
        if (tutorialCanvas == null && tutorialCanvasGroup != null)
            tutorialCanvas = tutorialCanvasGroup.GetComponentInParent<Canvas>();

        if (tutorialCanvas != null)
        {
            tutorialCanvas.overrideSorting = true;   // lo ponemos por encima
            tutorialCanvas.sortingOrder = tutorialSortingOrder;

            if (!tutorialCanvas.TryGetComponent<GraphicRaycaster>(out raycaster))
                raycaster = tutorialCanvas.gameObject.AddComponent<GraphicRaycaster>();
        }
        else
        {
            Debug.LogWarning("[TutorialManager] No se encontró Canvas del tutorial. Asigna 'tutorialCanvas' o 'tutorialCanvasGroup'.");
        }

        // 2) EventSystem + Input Module
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            var go = new GameObject("EventSystem (Auto)");
            eventSystem = go.AddComponent<EventSystem>();

            // Intentar usar el módulo del nuevo Input System si está disponible
            try
            {
                inputModule = go.AddComponent<InputSystemUIInputModule>();
            }
            catch
            {
                // Fallback al Standalone (viejo input) si el paquete no está o modo antiguo
                inputModule = go.AddComponent<StandaloneInputModule>();
            }
        }
        else
        {
            inputModule = eventSystem.currentInputModule;

            // Si no hay módulo, añadimos uno
            if (inputModule == null)
            {
                try
                {
                    inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                }
                catch
                {
                    inputModule = eventSystem.gameObject.AddComponent<StandaloneInputModule>();
                }
            }
        }
    }

    private void OnDisable()
    {
        if (tutorialCanvasGroup != null) DOTween.Kill(tutorialCanvasGroup);
        if (panelesTutorial != null)
        {
            foreach (var cg in panelesTutorial)
                if (cg != null) DOTween.Kill(cg);
        }
    }
}

