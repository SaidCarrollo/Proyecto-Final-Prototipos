// AnalisisResultadosManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnalisisResultadosManager : MonoBehaviour
{
    [Header("Datos")]
    public ResultadosDelQuizSO resultadosDelQuiz; // El mismo Scriptable Object que usó el QuizManager

    [Header("UI")]
    public Transform contenedorResultados; // El objeto "Content" del ScrollView
    public GameObject resultadoUIPrefab;   // El prefab que creaste para mostrar un resultado

    [Header("Iconos")]
    public Sprite iconoCorrecto;
    public Sprite iconoIncorrecto;

    void Start()
    {
        if (resultadosDelQuiz == null)
        {
            Debug.LogError("No asignaste ResultadosDelQuizSO en el inspector.");
            return;
        }

        resultadosDelQuiz.CargarResultados(); // <- Añade esto

        if (resultadosDelQuiz.resultados.Count == 0)
        {
            Debug.LogWarning("No hay resultados para mostrar.");
            return;
        }

        MostrarResultados();
    }


    void MostrarResultados()
    {
        foreach (var resultado in resultadosDelQuiz.resultados)
        {
            GameObject bloqueResultado = Instantiate(resultadoUIPrefab, contenedorResultados);

            TextMeshProUGUI textoPregunta = bloqueResultado.transform.Find("TextoPregunta").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI textoRespuestaMarcada = bloqueResultado.transform.Find("TextoRespuestaMarcada").GetComponent<TextMeshProUGUI>();
            Image iconoResultado = textoRespuestaMarcada.transform.Find("IconoResultado").GetComponent<Image>();
            TextMeshProUGUI textoJustificacion = bloqueResultado.transform.Find("TextoJustificacion").GetComponent<TextMeshProUGUI>();

            textoPregunta.text = resultado.textoPregunta;
            textoRespuestaMarcada.text = "Tu respuesta: " + resultado.textosRespuestas[resultado.indiceRespuestaMarcada];
            textoJustificacion.text = resultado.justificaciones[resultado.indiceRespuestaMarcada];

            bool esCorrecta = resultado.respuestasCorrectas[resultado.indiceRespuestaMarcada];
            iconoResultado.sprite = esCorrecta ? iconoCorrecto : iconoIncorrecto;
        }
    }
}