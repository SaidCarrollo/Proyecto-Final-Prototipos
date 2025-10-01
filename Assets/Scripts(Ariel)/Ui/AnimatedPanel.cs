using UnityEngine;
using DG.Tweening; // No olvides importar la librer�a de DOTween

// Asegura que el GameObject tenga siempre un CanvasGroup.
[RequireComponent(typeof(CanvasGroup))]
public class PanelAnimado : MonoBehaviour
{
    [Header("Configuraci�n de Animaci�n")]
    [Tooltip("Cu�nto dura la animaci�n de entrada y salida.")]
    [SerializeField] private float duracion = 0.3f;

    [Tooltip("La escala inicial desde la que aparecer� el panel (ej: 0.9 para un efecto 'pop').")]
    [SerializeField] private float escalaInicial = 0.9f;

    [Tooltip("El tipo de 'Ease' para la animaci�n de entrada.")]
    [SerializeField] private Ease easeEntrada = Ease.OutBack;

    [Tooltip("El tipo de 'Ease' para la animaci�n de salida.")]
    [SerializeField] private Ease easeSalida = Ease.InQuad;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        // Guardamos las referencias a los componentes para usarlos despu�s.
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    // OnEnable se llama autom�ticamente cada vez que el objeto se activa (SetActive(true))
    private void OnEnable()
    {
        // 1. Preparamos el estado inicial (invisible y peque�o)
        canvasGroup.alpha = 0f;
        rectTransform.localScale = new Vector3(escalaInicial, escalaInicial, escalaInicial);

        // 2. Creamos las animaciones de entrada
        // Animaci�n de escala para el efecto "pop"
        rectTransform.DOScale(1f, duracion)
            .SetEase(easeEntrada)
            .SetUpdate(true); // Ignora el Time.timeScale por si el men� est� en pausa

        // Animaci�n de fade para aparecer suavemente
        canvasGroup.DOFade(1f, duracion)
            .SetUpdate(true);
    }

    // Este es el m�todo que DEBES llamar para cerrar el panel
    public void CerrarPanel()
    {
        // Detenemos animaciones previas para evitar conflictos
        rectTransform.DOKill();
        canvasGroup.DOKill();

        // Creamos las animaciones de salida
        rectTransform.DOScale(escalaInicial, duracion)
            .SetEase(easeSalida)
            .SetUpdate(true);

        canvasGroup.DOFade(0f, duracion)
            .SetUpdate(true)
            .OnComplete(() => {
                // ESTA ES LA MAGIA:
                // Solo cuando la animaci�n de fade termina, desactivamos el objeto.
                gameObject.SetActive(false);
            });
    }
}