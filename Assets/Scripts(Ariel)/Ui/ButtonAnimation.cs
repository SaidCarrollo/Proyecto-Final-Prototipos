using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening; // No olvides importar la librería de DOTween

public class BotonAnimado : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración de la Animación")]
    [Tooltip("A qué escala crecerá el botón. 1.1 es un 10% más grande.")]
    [SerializeField] private float escalaHover = 1.1f;

    [Tooltip("Cuánto tiempo durará la animación de entrada y salida.")]
    [SerializeField] private float duracion = 0.2f;

    private Vector3 escalaOriginal;

    private void Awake()
    {
        // Guardamos la escala original del botón al iniciar.
        // Es importante hacerlo para no tener problemas si los botones tienen tamaños iniciales diferentes.
        escalaOriginal = transform.localScale;
    }

    // Esta función se llama cuando el puntero del mouse entra en el botón
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Detenemos cualquier animación de escala previa para evitar conflictos
        transform.DOKill();

        // Creamos la animación de crecimiento
        transform.DOScale(escalaOriginal * escalaHover, duracion)
            .SetEase(Ease.OutBack) // Un efecto "elástico" que queda muy bien
            .SetUpdate(true); // ¡LA CLAVE! Ignora el Time.timeScale
    }

    // Esta función se llama cuando el puntero del mouse sale del botón
    public void OnPointerExit(PointerEventData eventData)
    {
        // Detenemos cualquier animación de escala previa
        transform.DOKill();

        // Creamos la animación para volver a la escala original
        transform.DOScale(escalaOriginal, duracion)
            .SetUpdate(true); // También lo necesita para funcionar al salir del botón en pausa
    }
    public void QuitApplication()
    {
        Application.Quit();
    }
}