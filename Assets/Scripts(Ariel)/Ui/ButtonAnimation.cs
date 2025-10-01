using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening; // No olvides importar la librer�a de DOTween

public class BotonAnimado : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuraci�n de la Animaci�n")]
    [Tooltip("A qu� escala crecer� el bot�n. 1.1 es un 10% m�s grande.")]
    [SerializeField] private float escalaHover = 1.1f;

    [Tooltip("Cu�nto tiempo durar� la animaci�n de entrada y salida.")]
    [SerializeField] private float duracion = 0.2f;

    private Vector3 escalaOriginal;

    private void Awake()
    {
        // Guardamos la escala original del bot�n al iniciar.
        // Es importante hacerlo para no tener problemas si los botones tienen tama�os iniciales diferentes.
        escalaOriginal = transform.localScale;
    }

    // Esta funci�n se llama cuando el puntero del mouse entra en el bot�n
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Detenemos cualquier animaci�n de escala previa para evitar conflictos
        transform.DOKill();

        // Creamos la animaci�n de crecimiento
        transform.DOScale(escalaOriginal * escalaHover, duracion)
            .SetEase(Ease.OutBack) // Un efecto "el�stico" que queda muy bien
            .SetUpdate(true); // �LA CLAVE! Ignora el Time.timeScale
    }

    // Esta funci�n se llama cuando el puntero del mouse sale del bot�n
    public void OnPointerExit(PointerEventData eventData)
    {
        // Detenemos cualquier animaci�n de escala previa
        transform.DOKill();

        // Creamos la animaci�n para volver a la escala original
        transform.DOScale(escalaOriginal, duracion)
            .SetUpdate(true); // Tambi�n lo necesita para funcionar al salir del bot�n en pausa
    }
    public void QuitApplication()
    {
        Application.Quit();
    }
}