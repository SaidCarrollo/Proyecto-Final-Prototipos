
using UnityEngine;

public class FireTimer : MonoBehaviour
{
    [Header("Configuración de Tiempo")]
    [SerializeField] private float tiempoParaDescontrol = 20f;
    [SerializeField] private float intensidadFuegoDescontrolado = 0.8f;

    [Header("Events")]
    [Tooltip("Evento para cambiar la intensidad del fuego.")]
    [SerializeField] private FloatEvent fireIntensityEvent;

    [Tooltip("Evento que se dispara para mostrar un mensaje en la UI.")]
    [SerializeField] private GameEventstring messageEvent;

    [Tooltip("Evento que se dispara cuando el fuego se sale de control.")]
    [SerializeField] private GameEvent onUncontrolledFireEvent;
    [SerializeField] private BadgeManager badgeManager;
    [Header("Vignette Event")]
    [Tooltip("Evento para activar la viñeta de peligro.")]
    [SerializeField] private VignetteEvent vignetteEvent;

    private void Start()
    {
        StartCoroutine(EsperarYDescontrolarFuego());
    }

    private System.Collections.IEnumerator EsperarYDescontrolarFuego()
    {
        yield return new WaitForSeconds(tiempoParaDescontrol);

        if (fireIntensityEvent != null)
        {
            fireIntensityEvent.Raise(intensidadFuegoDescontrolado);
        }

        if (messageEvent != null)
        {
            messageEvent.Raise("Tardé demasiado... Es mejor salir de aquí…");
            badgeManager.UnlockBadge("Descontrol");
            Debug.Log("EVENTO DE MENSAJE: ¡Fuego descontrolado! PUBLICADO");
        }

        if (vignetteEvent != null)
        {
            vignetteEvent.Raise(Color.red, 0.5f, 3f);
        }

        if (onUncontrolledFireEvent != null)
        {
            onUncontrolledFireEvent.Raise(); 
            Debug.Log("EVENTO DE FUEGO DESCONTROLADO PUBLICADO");
        }
    }

    public void ReiniciarTemporizador()
    {
        StopAllCoroutines();
        StartCoroutine(EsperarYDescontrolarFuego());
    }

    public void ConfigurarTemporizador(float nuevoTiempo, float intensidad)
    {
        tiempoParaDescontrol = nuevoTiempo;
        intensidadFuegoDescontrolado = Mathf.Clamp01(intensidad);
    }
}