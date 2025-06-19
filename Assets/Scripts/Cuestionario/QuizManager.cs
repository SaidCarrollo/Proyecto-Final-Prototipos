using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class QuizManager : MonoBehaviour
{
    [Header("Datos del Cuestionario")]
    public CuestionarioSO cuestionario;

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
        botonEmpezarNivel.gameObject.SetActive(false);
        botonSiguientePregunta.onClick.AddListener(OnBotonSiguienteClick);
        CargarPregunta();
    }

    void CargarPregunta()
    {
        respuestaEnviada = false;

        // Limpiar los toggles de la pregunta anterior
        foreach (Toggle t in togglesInstanciados)
        {
            Destroy(t.gameObject);
        }
        togglesInstanciados.Clear();

        // Comprobar si ya se complet� el cuestionario (esta l�gica ahora es para despu�s de la �ltima pregunta)
        if (preguntaActualIndex >= cuestionario.preguntas.Length)
        {
            // Esto solo se ejecutar�a si hay un error o si se quiere una pantalla final despu�s del bot�n de nivel.
            // Por ahora lo dejamos como una salvaguarda.
            textoPregunta.text = "�Nivel listo para empezar!";
            grupoDeOpciones.SetActive(false);
            botonSiguientePregunta.gameObject.SetActive(false);
            botonEmpezarNivel.gameObject.SetActive(true);
            return;
        }

        // Decidir qu� bot�n mostrar ANTES de cargar la pregunta
        if (preguntaActualIndex == cuestionario.preguntas.Length - 1)
        {
            // Si es la �LTIMA pregunta, muestra el bot�n de empezar nivel
            botonSiguientePregunta.gameObject.SetActive(false);
            botonEmpezarNivel.gameObject.SetActive(true);
            botonEmpezarNivel.interactable = true;
        }
        else
        {
            // Si NO es la �ltima, muestra el bot�n de siguiente
            botonSiguientePregunta.gameObject.SetActive(true);
            botonSiguientePregunta.interactable = true;
            botonEmpezarNivel.gameObject.SetActive(false);
        }

        // --- El resto de la funci�n sigue igual ---
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
    }

    private IEnumerator CorregirYContinuar()
    {
        respuestaEnviada = true;
        botonSiguientePregunta.interactable = false;
        botonEmpezarNivel.interactable = false;

        // ... (toda la l�gica para encontrar el toggle y mostrar el icono de feedback sigue exactamente igual) ...
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
            bool esCorrecta = cuestionario.preguntas[preguntaActualIndex].respuestas[respuestaSeleccionadaIndex].esCorrecta;
            Image feedbackIcon = toggleSeleccionado.transform.Find("IconoFeedback").GetComponent<Image>();
            feedbackIcon.sprite = esCorrecta ? iconoCorrecto : iconoIncorrecto;
            feedbackIcon.gameObject.SetActive(true);

            Debug.Log("Mostrando feedback...");
            yield return new WaitForSeconds(2); // Esperar 2 segundos
            Debug.Log("Feedback mostrado.");

        }
        else
        {
            // ... (l�gica si no se seleccion� nada, sigue igual) ...
            yield break;
        }

        // --- �AQU� EST� EL CAMBIO FINAL! ---
        if (preguntaActualIndex >= cuestionario.preguntas.Length - 1)
        {
            // Si acabamos de corregir la �LTIMA pregunta...
            Debug.Log("Quiz finalizado. Invocando evento alFinalizarQuiz...");
            alFinalizarQuiz.Invoke(); // <-- �DISPARAMOS EL EVENTO!
        }
        else
        {
            // Si no era la �ltima, pasamos a la siguiente.
            preguntaActualIndex++;
            CargarPregunta();
        }
    }
}