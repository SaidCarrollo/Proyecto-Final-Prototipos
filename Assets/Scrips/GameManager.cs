using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections; 

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Won, Lost }
    private GameState currentState;

    [Header("Scene Management")]
    [Tooltip("El nombre exacto de la escena de victoria en Build Settings.")]
    [SerializeField] private string winSceneName;
    [Tooltip("El nombre exacto de la escena de derrota en Build Settings.")]
    [SerializeField] private string loseSceneName;

    [Header("Player Components to Disable")]
    [SerializeField] private PlayerInteraction playerInteraction;
    [Header("Managers")]
    [SerializeField] private BadgeManager badgeManager;
    [SerializeField] private MistakeManager mistakeManager; 

    void Start()
    {
        if (badgeManager != null)
        {
            badgeManager.ResetBadges();
        }
        if (mistakeManager != null) 
        {
            mistakeManager.ResetMistakes();
        }

        currentState = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void HandlePlayerDeath()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Lost;
        Debug.Log("GAME OVER: El jugador ha muerto. Iniciando carga de escena de derrota.");

        if (playerInteraction != null) playerInteraction.enabled = false;

        Time.timeScale = 0.2f;

        StartCoroutine(LoadAdditiveScene(loseSceneName));
    }

    public void HandlePlayerSurvival()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Won;
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