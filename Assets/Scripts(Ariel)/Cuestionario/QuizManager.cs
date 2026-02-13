using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using DG.Tweening; // Importante para la nueva animación

public enum EscenarioTipo
{
    FuegoCocina,
    FugaGas,
    SismoNocturno
}

public class QuizManager : MonoBehaviour
{
    [Header("Configuración de Escenario (PlayerPrefs)")]
    public EscenarioTipo escenarioActual;
    public bool esPostGame;

    [Header("Datos del Cuestionario")]
    public CuestionarioSO cuestionario;
    public ResultadosDelQuizSO resultadosGuardados;

    [Header("UI - Elementos de la Pregunta")]
    public TextMeshProUGUI textoPregunta;
    public GameObject grupoDeOpciones;
    public GameObject opcionRespuestaPrefab;

    [Header("UI - Feedback")]
    public Sprite iconoCorrecto;
    public Sprite iconoIncorrecto;
    public Button botonSiguientePregunta;
    public Button botonEmpezarNivel;

    [Header("UI - Funcionalidad Extra")]
    public Button botonSaltar;

    [Header("Eventos")]
    public UnityEvent alFinalizarQuiz;

    [Header("Animación de Tarjeta (DOTween)")]
    [SerializeField] private float optionDuration = 0.6f;
    [SerializeField] private float optionCascadeDelay = 0.12f;
    [SerializeField] private float initialScale = 0.7f;
    [SerializeField] private Ease rotationEase = Ease.OutBack;

