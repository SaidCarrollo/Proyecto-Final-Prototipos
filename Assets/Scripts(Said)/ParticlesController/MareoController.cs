using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class MareoController : MonoBehaviour
{
    [Header("Configuraci�n de Efectos")]
    [Tooltip("El Volume global que contiene los overrides de post-procesado.")]
    public Volume postProcessVolume;

    [Tooltip("Velocidad del 'pulso' o 'respiraci�n' del efecto.")]
    public float velocidadPulso = 1.5f;

    [Tooltip("Intensidad m�xima de la distorsi�n (tipo 'barril' o 'pellizco').")]
    public float intensidadMaxDistorsion = -0.6f;

    [Tooltip("Intensidad m�xima de la aberraci�n crom�tica (desfase de colores).")]
    public float intensidadMaxAberracion = 0.8f;

    [Tooltip("Intensidad m�xima de la vi�eta (oscurecer bordes).")]
    public float intensidadMaxVi�eta = 0.5f;

    // Referencias cacheadas a los efectos
    private LensDistortion m_LensDistortion;
    private ChromaticAberration m_ChromaticAberration;
    private Vignette m_Vignette;

    // Corutina activa
    private Coroutine efectoMareoActual;

    void Awake()
    {
        // Es crucial obtener las referencias a los overrides del perfil del Volume.
        // Si no los encuentra, lanzar� un error.
        if (postProcessVolume == null || postProcessVolume.profile == null)
        {
            Debug.LogError("Asigna un Volume de Post-Procesado en el Inspector.");
            this.enabled = false;
            return;
        }

        if (!postProcessVolume.profile.TryGet(out m_LensDistortion))
            Debug.LogError("El perfil del Volume NO tiene un override de 'Lens Distortion'. A��delo.");

        if (!postProcessVolume.profile.TryGet(out m_ChromaticAberration))
            Debug.LogError("El perfil del Volume NO tiene un override de 'Chromatic Aberration'. A��delo.");

        if (!postProcessVolume.profile.TryGet(out m_Vignette))
            Debug.LogError("El perfil del Volume NO tiene un override de 'Vignette'. A��delo.");
    }

    /// <summary>
    /// M�todo p�blico para iniciar el efecto de mareo.
    /// </summary>
    /// <param name="duracion">Cu�nto tiempo (en segundos) debe durar el efecto.</param>
    public void IniciarEfectoMareo(float duracion)
    {
        // Si ya hay un efecto corriendo, lo detenemos antes de iniciar uno nuevo.
        // Esto asegura que no se solapen.
        if (efectoMareoActual != null)
        {
            StopCoroutine(efectoMareoActual);
        }

        // Iniciamos la nueva corutina y guardamos su referencia.
        efectoMareoActual = StartCoroutine(EfectoMareoCoroutine(duracion));
    }

    /// <summary>
    /// La Corutina que anima los valores del post-procesado.
    /// </summary>
    private IEnumerator EfectoMareoCoroutine(float duracion)
    {
        float tiempoPasado = 0f;

        while (tiempoPasado < duracion)
        {
            tiempoPasado += Time.deltaTime;

            float fade = Mathf.Sin((tiempoPasado / duracion) * Mathf.PI);

            float pulso = (Mathf.Sin(Time.time * velocidadPulso) * 0.5f) + 0.5f;

            if (m_LensDistortion != null)
                m_LensDistortion.intensity.value = Mathf.Lerp(0, intensidadMaxDistorsion, pulso) * fade;

            if (m_ChromaticAberration != null)
                m_ChromaticAberration.intensity.value = Mathf.Lerp(0, intensidadMaxAberracion, pulso) * fade;

            if (m_Vignette != null)
                m_Vignette.intensity.value = Mathf.Lerp(0, intensidadMaxVi�eta, pulso) * fade;

            // Espera al siguiente frame
            yield return null;
        }

        ResetearEfectos();
        efectoMareoActual = null; 
    }

    private void ResetearEfectos()
    {
        if (m_LensDistortion != null) m_LensDistortion.intensity.value = 0f;
        if (m_ChromaticAberration != null) m_ChromaticAberration.intensity.value = 0f;
        if (m_Vignette != null) m_Vignette.intensity.value = 0f;
    }

    void OnDisable()
    {
        ResetearEfectos();
        if (efectoMareoActual != null)
        {
            StopCoroutine(efectoMareoActual);
            efectoMareoActual = null;
        }
    }
}
