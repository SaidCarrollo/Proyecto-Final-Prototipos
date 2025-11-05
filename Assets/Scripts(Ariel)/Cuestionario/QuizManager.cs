using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using DG.Tweening; // DOTween

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

    [Header("Animación DOTween")]
    [Tooltip("Duración del fade de la pregunta")]
    [SerializeField] private float questionFadeDuration = 0.35f;

    [Tooltip("Escala inicial de cada opción (antes de animar)")]
    [SerializeField] private float optionInitialScale = 0.8f;

    [Tooltip("Duración del fade/escala de cada opción")]
    [SerializeField] private float optionFadeDuration = 0.25f;

    [Tooltip("Retraso entre cada opción para el efecto cascada")]
    [SerializeField] private float optionCascadeDelay = 0.08f;

    [Tooltip("Ease de la entrada de las opciones")]
    [SerializeField] private Ease optionEase = Ease.OutBack;

    [Header("Typewriter (Escritura)")]
    [Tooltip("Delay entre caracteres de la pregunta (rápido). Ej: 0.015")]
    [SerializeField] private float questionTypewriterCharDelay = 0.015f;

    [Tooltip("Delay entre caracteres de las respuestas (rápido). Ej: 0.01")]
    [SerializeField] private float optionTypewriterCharDelay = 0.01f;

    [Header("Feedback al no marcar opción")]
    [Tooltip("Duración del temblor cuando no se ha marcado ninguna respuesta")]
    [SerializeField] private float shakeDuration = 0.3f;
    [Tooltip("Magnitud del temblor (en píxeles)")]
    [SerializeField] private float shakeStrength = 10f;
    [Tooltip("Vibrato del temblor (cantidad de sacudidas)")]
    [SerializeField] private int shakeVibrato = 20;

    [Header("Colores de las opciones")]
    [Tooltip("Color base del fondo del toggle (no seleccionado)")]
    [SerializeField] private Color normalToggleColor = Color.white;
    [Tooltip("Color del fondo cuando la opción está seleccionada")]
    [SerializeField] private Color selectedToggleColor = Color.green;

    private int preguntaActualIndex = 0;
    private List<Toggle> togglesInstanciados = new List<Toggle>();
    private bool respuestaEnviada = false;

    // Control de corutinas de escritura
    private Coroutine questionTypewriterCoroutine;
    private readonly List<Coroutine> optionTypewriterCoroutines = new List<Coroutine>();

    // Para recordar el texto completo de cada opción
    private Dictionary<TextMeshProUGUI, string> opcionTextosCompletos = new Dictionary<TextMeshProUGUI, string>();

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

        // Parar corutinas de escritura anteriores
        if (questionTypewriterCoroutine != null)
        {
            StopCoroutine(questionTypewriterCoroutine);
            questionTypewriterCoroutine = null;
        }
        foreach (var c in optionTypewriterCoroutines)
        {
            if (c != null) StopCoroutine(c);
        }
        optionTypewriterCoroutines.Clear();
        opcionTextosCompletos.Clear();

        // Limpiar opciones anteriores
        foreach (Toggle t in togglesInstanciados)
        {
            if (t != null)
            {
                t.transform.DOKill();
                var cgPrev = t.GetComponent<CanvasGroup>();
                if (cgPrev != null) cgPrev.DOKill();
                Destroy(t.gameObject);
            }
        }
        togglesInstanciados.Clear();

        // Fin del cuestionario
        if (preguntaActualIndex >= cuestionario.preguntas.Length)
        {
            string textoFinal = "¡Nivel listo para empezar!";

            // Typewriter para el mensaje final
            if (textoPregunta != null)
            {
                textoPregunta.text = "";
                questionTypewriterCoroutine = StartCoroutine(
                    TypeText(textoPregunta, textoFinal, questionTypewriterCharDelay)
                );
            }

            grupoDeOpciones.SetActive(false);
            botonSiguientePregunta.gameObject.SetActive(false);
            botonEmpezarNivel.gameObject.SetActive(true);

            AnimarEntradaPreguntaYOpciones(new List<GameObject>());
            return;
        }

        // Config de botones Siguiente / Empezar
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

        // Pregunta: limpiar y luego escribir con efecto
        if (textoPregunta != null)
        {
            textoPregunta.text = "";
            questionTypewriterCoroutine = StartCoroutine(
                TypeText(textoPregunta, pregunta.textoPregunta, questionTypewriterCharDelay)
            );
        }

        // ToggleGroup: permitir todo apagado
        ToggleGroup toggleGroup = grupoDeOpciones.GetComponent<ToggleGroup>();
        if (toggleGroup != null)
        {
            toggleGroup.allowSwitchOff = true;
        }

        // Instanciar opciones y guardarlas para animación
        List<GameObject> opcionesCreadas = new List<GameObject>();

        for (int i = 0; i < pregunta.respuestas.Length; i++)
        {
            GameObject nuevaOpcion = Instantiate(opcionRespuestaPrefab, grupoDeOpciones.transform);
            opcionesCreadas.Add(nuevaOpcion);

            Toggle toggle = nuevaOpcion.GetComponent<Toggle>();
            togglesInstanciados.Add(toggle);

            if (toggleGroup != null)
                toggle.group = toggleGroup;

            // Aseguramos que arranca apagado
            toggle.isOn = false;

            // 🔹 DESACTIVAR TRANSICIONES DE COLOR DEL TOGGLE
            toggle.transition = Selectable.Transition.None;

            // 🔹 Buscar la imagen de fondo a colorear
            Image bgImage = toggle.targetGraphic as Image;
            if (bgImage == null)
                bgImage = toggle.GetComponent<Image>(); // respaldo por si el targetGraphic está vacío

            if (bgImage != null)
            {
                // Color base al crearla
                bgImage.color = normalToggleColor;

                Image localImage = bgImage; // capturamos referencia local para la lambda
                toggle.onValueChanged.AddListener(isOn =>
                {
                    // Primer toque -> isOn = true -> verde
                    // Segundo toque -> isOn = false -> blanco
                    if (localImage != null)
                        localImage.color = isOn ? selectedToggleColor : normalToggleColor;
                });
            }

            // Texto de la respuesta con typewriter
            TextMeshProUGUI tmpTexto = nuevaOpcion.GetComponentInChildren<TextMeshProUGUI>();
            string textoCompleto = pregunta.respuestas[i].textoRespuesta;
            if (tmpTexto != null)
            {
                opcionTextosCompletos[tmpTexto] = textoCompleto;
                tmpTexto.text = ""; // se llenará con typewriter al entrar en cascada
            }

            // Estado base para la animación DOTween (alpha 0, escala pequeña)
            CanvasGroup cg = nuevaOpcion.GetComponent<CanvasGroup>();
            if (cg == null) cg = nuevaOpcion.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            RectTransform rt = nuevaOpcion.transform as RectTransform;
            if (rt != null)
            {
                rt.localScale = Vector3.one * optionInitialScale;
            }

            cg.DOKill();
            nuevaOpcion.transform.DOKill();
        }

        // Por si acaso, apagamos todas al final del spawn
        if (toggleGroup != null)
        {
            toggleGroup.SetAllTogglesOff();
        }

        // Animar entrada de la pregunta + opciones en cascada
        AnimarEntradaPreguntaYOpciones(opcionesCreadas);
    }

    public void OnBotonSiguienteClick()
    {
        if (respuestaEnviada) return;

        // Comprobar si hay alguna opción marcada
        bool algunaSeleccionada = false;
        for (int i = 0; i < togglesInstanciados.Count; i++)
        {
            if (togglesInstanciados[i] != null && togglesInstanciados[i].isOn)
            {
                algunaSeleccionada = true;
                break;
            }
        }

        // Si no hay ninguna marcada, temblor y no avanza
        if (!algunaSeleccionada)
        {
            ShakeOptions();
            SoundManager.Instance.PlaySFX("Click");
            return;
        }

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

            yield return new WaitForSeconds(2f);
        }
        else
        {
            // Por seguridad, reactivar si algo raro pasó
            respuestaEnviada = false;
            botonSiguientePregunta.interactable = true;
            botonEmpezarNivel.interactable = true;
            yield break;
        }

        // Animación de salida de la pregunta actual antes de cambiar
        yield return StartCoroutine(AnimarSalidaPreguntaYOpciones());

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
    }

    #region ANIMACIONES DOTWEEN

    private void AnimarEntradaPreguntaYOpciones(List<GameObject> opciones)
    {
        // Pregunta (fade-in simple)
        if (textoPregunta != null)
        {
            CanvasGroup cgPregunta = textoPregunta.GetComponent<CanvasGroup>();
            if (cgPregunta == null) cgPregunta = textoPregunta.gameObject.AddComponent<CanvasGroup>();

            cgPregunta.DOKill();

            cgPregunta.alpha = 0f;
            cgPregunta.DOFade(1f, questionFadeDuration)
                      .SetEase(Ease.OutQuad);
        }

        // Opciones en cascada + typewriter de cada opción
        float delayAcumulado = 0f;

        foreach (var opcion in opciones)
        {
            if (opcion == null) continue;

            CanvasGroup cg = opcion.GetComponent<CanvasGroup>();
            if (cg == null) cg = opcion.AddComponent<CanvasGroup>();
            cg.DOKill();

            RectTransform rt = opcion.transform as RectTransform;
            if (rt == null) continue;
            rt.DOKill();

            cg.alpha = 0f;
            rt.localScale = Vector3.one * optionInitialScale;

            // Capturas locales para el cierre de la lambda
            var cgLocal = cg;
            var rtLocal = rt;
            TextMeshProUGUI tmpLocal = opcion.GetComponentInChildren<TextMeshProUGUI>();

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delayAcumulado);
            seq.Append(
                rtLocal.DOScale(1f, optionFadeDuration)
                      .SetEase(optionEase)
            );
            seq.Join(
                cgLocal.DOFade(1f, optionFadeDuration)
            );

            // Al terminar la entrada de esta opción, comenzar su typewriter
            seq.OnComplete(() =>
            {
                if (tmpLocal != null && opcionTextosCompletos.TryGetValue(tmpLocal, out string fullText))
                {
                    tmpLocal.text = ""; // aseguramos vacío
                    var c = StartCoroutine(TypeText(tmpLocal, fullText, optionTypewriterCharDelay));
                    optionTypewriterCoroutines.Add(c);
                }
            });

            delayAcumulado += optionCascadeDelay;
        }
    }

    private IEnumerator AnimarSalidaPreguntaYOpciones()
    {
        float durSalidaPregunta = questionFadeDuration * 0.6f;
        float durSalidaOpciones = optionFadeDuration * 0.6f;
        float durMax = Mathf.Max(durSalidaPregunta, durSalidaOpciones);

        // Pregunta
        if (textoPregunta != null)
        {
            CanvasGroup cgPregunta = textoPregunta.GetComponent<CanvasGroup>();
            if (cgPregunta == null) cgPregunta = textoPregunta.gameObject.AddComponent<CanvasGroup>();

            cgPregunta.DOKill();
            cgPregunta.DOFade(0f, durSalidaPregunta).SetEase(Ease.InQuad);
        }

        // Opciones
        foreach (var t in togglesInstanciados)
        {
            if (t == null) continue;

            CanvasGroup cg = t.GetComponent<CanvasGroup>();
            if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();

            cg.DOKill();
            cg.DOFade(0f, durSalidaOpciones).SetEase(Ease.InQuad);

            RectTransform rt = t.transform as RectTransform;
            if (rt != null)
            {
                rt.DOKill();
                rt.DOScale(optionInitialScale, durSalidaOpciones)
                  .SetEase(Ease.InQuad);
            }
        }

        yield return new WaitForSeconds(durMax);
    }

    /// <summary>
    /// Temblor de las opciones cuando se intenta continuar sin marcar ninguna.
    /// </summary>
    private void ShakeOptions()
    {
        // Sacudir cada opción
        foreach (var t in togglesInstanciados)
        {
            if (t == null) continue;

            RectTransform rt = t.transform as RectTransform;
            if (rt == null) continue;

            // completar tween actual para volver a la posición original
            rt.DOKill(true);

            // Temblor horizontal suave tipo sismo
            rt.DOShakeAnchorPos(
                shakeDuration,
                new Vector2(shakeStrength, 0f),
                shakeVibrato,
                90f,
                false,
                true
            );
        }
    }

    #endregion

    #region TYPEWRITER

    private IEnumerator TypeText(TMP_Text textComponent, string fullText, float delayPerChar)
    {
        if (textComponent == null || string.IsNullOrEmpty(fullText))
            yield break;

        textComponent.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            textComponent.text += fullText[i];

            // rápido, pero con un pelín de delay para que se note
            if (delayPerChar > 0f)
                yield return new WaitForSeconds(delayPerChar);
            else
                yield return null;
        }
    }

    #endregion
}
