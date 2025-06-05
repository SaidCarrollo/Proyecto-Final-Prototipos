
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
        emissionModule = ps.emission; 
    }

    public void SetIntensity(float intensity) 
    {
        if (intensity <= 0.01f && ps.isPlaying) 
        {
            emissionModule.enabled = false; 
        }
        else
        {
            if (!emissionModule.enabled)
            {
                emissionModule.enabled = true; 
            }
            if (!ps.isPlaying && intensity > 0.01f)
            {
                ps.Play();
            }
            emissionModule.rateOverTime = Mathf.Lerp(minEmissionRate, maxEmissionRate, intensity);
        }
        Debug.Log($"Particle intensity set to: {intensity}, Emission rate: {emissionModule.rateOverTime.constant}");
    }

    public void SetParticlesActive(bool isActive)
    {
        if (isActive)
        {
            if (!emissionModule.enabled) emissionModule.enabled = true;
            if (!ps.isPlaying) ps.Play();
            emissionModule.rateOverTime = maxEmissionRate; 
        }
        else
        {
            emissionModule.enabled = false;
        }
    }

}