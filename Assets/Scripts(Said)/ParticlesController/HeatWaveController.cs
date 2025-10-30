using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HeatWaveController : MonoBehaviour
{
    [Header("Post Process Volume")]
    [SerializeField] private Volume postProcessVolume;

    [Header("Intensidad")]
    [Range(0f, 1f)] public float intensidadObjetivoEnFuego = 1f;
    public float velocidadSubida = 1.5f;
    public float velocidadBajada = 1.0f;
    [Range(0f, 1f)] public float intensidadActual = 0f;

    [Header("Lens Distortion (aire temblando)")]
    public float distorsionMax = -0.4f;
    public float amplitudOndas = 0.15f;
    public float frecuenciaOndas = 6f;

    [Header("Bloom / Brillo caliente")]
    public float bloomIntensidadMax = 1.2f;

    [Header("Color caliente")]
    public float postExposureMax = 0.8f;
    public Color colorFiltroCaliente = new Color(1f, 0.55f, 0.3f, 1f);
    [Range(0f, 1f)] public float colorFiltroStrength = 0.6f;

    private LensDistortion m_LensDistortion;
    private Bloom m_Bloom;
    private ColorAdjustments m_ColorAdj;

    private bool enZonaCalor = false;

    void Awake()
    {
        if (postProcessVolume == null || postProcessVolume.profile == null)
        {
            Debug.LogError("HeatWaveController: Asigna un Volume con LensDistortion / Bloom / ColorAdjustments.");
            enabled = false;
            return;
        }

        postProcessVolume.profile.TryGet(out m_LensDistortion);
        postProcessVolume.profile.TryGet(out m_Bloom);
        postProcessVolume.profile.TryGet(out m_ColorAdj);

        if (m_LensDistortion == null)
            Debug.LogWarning("HeatWaveController: Falta LensDistortion en el Volume.");
        if (m_Bloom == null)
            Debug.LogWarning("HeatWaveController: Falta Bloom en el Volume.");
        if (m_ColorAdj == null)
            Debug.LogWarning("HeatWaveController: Falta ColorAdjustments en el Volume.");
    }

    void Update()
    {
        float target = enZonaCalor ? intensidadObjetivoEnFuego : 0f;
        float vel = enZonaCalor ? velocidadSubida : velocidadBajada;
        intensidadActual = Mathf.MoveTowards(intensidadActual, target, vel * Time.deltaTime);

        float onda = Mathf.Sin(Time.time * frecuenciaOndas);
        float aporteOnda = onda * amplitudOndas * intensidadActual;

        if (m_LensDistortion != null)
        {
            float baseDistorsion = distorsionMax * intensidadActual;
            float finalDistorsion = baseDistorsion + aporteOnda;
            m_LensDistortion.intensity.value = finalDistorsion;
            m_LensDistortion.scale.value = Mathf.Lerp(1f, 0.9f, intensidadActual);
        }

        if (m_Bloom != null)
        {
            float bloomExtra = bloomIntensidadMax * intensidadActual;
            m_Bloom.intensity.value = bloomExtra;
        }

        if (m_ColorAdj != null)
        {
            float exposure = Mathf.Lerp(0f, postExposureMax, intensidadActual);
            m_ColorAdj.postExposure.value = exposure;

            Color finalColor = Color.Lerp(
                Color.white,
                colorFiltroCaliente,
                colorFiltroStrength * intensidadActual
            );
            m_ColorAdj.colorFilter.value = finalColor;
        }
    }

    // --- API pública que vamos a llamar desde el GameManager/Eventos ---

    public void ActivarCalor()
    {
        enZonaCalor = true;
    }

    public void DesactivarCalor()
    {
        enZonaCalor = false;
    }

    public void PicoInstantaneo(float fuerza = 1f)
    {
        intensidadActual = Mathf.Clamp01(fuerza);
        enZonaCalor = true;
    }

    void OnDisable()
    {
        ResetVisual();
    }

    private void ResetVisual()
    {
        if (m_LensDistortion != null)
        {
            m_LensDistortion.intensity.value = 0f;
            m_LensDistortion.scale.value = 1f;
        }
        if (m_Bloom != null)
        {
            m_Bloom.intensity.value = 0f;
        }
        if (m_ColorAdj != null)
        {
            m_ColorAdj.postExposure.value = 0f;
            m_ColorAdj.colorFilter.value = Color.white;
        }
    }
}
