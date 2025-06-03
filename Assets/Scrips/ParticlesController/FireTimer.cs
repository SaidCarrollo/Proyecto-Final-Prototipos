using UnityEngine;

public class FireTimer : MonoBehaviour
{
    [Header("Configuraci�n de Tiempo")]
    [SerializeField] private float tiempoEspera = 20f;
    [SerializeField] private float nuevaIntensidad = 0.8f; // Valor entre 0 y 1

    [Header("Evento de Fuego")]
    [SerializeField] private FloatEvent fireIntensityEvent;

    private void Start()
    {
        // Iniciar el temporizador al comienzo
        StartCoroutine(EsperarYAumentarFuego());
    }

    private System.Collections.IEnumerator EsperarYAumentarFuego()
    {
        // Esperar el tiempo configurado
        yield return new WaitForSeconds(tiempoEspera);

        // Disparar el evento con la nueva intensidad
        if (fireIntensityEvent != null)
        {
            fireIntensityEvent.Raise(nuevaIntensidad);
            Debug.Log($"Intensidad del fuego aumentada a {nuevaIntensidad} despu�s de {tiempoEspera} segundos");
        }
    }

    // M�todo p�blico para reiniciar el temporizador
    public void ReiniciarTemporizador()
    {
        StopAllCoroutines();
        StartCoroutine(EsperarYAumentarFuego());
    }

    // M�todo para configurar nuevos valores
    public void ConfigurarTemporizador(float nuevoTiempo, float intensidad)
    {
        tiempoEspera = nuevoTiempo;
        nuevaIntensidad = Mathf.Clamp01(intensidad);
    }
}