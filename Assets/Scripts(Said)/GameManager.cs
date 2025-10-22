using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Won, Lost }
    private GameState currentState;

    [Header("Scene Management")]
    [SerializeField] private string winSceneName;
    [SerializeField] private string loseSceneName;

    [Header("Player Componentes")]
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private FirstPersonController playerController;
    [Header("Managers y Events")]
    [SerializeField] private BadgeManager badgeManager;
    [SerializeField] private GameEvent onPlayerDeathEvent;
    [SerializeField] private GameEvent onPlayerSurvivedEvent;
    [SerializeField] private GameEventstring messageEvent;
    [SerializeField] private GameEvent onUncontrolledFireEvent;
    [SerializeField] private UIManager uiManager;
    [Header("Muerte componentes")] 
    [SerializeField] private float tiempoParaMorir = 15f;
    private Coroutine deathCoroutine;
    private Coroutine survivalCoroutine;
    [Header("Level-Specific Logic")]
    [Tooltip("Activa esta casilla si el nivel actual contiene NPCs que deben ser salvados.")]
    [SerializeField] private bool levelHasNPCs = false; 

    [Header("Custom Managers")]
    [Tooltip("Asigna el NPCSaveableManager solo si 'levelHasNPCs' está activado.")]
    [SerializeField] private NPCSaveableManager npcManager; 
    [SerializeField] private UITimerController uiTimerController;

    [Header("End Badges (por nivel)")]
    [SerializeField] private bool awardNoRunBadgeOnWin = false;
    [SerializeField] private string noRunBadgeId = "CalmaBajoPresion";

    [SerializeField] private bool awardWindowSafeBadgeOnWin = false;
    [SerializeField] private string windowSafeBadgeId = "DistanciaSegura";
    public bool IsFireUncontrolled { get; private set; } = false;
    void Start()
    {
        if (badgeManager != null)
        {
            badgeManager.ResetBadges();
        }
        IsFireUncontrolled = false;
    }

    public void HandleUncontrolledFire()
    {
        if (IsFireUncontrolled) return; 

        Debug.Log("GameManager ha sido notificado: ¡El fuego está fuera de control!");
        IsFireUncontrolled = true;

        if (uiManager != null)
        {
            uiManager.UpdateObjectiveText("Evacúa.");
        }
        else
        {
            Debug.LogWarning("GameManager: La referencia a UIManager no está asignada. No se puede actualizar el texto del objetivo.");
        }
    }
    public void IniciarContadorMortal()
    {
        if (currentState != GameState.Playing) return;

        Debug.Log("Contador mortal iniciado en GameManager. El jugador tiene " + tiempoParaMorir + " segundos.");
        if (uiTimerController != null)
        {
            uiTimerController.StartMortalTimer(tiempoParaMorir); 
        }
        if (messageEvent != null)
        {
            messageEvent.Raise("¡El tiempo se agota!");
        }

        if (deathCoroutine == null)
        {
            deathCoroutine = StartCoroutine(ContadorParaMuerte());
        }
    }

    private IEnumerator ContadorParaMuerte()
    {
        yield return new WaitForSeconds(tiempoParaMorir);
        Debug.Log("Se acabó el tiempo del GameManager. El jugador ha muerto.");
        badgeManager.UnlockBadge("GameOverSinTiempo"); 
        HandlePlayerDeath(); 
    }
    public void IniciarContadorSupervivencia(float duration)
    {
        if (survivalCoroutine == null)
        {
            Debug.Log($"Iniciando cuenta atrás para la VICTORIA de {duration} segundos.");
            survivalCoroutine = StartCoroutine(SupervivenciaCoroutine(duration));

            if (uiTimerController != null)
            {

                uiTimerController.StartFireTimer(duration);
            }
        }
    }

    private IEnumerator SupervivenciaCoroutine(float duration)
    {

        yield return new WaitForSeconds(duration);

        Debug.Log("El tiempo extra ha terminado. El jugador sobrevive.");

        HandlePlayerSurvival();
    }
    public void HandlePlayerDeath()
    {
        if (currentState != GameState.Playing) return;

        if (levelHasNPCs && npcManager != null)
        {
            npcManager.EvaluateAtGameEnd();
        }

        currentState = GameState.Lost;
        if (uiTimerController != null)
        {
            uiTimerController.HideTimer(); 
        }
        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
            deathCoroutine = null;
        }

        Debug.Log("GAME OVER: El jugador ha muerto. Iniciando carga de escena de derrota.");
        LevelCompletionData.currentManager = this.badgeManager;
        if (playerInteraction != null) playerInteraction.enabled = false;
        Time.timeScale = 0.2f;
        StartCoroutine(LoadAdditiveScene(loseSceneName));
    }

    public void HandlePlayerSurvival()
    {
        if (currentState != GameState.Playing) return;
        if (levelHasNPCs && npcManager != null)
        {
            npcManager.EvaluateAtGameEnd();
        }
        currentState = GameState.Won;
        if (uiTimerController != null)
        {
            uiTimerController.HideTimer(); 
        }

        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
            deathCoroutine = null;
            Debug.Log("Contador mortal detenido. El jugador ha sobrevivido.");
        }

        Debug.Log("¡VICTORIA!: El jugador ha sobrevivido. Iniciando carga de escena de victoria.");
        LevelCompletionData.currentManager = this.badgeManager;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null)
        {
            playerController.SetInputEnabled(false);
        }

        if (playerInteraction != null) playerInteraction.enabled = false;
        if (badgeManager != null && playerController != null)
        {
            if (awardNoRunBadgeOnWin && !playerController.HasEverRun)
                badgeManager.UnlockBadge(noRunBadgeId);

            if (awardWindowSafeBadgeOnWin && !playerController.WindowInjuryOccurred)
                badgeManager.UnlockBadge(windowSafeBadgeId);
        }
        StartCoroutine(LoadAdditiveScene(winSceneName));
    }

    private IEnumerator LoadAdditiveScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("El nombre de la escena no está asignado en el GameManager. No se puede cargar.");
            yield break;
        }
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        Debug.Log($"Escena '{sceneName}' cargada aditivamente.");
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
      //  Time.timeScale = 1f;
    }
    public void SetGamePaused(bool pause)
    {
        if (pause)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }
}