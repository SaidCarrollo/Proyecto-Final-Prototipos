using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Won, Lost }
    private GameState currentState;

    [Header("UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Player Components to Disable")]
    [SerializeField] private PlayerInteraction playerInteraction; 

    void Start()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void HandlePlayerDeath()
    {
        if (currentState != GameState.Playing) return; 

        currentState = GameState.Lost;
        Debug.Log("GAME OVER: El jugador ha muerto.");

        if (losePanel != null) losePanel.SetActive(true);
        if (playerInteraction != null) playerInteraction.enabled = false;

        Time.timeScale = 0f; // Pausar el juego
    }

    public void HandlePlayerSurvival()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Won;
        Debug.Log("¡VICTORIA!: El jugador ha sobrevivido.");

        if (winPanel != null) winPanel.SetActive(true);
        if (playerInteraction != null) playerInteraction.enabled = false;

        Time.timeScale = 0f; 
    }
}