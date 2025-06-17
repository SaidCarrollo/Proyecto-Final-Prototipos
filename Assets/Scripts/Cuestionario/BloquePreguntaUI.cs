using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BloquePreguntaUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI textoPregunta;
    public GameObject grupoDeOpciones; // El objeto padre de los Toggles
    public GameObject opcionRespuestaPrefab; // El prefab del Toggle

    private Pregunta preguntaAsociada;
    private List<Toggle> togglesInstanciados = new List<Toggle>();

    // El QuizManager llama a esta función para configurar el bloque
    public void Configurar(Pregunta pregunta)
    {
        preguntaAsociada = pregunta;
        textoPregunta.text = pregunta.textoPregunta;

        ToggleGroup toggleGroup = grupoDeOpciones.GetComponent<ToggleGroup>();

        // Crear un toggle para cada opción de respuesta
        for (int i = 0; i < pregunta.respuestas.Length; i++)
        {
            GameObject nuevaOpcion = Instantiate(opcionRespuestaPrefab, grupoDeOpciones.transform);
            Toggle toggle = nuevaOpcion.GetComponent<Toggle>();
            toggle.group = toggleGroup; // Asignar al grupo
            togglesInstanciados.Add(toggle);

            // Configurar el texto de la opción
            nuevaOpcion.GetComponentInChildren<TextMeshProUGUI>().text = pregunta.respuestas[i].textoRespuesta;
        }
    }

    // Esta función revisa la respuesta y muestra el feedback
    public void CorregirPregunta()
    {
        for (int i = 0; i < togglesInstanciados.Count; i++)
        {
            Toggle toggle = togglesInstanciados[i];
            Image feedbackIcon = toggle.transform.Find("FeedbackIcon").GetComponent<Image>();

            // Si este toggle fue el que se seleccionó...
            if (toggle.isOn)
            {
                feedbackIcon.gameObject.SetActive(true);
                // ...comprobamos si la respuesta asociada era la correcta
                if (preguntaAsociada.respuestas[i].esCorrecta)
                {
                    feedbackIcon.color = Color.green; // O asigna un sprite de Check
                }
                else
                {
                    feedbackIcon.color = Color.red; // O asigna un sprite de X 
                }
            }
            // Opcional: Desactivar los toggles para que no se pueda cambiar la respuesta
            toggle.interactable = false;
        }
    }
}