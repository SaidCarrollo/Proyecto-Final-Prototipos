using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    [Header("Datos del Cuestionario")]
    public CuestionarioSO cuestionario; // Arrastra tu ScriptableObject aquí

    [Header("UI Prefabs y Contenedores")]
    public GameObject preguntaUIPrefab; // El prefab del bloque de pregunta
    public Transform contenedorPrincipal; // El objeto "Content" con el GridLayoutGroup

    [Header("Botones de Acción")]
    public Button botonCorregir;
    public Button botonContinuar;

    [Header("Iconos de Resultado")]
    public Sprite iconoCorrecto;
    public Sprite iconoIncorrecto;

    // Diccionarios para guardar el estado del cuestionario
    private Dictionary<int, Button> respuestasSeleccionadas = new Dictionary<int, Button>();
    private Dictionary<Button, bool> mapaDeRespuestasCorrectas = new Dictionary<Button, bool>();
    private List<Button> todosLosBotonesDeRespuesta = new List<Button>();

    void Start()
    {
        botonCorregir.gameObject.SetActive(false);
        botonContinuar.gameObject.SetActive(false);
        InstanciarCuestionario();
    }

    void InstanciarCuestionario()
    {
        for (int i = 0; i < cuestionario.preguntas.Length; i++)
        {
            // --- Crear el bloque de pregunta ---
            GameObject bloquePregunta = Instantiate(preguntaUIPrefab, contenedorPrincipal);
            int preguntaIndex = i; // Variable local para evitar problemas en el listener

            // --- Configurar el texto de la pregunta ---
            bloquePregunta.GetComponentInChildren<TextMeshProUGUI>().text = cuestionario.preguntas[i].textoPregunta;

            // --- Crear los botones de respuesta ---
            Button[] botonesRespuesta = bloquePregunta.GetComponentsInChildren<Button>();
            for (int j = 0; j < botonesRespuesta.Length; j++)
            {
                Button botonActual = botonesRespuesta[j];
                todosLosBotonesDeRespuesta.Add(botonActual);

                // Configurar texto del botón y guardar si es correcto
                Respuesta respuestaData = cuestionario.preguntas[i].respuestas[j];
                botonActual.GetComponentInChildren<TextMeshProUGUI>().text = respuestaData.textoRespuesta;
                mapaDeRespuestasCorrectas[botonActual] = respuestaData.esCorrecta;

                // Añadir listener
                botonActual.onClick.AddListener(() => OnRespuestaSeleccionada(preguntaIndex, botonActual));
            }
        }
    }

    void OnRespuestaSeleccionada(int preguntaIndex, Button botonPulsado)
    {
        // Guardar la selección del jugador
        respuestasSeleccionadas[preguntaIndex] = botonPulsado;

        // Podrías añadir un efecto visual al botón pulsado para que el jugador sepa que lo ha seleccionado
        // (Ej. cambiarle el color)

        // Comprobar si ya se han respondido todas las preguntas
        if (respuestasSeleccionadas.Count == cuestionario.preguntas.Length)
        {
            botonCorregir.gameObject.SetActive(true);
        }
    }

    // Esta función la llamarás desde el onClick del botonCorregir en el Inspector
    public void CorregirRespuestas()
    {
        botonCorregir.gameObject.SetActive(false);

        foreach (Button boton in todosLosBotonesDeRespuesta)
        {
            // Desactivamos todos los botones para que no se puedan cambiar las respuestas
            boton.interactable = false;
        }

        // Iteramos sobre las respuestas que el jugador seleccionó
        foreach (var seleccion in respuestasSeleccionadas)
        {
            Button botonSeleccionado = seleccion.Value;
            bool fueCorrecta = mapaDeRespuestasCorrectas[botonSeleccionado];

            Image icono = botonSeleccionado.transform.Find("IconoResultado").GetComponent<Image>();
            icono.sprite = fueCorrecta ? iconoCorrecto : iconoIncorrecto;
            icono.gameObject.SetActive(true);
        }

        // Mostramos el botón para continuar a la siguiente escena
        botonContinuar.gameObject.SetActive(true);
    }
}