    [Header("Colores de Selección")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(0.8f, 0.9f, 1f);

    private int preguntaActualIndex = 0;
    private List<Button> botonesInstanciados = new List<Button>();
    private int indiceSeleccionado = -1;
    private bool respuestaEnviada = false;

    // ELIMINADO: private Coroutine questionTypewriterCoroutine; 
    private List<Coroutine> optionTypewriterCoroutines = new List<Coroutine>();
    private Dictionary<TextMeshProUGUI, string> opcionTextosCompletos = new Dictionary<TextMeshProUGUI, string>();

    void Start()
    {
        if (resultadosGuardados != null) resultadosGuardados.LimpiarResultados();
        botonEmpezarNivel.gameObject.SetActive(false);
        botonSiguientePregunta.onClick.AddListener(OnBotonSiguienteClick);

        ConfigurarBotonSaltar();
        CargarPregunta();
    }

    private string ObtenerKeyPlayerPref()
    {
        string tipoEvaluacion = esPostGame ? "Post" : "Pre";
        return $"QuizCompleted_{escenarioActual}_{tipoEvaluacion}";
    }

    private void ConfigurarBotonSaltar()
    {
        if (botonSaltar == null) return;
        string key = ObtenerKeyPlayerPref();
        bool yaCompletado = PlayerPrefs.GetInt(key, 0) == 1;

        if (yaCompletado)
        {
            botonSaltar.gameObject.SetActive(true);
            botonSaltar.onClick.RemoveAllListeners();
            botonSaltar.onClick.AddListener(SaltarCuestionario);
        }
        else
        {
            botonSaltar.gameObject.SetActive(false);
        }
    }

    public void SaltarCuestionario()
    {
        SoundManager.Instance?.PlaySFX("Click");
        LimpiarCorutinasYBotones();
        botonEmpezarNivel.gameObject.SetActive(true);
        alFinalizarQuiz.Invoke();
    }

    void CargarPregunta()
    {
        respuestaEnviada = false;
        indiceSeleccionado = -1;

        LimpiarCorutinasYBotones();

        if (preguntaActualIndex >= cuestionario.preguntas.Length)
        {
            MostrarFinDelQuiz();
            return;
        }

        ConfigurarBotonesNavegacion();
        DesactivarBotones(true);

        Pregunta pregunta = cuestionario.preguntas[preguntaActualIndex];

        // --- NUEVA LÓGICA DE ANIMACIÓN PARA LA PREGUNTA ---
        if (textoPregunta != null)
        {
            AnimarTextoPregunta(pregunta.textoPregunta);
        }
        // --------------------------------------------------

        for (int i = 0; i < pregunta.respuestas.Length; i++)
        {
            GameObject nuevaOpcion = Instantiate(opcionRespuestaPrefab, grupoDeOpciones.transform);
            Button btn = nuevaOpcion.GetComponent<Button>();
            botonesInstanciados.Add(btn);

            int index = i;
            btn.onClick.AddListener(() => SeleccionarOpcion(index));

            Image marco = btn.transform.Find("MarcoImagen")?.GetComponent<Image>();
            if (marco != null) marco.color = normalColor;

            Image iconoRepresentativo = marco?.transform.Find("Image")?.GetComponent<Image>();
            if (iconoRepresentativo != null)
            {
                if (pregunta.respuestas[i].imagenOpcion != null)
                {
                    iconoRepresentativo.sprite = pregunta.respuestas[i].imagenOpcion;
                    iconoRepresentativo.gameObject.SetActive(true);
                    iconoRepresentativo.transform.localScale = Vector3.one;
                }
                else
                {
                    iconoRepresentativo.gameObject.SetActive(false);
                }
            }

            TextMeshProUGUI tmpTexto = btn.transform.Find("Texto")?.GetComponent<TextMeshProUGUI>();
            if (tmpTexto != null)
            {
                opcionTextosCompletos[tmpTexto] = pregunta.respuestas[i].textoRespuesta;
                tmpTexto.text = ""; // Las opciones siguen usando TypeText si quieres, o puedes cambiarlo también
            }

            nuevaOpcion.transform.DOKill();
            nuevaOpcion.transform.localRotation = Quaternion.Euler(0, 90, 0);
            nuevaOpcion.transform.localScale = Vector3.one * initialScale;

            CanvasGroup cg = nuevaOpcion.GetComponent<CanvasGroup>() ?? nuevaOpcion.AddComponent<CanvasGroup>();
            cg.alpha = 0;
        }

        AnimarEntradaTarjetas();
    }

    // --- NUEVO MÉTODO PARA ANIMAR EL TEXTO DE LA PREGUNTA ---
    private void AnimarTextoPregunta(string texto)
    {
        // 1. Matar animaciones previas para evitar conflictos
        textoPregunta.transform.DOKill();
        textoPregunta.DOKill();

        // 2. Setear el texto
        textoPregunta.text = texto;

        // 3. Resetear estado inicial (Transparente)
        textoPregunta.alpha = 0;

        // 4. ANIMACIÓN ASCENDENTE
        // Movemos el texto 50 unidades hacia abajo RELATIVAMENTE y hacemos que suba a su posición original
        // El true en From(true) indica que es una posición relativa
        textoPregunta.rectTransform.DOAnchorPosY(-50f, 0.8f).From(true).SetEase(Ease.OutBack);

        // 5. FADE IN
        textoPregunta.DOFade(1f, 0.8f);
    }
    // --------------------------------------------------------

    void SeleccionarOpcion(int index)
    {
        if (respuestaEnviada) return;

        indiceSeleccionado = index;
        SoundManager.Instance?.PlaySFX("Click");

        for (int i = 0; i < botonesInstanciados.Count; i++)
        {
            Image marco = botonesInstanciados[i].transform.Find("MarcoImagen")?.GetComponent<Image>();
            if (marco != null)
            {
                marco.DOColor(i == index ? selectedColor : normalColor, 0.2f);
                botonesInstanciados[i].transform.DOScale(i == index ? 1.05f : 1.0f, 0.2f);
            }
        }
    }

    private void AnimarEntradaTarjetas()
    {
        for (int i = 0; i < botonesInstanciados.Count; i++)
        {
            GameObject obj = botonesInstanciados[i].gameObject;
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            float delay = i * optionCascadeDelay;

            obj.transform.DOLocalRotate(Vector3.zero, optionDuration)
                .SetDelay(delay)
                .SetEase(rotationEase);

            obj.transform.DOScale(1f, optionDuration)
                .SetDelay(delay)
                .SetEase(rotationEase)
                .OnComplete(() => {
                    if (cg != null) cg.blocksRaycasts = true;
                });

            cg.DOFade(1f, optionDuration).SetDelay(delay);

            int index = i;
            // Mantenemos la animación de escritura en las OPCIONES para que se vea dinámico,
            // pero si quieres cambiar esto también avísame.
            DOVirtual.DelayedCall(delay + (optionDuration * 0.4f), () => {
                if (index < botonesInstanciados.Count && botonesInstanciados[index] != null)
                {
                    TextMeshProUGUI tmp = botonesInstanciados[index].transform.Find("Texto").GetComponent<TextMeshProUGUI>();
                    var c = StartCoroutine(TypeText(tmp, opcionTextosCompletos[tmp], 0.01f));
                    optionTypewriterCoroutines.Add(c);
                }
            });
        }
    }

    public void OnBotonSiguienteClick()
    {
        if (respuestaEnviada || indiceSeleccionado == -1)
        {
            if (indiceSeleccionado == -1) ShakeOptions();
            return;
        }
        StartCoroutine(CorregirYContinuar());
    }

    private IEnumerator CorregirYContinuar()
    {
        respuestaEnviada = true;
        DesactivarBotones(false);
        if (botonSaltar != null) botonSaltar.interactable = false;

        Pregunta preguntaActual = cuestionario.preguntas[preguntaActualIndex];
        bool esCorrecta = preguntaActual.respuestas[indiceSeleccionado].esCorrecta;

        Transform marcoObj = botonesInstanciados[indiceSeleccionado].transform.Find("MarcoImagen");
        Image marcoImg = marcoObj.GetComponent<Image>();
        Image iconoImg = marcoObj.Find("Image")?.GetComponent<Image>();

        if (iconoImg != null)
        {
            iconoImg.sprite = esCorrecta ? iconoCorrecto : iconoIncorrecto;
            iconoImg.gameObject.SetActive(true);
            iconoImg.transform.localScale = Vector3.zero;
            iconoImg.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack);
        }

        marcoImg.DOColor(esCorrecta ? Color.green : Color.red, 0.4f);

        if (resultadosGuardados != null)
            resultadosGuardados.resultados.Add(new ResultadoPregunta(preguntaActual, indiceSeleccionado));

        yield return new WaitForSeconds(1.8f);
        yield return AnimarSalida();

        if (preguntaActualIndex >= cuestionario.preguntas.Length - 1)
            FinalizarQuiz();
        else
        {
            preguntaActualIndex++;
            if (botonSaltar != null) botonSaltar.interactable = true;
            CargarPregunta();
        }
    }

