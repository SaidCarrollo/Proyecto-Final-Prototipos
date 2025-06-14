
using UnityEngine;

public class FireTimer : MonoBehaviour
{
    [Header("Configuración de Tiempo")]
    [SerializeField] private float tiempoEspera = 20f;
    [SerializeField] private float nuevaIntensidad = 0.8f;

    [Header("Events")]
    [Tooltip("Evento para cambiar la intensidad del fuego.")]
    [SerializeField] private FloatEvent fireIntensityEvent;

    [Tooltip("Evento que se dispara para mostrar un mensaje en la UI.")]
    [SerializeField] private GameEventstring messageEvent;
    [SerializeField] private GameEvent onPlayerDeathEvent;
    [Header("Vignette Event")]
    [Tooltip("Evento para activar la viñeta.")]
    [SerializeField] private VignetteEvent vignetteEvent;

    private void Start()
    {
        StartCoroutine(EsperarYAumentarFuego());
    }

    private System.Collections.IEnumerator EsperarYAumentarFuego()
    {
        yield return new WaitForSeconds(tiempoEspera);

        if (fireIntensityEvent != null)
        {
            fireIntensityEvent.Raise(nuevaIntensidad);
        }

        if (messageEvent != null)
        {
            messageEvent.Raise("¡Fuego descontrolado!");
            Debug.Log("EVENTO DE MENSAJE: ¡Fuego descontrolado! PUBLICADO");
        }

        if (vignetteEvent != null)
        {
            vignetteEvent.Raise(Color.red, 0.5f, 3f); 
        }
        if (onPlayerDeathEvent != null)
        {
            onPlayerDeathEvent.Raise();
            Debug.Log("EVENTO DE MUERTE PUBLICADO");
        }
    }

    public void ReiniciarTemporizador()
    {
        StopAllCoroutines();
        StartCoroutine(EsperarYAumentarFuego());
    }

    public void ConfigurarTemporizador(float nuevoTiempo, float intensidad)
    {
        tiempoEspera = nuevoTiempo;
        nuevaIntensidad = Mathf.Clamp01(intensidad);
    }
}