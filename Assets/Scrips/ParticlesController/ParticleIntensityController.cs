// ParticleIntensityController.cs
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleIntensityController : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystem.EmissionModule emissionModule;

    public float minEmissionRate = 5f;
    public float maxEmissionRate = 100f;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        emissionModule = ps.emission; // Guardar el m�dulo de emisi�n
    }

    // Este m�todo ser� llamado por el FloatEventListener
    public void SetIntensity(float intensity) // intensity ser� un valor entre 0.0 y 1.0
    {
        if (intensity <= 0.01f && ps.isPlaying) // Un peque�o umbral para apagar
        {
            emissionModule.enabled = false; // Desactiva el m�dulo de emisi�n
            // Opcional: podr�as querer detener el sistema de part�culas completamente
            // ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        else
        {
            if (!emissionModule.enabled)
            {
                emissionModule.enabled = true; // Reactiva el m�dulo de emisi�n si estaba apagado
            }
            if (!ps.isPlaying && intensity > 0.01f)
            {
                ps.Play();
            }
            emissionModule.rateOverTime = Mathf.Lerp(minEmissionRate, maxEmissionRate, intensity);
        }
        Debug.Log($"Particle intensity set to: {intensity}, Emission rate: {emissionModule.rateOverTime.constant}");
    }

    // M�todo para encender/apagar directamente si prefieres un booleano
    public void SetParticlesActive(bool isActive)
    {
        if (isActive)
        {
            if (!emissionModule.enabled) emissionModule.enabled = true;
            if (!ps.isPlaying) ps.Play();
            emissionModule.rateOverTime = maxEmissionRate; // O una tasa por defecto al encender
        }
        else
        {
            emissionModule.enabled = false;
            // ps.Stop(true, ParticleSystemStopBehavior.StopEmitting); // Opci�n m�s agresiva
        }
    }
}