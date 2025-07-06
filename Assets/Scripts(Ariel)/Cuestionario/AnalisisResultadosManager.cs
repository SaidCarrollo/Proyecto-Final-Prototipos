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
        if (resultadosDelQuiz == null || resultadosDelQuiz.resultados.Count == 0)
        {
            Debug.LogWarning("No hay resultados para mostrar.");
            return;
        }

        MostrarResultados();
    }

    void MostrarResultados()
    {
        // Recorremos cada resultado que guardamos
        foreach (var resultado in resultadosDelQuiz.resultados)
        {
            // Creamos una instancia del prefab de UI
            GameObject bloqueResultado = Instantiate(resultadoUIPrefab, contenedorResultados);

            // Obtenemos los datos guardados
            Pregunta pregunta = resultado.preguntaOriginal;
            int indiceMarcado = resultado.indiceRespuestaMarcada;
            Respuesta respuestaMarcada = pregunta.respuestas[indiceMarcado];

            // Buscamos los componentes de UI dentro del prefab instanciado
            TextMeshProUGUI textoPregunta = bloqueResultado.transform.Find("TextoPregunta").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI textoRespuestaMarcada = bloqueResultado.transform.Find("TextoRespuestaMarcada").GetComponent<TextMeshProUGUI>();
            Image iconoResultado = textoRespuestaMarcada.transform.Find("IconoResultado").GetComponent<Image>();
            TextMeshProUGUI textoJustificacion = bloqueResultado.transform.Find("TextoJustificacion").GetComponent<TextMeshProUGUI>();

            // Populamos la UI con los datos
            textoPregunta.text = pregunta.textoPregunta;
            textoRespuestaMarcada.text = "Tu respuesta: " + respuestaMarcada.textoRespuesta;
            textoJustificacion.text = respuestaMarcada.justificacion;

            if (respuestaMarcada.esCorrecta)
            {
                iconoResultado.sprite = iconoCorrecto;
            }
            else
            {
                iconoResultado.sprite = iconoIncorrecto;
            }
        }
    }
}