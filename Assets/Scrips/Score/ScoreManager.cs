
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Referencia al texto de la UI para mostrar el puntaje (TextMeshPro).")]
    public TextMeshProUGUI scoreText;

    [Header("Configuraci�n de Puntos")]
    [Tooltip("Puntos a a�adir si se llama a los bomberos durante un fuego descontrolado.")]
    [SerializeField] private int puntosLlamadaCorrecta = 100;
    [Tooltip("Puntos a restar si se llama a los bomberos sin necesidad (falsa alarma).")]
    [SerializeField] private int puntosLlamadaIncorrecta = 50;

    private int currentScore = 0;
    private bool penalizacionActiva = false;

    public void ActivarPenalizacionPorFuego()
    {
        Debug.LogWarning("�PENALIZACI�N DE PUNTAJE ACTIVADA! A partir de ahora se restar�n puntos por acciones incorrectas.");
        penalizacionActiva = true;
    }

    public void OnLlamadaRealizada()
    {
        if (penalizacionActiva)
        {
            Debug.Log("Llamada correcta a los bomberos. �Puntos ganados!");
            currentScore += puntosLlamadaCorrecta; 
            UpdateScoreUI();                      
        }
        else
        {
            // El fuego NO est� descontrolado. 
            Debug.LogWarning("Llamada innecesaria (Falsa Alarma). �Puntos perdidos!");
            currentScore -= puntosLlamadaIncorrecta;
            UpdateScoreUI();
        }
    }

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddPoints(float points)
    {

        if (penalizacionActiva) 
        {
            currentScore -= (int)points;
            Debug.Log("Penalizaci�n aplicada: -" + points + ". Puntaje total: " + currentScore);
        }
        else
        {
            currentScore += (int)points; 
            Debug.Log("Puntos a�adidos: " + points + ". Puntaje total: " + currentScore);
        }

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