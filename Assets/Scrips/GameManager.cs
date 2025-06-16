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

    [Header("Player Components to Disable")]
    [SerializeField] private PlayerInteraction playerInteraction;

    [Header("Managers and Events")]
    [SerializeField] private BadgeManager badgeManager;
    [SerializeField] private GameEvent onPlayerDeathEvent;
    [SerializeField] private GameEvent onPlayerSurvivedEvent;
    [SerializeField] private GameEventstring messageEvent;
    [SerializeField] private GameEvent onUncontrolledFireEvent;
    [Header("Death Timer Settings")] 
    [SerializeField] private float tiempoParaMorir = 15f;
    private Coroutine deathCoroutine;
    public bool IsFireUncontrolled { get; private set; } = false;
    void Start()
    {
        if (badgeManager != null)
        {
            badgeManager.ResetBadges();
        }
        IsFireUncontrolled = false;
        currentState = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void HandleUncontrolledFire()
    {
        Debug.Log("GameManager ha sido notificado: ¡El fuego está fuera de control!");
        IsFireUncontrolled = true;
    }
    public void IniciarContadorMortal()
    {
        if (currentState != GameState.Playing) return;

        Debug.Log("Contador mortal iniciado en GameManager. El jugador tiene " + tiempoParaMorir + " segundos.");

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

    public void HandlePlayerDeath()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Lost;

        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
            deathCoroutine = null;
        }

        Debug.Log("GAME OVER: El jugador ha muerto. Iniciando carga de escena de derrota.");
        if (playerInteraction != null) playerInteraction.enabled = false;
        Time.timeScale = 0.2f;
        StartCoroutine(LoadAdditiveScene(loseSceneName));
    }

    public void HandlePlayerSurvival()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Won;

        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
            deathCoroutine = null;
            Debug.Log("Contador mortal detenido. El jugador ha sobrevivido.");
        }

        Debug.Log("¡VICTORIA!: El jugador ha sobrevivido. Iniciando carga de escena de victoria.");
        if (playerInteraction != null) playerInteraction.enabled = false;
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
        Time.timeScale = 1f;
    }
}