    private void LimpiarCorutinasYBotones()
    {
        // ELIMINADO: if (questionTypewriterCoroutine != null) StopCoroutine(questionTypewriterCoroutine);

        foreach (var c in optionTypewriterCoroutines) if (c != null) StopCoroutine(c);
        optionTypewriterCoroutines.Clear();
        opcionTextosCompletos.Clear();

        foreach (Button b in botonesInstanciados)
        {
            if (b != null) { b.transform.DOKill(); Destroy(b.gameObject); }
        }
        botonesInstanciados.Clear();
    }

    private void ShakeOptions()
    {
        SoundManager.Instance?.PlaySFX("Click");
        foreach (var b in botonesInstanciados)
        {
            b.transform.DOComplete();
            b.transform.DOShakePosition(0.4f, new Vector3(15f, 0, 0), 10, 90, false, true);
        }
    }

    private void DesactivarBotones(bool interactable)
    {
        foreach (var b in botonesInstanciados) b.interactable = interactable;
        bool esUltima = (preguntaActualIndex == cuestionario.preguntas.Length - 1);
        if (botonSiguientePregunta != null) botonSiguientePregunta.interactable = interactable && !esUltima;
        if (botonEmpezarNivel != null) botonEmpezarNivel.interactable = interactable && esUltima;
    }

    private IEnumerator AnimarSalida()
    {
        foreach (var b in botonesInstanciados)
        {
            if (b == null) continue;
            if (b.TryGetComponent<CanvasGroup>(out var cg)) cg.blocksRaycasts = false;

            b.transform.DOLocalRotate(new Vector3(0, -90, 0), 0.3f).SetEase(Ease.InQuad);
            b.transform.DOScale(0.5f, 0.3f);
            if (cg != null) cg.DOFade(0, 0.3f);
        }

        // También desvanecemos la pregunta actual al salir
        textoPregunta.DOFade(0, 0.3f);

        yield return new WaitForSeconds(0.3f);
    }

    private void ConfigurarBotonesNavegacion()
    {
        bool esUltima = (preguntaActualIndex == cuestionario.preguntas.Length - 1);
        botonSiguientePregunta.gameObject.SetActive(!esUltima);
        botonEmpezarNivel.gameObject.SetActive(esUltima);
    }

    private void FinalizarQuiz()
    {
        string key = ObtenerKeyPlayerPref();
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        resultadosGuardados?.GuardarResultados();
        alFinalizarQuiz.Invoke();
    }

    private void MostrarFinDelQuiz()
    {
        AnimarTextoPregunta("¡Excelente trabajo!"); // Usamos la nueva animación aquí también
        grupoDeOpciones.SetActive(false);
        botonEmpezarNivel.gameObject.SetActive(true);
        botonEmpezarNivel.interactable = true;
        if (botonSaltar != null) botonSaltar.gameObject.SetActive(false);
    }

    // Esta corrutina se mantiene SOLO para las respuestas pequeñas, si deseas
    private IEnumerator TypeText(TMP_Text textComponent, string fullText, float delay)
    {
        textComponent.text = "";
        foreach (char c in fullText)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(delay);
        }
    }
}