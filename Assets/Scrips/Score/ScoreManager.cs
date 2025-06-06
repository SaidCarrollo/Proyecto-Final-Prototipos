
using UnityEngine;
using TMPro; 

public class ScoreManager : MonoBehaviour
{
    [Tooltip("Referencia al texto de la UI para mostrar el puntaje (TextMeshPro).")]
    public TextMeshProUGUI scoreText; 

    private int currentScore = 0;

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddPoints(float points)
    {
        currentScore += (int)points;
        Debug.Log("Puntos añadidos: " + points + ". Puntaje total: " + currentScore);
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Puntaje: " + currentScore;
        }
    }
}