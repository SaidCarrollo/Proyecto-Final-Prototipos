// AnalisisResultadosManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnalisisResultadosManager : MonoBehaviour
{
    [Header("UI")]
    public Transform contenedorResultados; // El objeto "Content" del ScrollView
    public GameObject resultadoUIPrefab;   // El prefab que creaste para mostrar un resultado

    [Header("Iconos")]
    public Sprite iconoCorrecto;
    public Sprite iconoIncorrecto;

    [Header("Selección dinámica")]
    public LastPlayedLevelSO lastPlayed;   // ← arrástralo en el Inspector
    public SurveyRegistrySO registry;      // ← arrástralo en el Inspector
    public bool esPostGame;                // marca si es post o pre en ESTA escena

    [Header("Datos")]
    public ResultadosDelQuizSO resultadosDelQuiz; // si no lo setean manual, se resuelve
    void Start()
    {
        // 1) Resolver el SO si no está asignado manualmente
        if (resultadosDelQuiz == null && registry != null && lastPlayed != null && lastPlayed.lastLevel != null)
        {
            bool ultimoFuePost = PlayerPrefs.GetInt("LAST_QUIZ_MOMENT", 0) == 1; // 0=PRE, 1=POST
            resultadosDelQuiz = registry.GetResultados(lastPlayed.lastLevel, ultimoFuePost);
        }

        if (resultadosDelQuiz == null)
        {
            Debug.LogError("No se pudo resolver ResultadosDelQuizSO. Revisa registry/lastPlayed y que se haya seteado LAST_QUIZ_MOMENT desde el Quiz.");
            return;
        }

        // 2) Cargar el último archivo que coincida con prefijo + nivel + momento
        string path = ResultadosFileHelper.GetUltimoArchivo(
            resultadosDelQuiz.prefijo,
            resultadosDelQuiz.nombreNivel,
            resultadosDelQuiz.esPostGame
        );
        Debug.Log("Analisis: cargando archivo: " + path);

        if (!string.IsNullOrEmpty(path))
        {
            resultadosDelQuiz.CargarResultadosDesdeArchivo(path); // ← LLAMADA SOBRE LA INSTANCIA
        }
        else
        {
            Debug.LogWarning("No se encontró un archivo de resultados para este patrón.");
        }

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