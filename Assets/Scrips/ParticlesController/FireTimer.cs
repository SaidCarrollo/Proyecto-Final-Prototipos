using UnityEngine;

public class FireTimer : MonoBehaviour
{
    [Header("Configuración de Tiempo")]
    [SerializeField] private float tiempoEspera = 20f;
    [SerializeField] private float nuevaIntensidad = 0.8f; 

    [Header("Evento de Fuego")]
    [SerializeField] private FloatEvent fireIntensityEvent;

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
            Debug.Log($"Intensidad del fuego aumentada a {nuevaIntensidad} después de {tiempoEspera} segundos");
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