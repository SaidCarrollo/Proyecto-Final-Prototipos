using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Won, Lost }
    private GameState currentState = GameState.Playing;

    [Header("Scene Management (Sistema As�ncrono)")]
    [Tooltip("Escena a cargar cuando el jugador GANA.")]
    [SerializeField] private SceneDefinitionSO winScene;
    [Tooltip("Escena a cargar cuando el jugador PIERDE.")]
    [SerializeField] private SceneDefinitionSO loseScene;
    [Header("Nivel actual (para Retry)")]
    [SerializeField] private SceneDefinitionSO thisLevel;          // La SceneDefinition de ESTA escena de gameplay
    [SerializeField] private LastPlayedLevelSO lastPlayedLevel;    // Asset compartido para recordar el �ltimo nivel
    [SerializeField] private bool rememberLevelOnAwake = true;     // Por si quieres desactivarlo en alguna escena rara

    [Tooltip("Canal para solicitar pre-carga/carga de escenas (as�ncrono).")]
    [SerializeField] private SceneLoadEventChannelSO sceneLoadChannel;
    [Tooltip("Canal para ACTIVAR la escena pre-cargada (dispara el fade y el allowSceneActivation).")]
    [SerializeField] private SceneChannelSO activatePreloadedSceneChannel;

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
    [SerializeField] private bool levelHasNPCs = false;
    [SerializeField] private NPCSaveableManager npcManager;
    [SerializeField] private UITimerController uiTimerController;

    [Header("End Badges (por nivel)")]
    [SerializeField] private bool awardNoRunBadgeOnWin = false;
    [SerializeField] private string noRunBadgeId = "CalmaBajoPresion";
    [SerializeField] private bool awardWindowSafeBadgeOnWin = false;
    [SerializeField] private string windowSafeBadgeId = "DistanciaSegura";
    [Header("Badge 'Sin Da�o'")]
    [SerializeField] private bool awardIdealExitBadgeOnWin = true;
    [SerializeField] private string idealExitBadgeId = "Sin da�os";
    public bool IsFireUncontrolled { get; private set; } = false;
    [Header("UI Secundarias")]
    [SerializeField] private ObjectiveChecklistUI objectiveChecklistUI;
    void Start()
    {
        if (rememberLevelOnAwake && lastPlayedLevel != null && thisLevel != null)
        {
            lastPlayedLevel.lastLevel = thisLevel;
            Debug.Log($"[GameManager] Recordado nivel: {thisLevel.name}");
        }
        if (badgeManager != null)
            badgeManager.ResetBadges();

        IsFireUncontrolled = false;
        currentState = GameState.Playing;
    }

    public void HandleUncontrolledFire()
    {
        if (IsFireUncontrolled) return;

        Debug.Log("GameManager: �El fuego est� fuera de control!");
        IsFireUncontrolled = true;

        if (uiManager != null)
            uiManager.UpdateObjectiveText("Evac�a.");
        else
            Debug.LogWarning("GameManager: UIManager no asignado; no se puede actualizar el objetivo.");
    }

    public void IniciarContadorMortal()
    {
        if (currentState != GameState.Playing) return;

        Debug.Log($"Contador mortal iniciado ({tiempoParaMorir}s).");
        uiTimerController?.StartMortalTimer(tiempoParaMorir);
        messageEvent?.Raise("�El tiempo se agota!");
        if (objectiveChecklistUI != null)
        {
            objectiveChecklistUI.ForceFailPendingsAndGoToSecondPhase();
        }
        if (deathCoroutine == null)
            deathCoroutine = StartCoroutine(ContadorParaMuerte());
    }

    private IEnumerator ContadorParaMuerte()
    {
        yield return new WaitForSeconds(tiempoParaMorir);
        Debug.Log("Se acab� el tiempo. El jugador ha muerto.");
        badgeManager?.UnlockBadge("GameOverSinTiempo");
        HandlePlayerDeath();
    }

    public void IniciarContadorSupervivencia(float duration)
    {
        if (survivalCoroutine != null) return;

        Debug.Log($"Iniciando cuenta atr�s de VICTORIA: {duration} s.");
        survivalCoroutine = StartCoroutine(SupervivenciaCoroutine(duration));
        uiTimerController?.StartFireTimer(duration);
    }

    private IEnumerator SupervivenciaCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        Debug.Log("Tiempo extra terminado. El jugador sobrevive.");
        HandlePlayerSurvival();
    }

    public void HandlePlayerDeath()
    {
        if (currentState != GameState.Playing) return;

        if (levelHasNPCs && npcManager != null)
            npcManager.EvaluateAtGameEnd();

        currentState = GameState.Lost;
        uiTimerController?.HideTimer();

        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
            deathCoroutine = null;
        }

        Debug.Log("GAME OVER: Iniciando transici�n a escena de derrota.");
        LevelCompletionData.currentManager = this.badgeManager;

        // Desactivar input de jugador y liberar cursor para UI
        if (playerController != null) playerController.SetInputEnabled(false);
        if (playerInteraction != null) playerInteraction.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Opcional: mantener una leve c�mara lenta antes del fade
        Time.timeScale = 0.2f;

        StartCoroutine(TransitionToSceneAsync(loseScene));
        onPlayerDeathEvent?.Raise();
        // Al final de HandlePlayerSurvival / HandlePlayerDeath, antes de lanzar la transici�n:
        Time.timeScale = 1f;                  // normaliza
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HandlePlayerSurvival()
    {
        if (currentState != GameState.Playing) return;

        if (levelHasNPCs && npcManager != null)
            npcManager.EvaluateAtGameEnd();

        currentState = GameState.Won;
        uiTimerController?.HideTimer();

        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
            deathCoroutine = null;
            Debug.Log("Contador mortal detenido. El jugador ha sobrevivido.");
        }

        Debug.Log("�VICTORIA!: Iniciando transici�n a escena de victoria.");
        LevelCompletionData.currentManager = this.badgeManager;

        // Badges de fin de nivel (si aplica)
        if (badgeManager != null && playerController != null)
        {
            if (awardNoRunBadgeOnWin && !playerController.HasEverRun)
                badgeManager.UnlockBadge(noRunBadgeId);

            if (awardWindowSafeBadgeOnWin && !playerController.WindowInjuryOccurred)
                badgeManager.UnlockBadge(windowSafeBadgeId);
            if (awardIdealExitBadgeOnWin && !playerController.HasTakenDamage)
                badgeManager.UnlockBadge(idealExitBadgeId);
        }

        // Desactivar input y liberar cursor para usar la UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null) playerController.SetInputEnabled(false);
        if (playerInteraction != null) playerInteraction.enabled = false;

        // Pausa total antes del fade (el sistema de transici�n usa tiempo no escalado)
        Time.timeScale = 0f;

        StartCoroutine(TransitionToSceneAsync(winScene));
        onPlayerSurvivedEvent?.Raise();
        // Al final de HandlePlayerSurvival / HandlePlayerDeath, antes de lanzar la transici�n:
        Time.timeScale = 1f;                  // normaliza
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator TransitionToSceneAsync(SceneDefinitionSO targetScene)
    {
        if (targetScene == null)
        {
            Debug.LogError("[GameManager] No se asign� la SceneDefinitionSO de destino.");
            yield break;
        }

        // Paso 1: solicitar PRE-CARGA as�ncrona (manteniendo paused si as� se desea).
        sceneLoadChannel?.RaiseEvent(targetScene, LoadSceneMode.Single, true);

        // Paso 2: dar un peque�o margen en tiempo real y ACTIVAR (esto dispara el fade y allowSceneActivation).
        yield return new WaitForSecondsRealtime(0.1f);
        activatePreloadedSceneChannel?.RaiseEvent();

        // No necesitamos esperar aqu�; SceneLoader har� el fade-in y normalizar� el Time.timeScale al terminar la carga.
    }

    public void SetGamePaused(bool pause)
    {
        Time.timeScale = pause ? 0f : 1f;
    }
}
