using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;
public class QuizManager : MonoBehaviour
{
    [Header("Datos del Cuestionario")]
    public CuestionarioSO cuestionario;
    [Header("Almacenamiento de Resultados")]
    public ResultadosDelQuizSO resultadosGuardados;
    public bool esPostGame;

    [Header("UI - Elementos de la Pregunta")]
    public TextMeshProUGUI textoPregunta;
    public GameObject grupoDeOpciones;
    public GameObject opcionRespuestaPrefab;

    [Header("UI - Feedback")]
    public Sprite iconoCorrecto;
    public Sprite iconoIncorrecto;
    public Button botonSiguientePregunta;
    public Button botonEmpezarNivel;

    [Header("Eventos")]
    public UnityEvent alFinalizarQuiz;

    private int preguntaActualIndex = 0;
    private List<Toggle> togglesInstanciados = new List<Toggle>();
    private bool respuestaEnviada = false;

    void Start()
    {
        if (resultadosGuardados != null)
        {
            resultadosGuardados.LimpiarResultados();
        }
        botonEmpezarNivel.gameObject.SetActive(false);
        botonSiguientePregunta.onClick.AddListener(OnBotonSiguienteClick);
        CargarPregunta();
    }

    void CargarPregunta()
    {
        respuestaEnviada = false;

        foreach (Toggle t in togglesInstanciados)
        {
            Destroy(t.gameObject);
        }
        togglesInstanciados.Clear();

        if (preguntaActualIndex >= cuestionario.preguntas.Length)
        {
            textoPregunta.text = "¡Nivel listo para empezar!";
            grupoDeOpciones.SetActive(false);
            botonSiguientePregunta.gameObject.SetActive(false);
            botonEmpezarNivel.gameObject.SetActive(true);
            return;
        }

        if (preguntaActualIndex == cuestionario.preguntas.Length - 1)
        {
            botonSiguientePregunta.gameObject.SetActive(false);
            botonEmpezarNivel.gameObject.SetActive(true);
            botonEmpezarNivel.interactable = true;
        }
        else
        {
            botonSiguientePregunta.gameObject.SetActive(true);
            botonSiguientePregunta.interactable = true;
            botonEmpezarNivel.gameObject.SetActive(false);
        }

        Pregunta pregunta = cuestionario.preguntas[preguntaActualIndex];
        textoPregunta.text = pregunta.textoPregunta;

        for (int i = 0; i < pregunta.respuestas.Length; i++)
        {
            GameObject nuevaOpcion = Instantiate(opcionRespuestaPrefab, grupoDeOpciones.transform);
            Toggle toggle = nuevaOpcion.GetComponent<Toggle>();
            togglesInstanciados.Add(toggle);
            toggle.group = grupoDeOpciones.GetComponent<ToggleGroup>();
            toggle.isOn = false;
            nuevaOpcion.GetComponentInChildren<TextMeshProUGUI>().text = pregunta.respuestas[i].textoRespuesta;
        }
    }

    public void OnBotonSiguienteClick()
    {
        if (respuestaEnviada) return;
        StartCoroutine(CorregirYContinuar());
        SoundManager.Instance.PlaySFX("Click");
    }

    private IEnumerator CorregirYContinuar()
    {
        respuestaEnviada = true;
        botonSiguientePregunta.interactable = false;
        botonEmpezarNivel.interactable = false;

        int respuestaSeleccionadaIndex = -1;
        Toggle toggleSeleccionado = null;
        for (int i = 0; i < togglesInstanciados.Count; i++)
        {
            togglesInstanciados[i].interactable = false;
            if (togglesInstanciados[i].isOn)
            {
                respuestaSeleccionadaIndex = i;
                toggleSeleccionado = togglesInstanciados[i];
            }
        }

        if (toggleSeleccionado != null)
        {
            Pregunta preguntaActual = cuestionario.preguntas[preguntaActualIndex];
            if (resultadosGuardados != null)
            {
                resultadosGuardados.resultados.Add(new ResultadoPregunta(preguntaActual, respuestaSeleccionadaIndex));
            }

            bool esCorrecta = preguntaActual.respuestas[respuestaSeleccionadaIndex].esCorrecta;
            Image feedbackIcon = toggleSeleccionado.transform.Find("IconoFeedback").GetComponent<Image>();
            feedbackIcon.sprite = esCorrecta ? iconoCorrecto : iconoIncorrecto;
            feedbackIcon.gameObject.SetActive(true);

            yield return new WaitForSeconds(2);
        }
        else
        {
            yield break;
        }

        if (preguntaActualIndex >= cuestionario.preguntas.Length - 1)
        {
            // 1. Guardar local
            if (resultadosGuardados != null)
            {
                resultadosGuardados.GuardarResultados();
                Debug.Log($"[QuizManager] Resultados guardados localmente para {resultadosGuardados.nombreNivel} ({(esPostGame ? "POST" : "PRE")}).");
            }
            else
            {
                Debug.LogWarning("[QuizManager] No hay ResultadosDelQuizSO asignado.");
            }

            // 2. Flag PRE/POST
            PlayerPrefs.SetInt("LAST_QUIZ_MOMENT", esPostGame ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log("[QuizManager] LAST_QUIZ_MOMENT guardado como: " + (esPostGame ? "POST(1)" : "PRE(0)"));

            // 3. Subir a la nube
            if (resultadosGuardados != null)
            {
                Debug.Log("[QuizManager] Iniciando subida a la nube...");
                _ = QuizCloudUploader.SubirResultadosAsync(resultadosGuardados);
            }

            // 4. Continuar flujo
            alFinalizarQuiz.Invoke();
        }
        else
        {
            preguntaActualIndex++;
            CargarPregunta();
        }

        yield break;
    }
}
