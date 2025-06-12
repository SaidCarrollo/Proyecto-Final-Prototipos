// VignetteController.cs
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // O UnityEngine.Rendering.HighDefinition si usas HDRP

public class VignetteController : MonoBehaviour
{
    [SerializeField] private Volume postProcessVolume;
    private Vignette vignette;

    private Coroutine activeVignetteCoroutine;

    private void Start()
    {
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out vignette))
        {
            // Opcional: Asegurarse de que la viñeta está desactivada al inicio.
            vignette.intensity.value = 0f;
        }
        else
        {
            Debug.LogError("No se encontró un perfil de volumen o un efecto de viñeta.");
        }
    }

    public void TriggerVignette(Color color, float intensity, float duration)
    {
        if (vignette == null) return;

        if (activeVignetteCoroutine != null)
        {
            StopCoroutine(activeVignetteCoroutine);
        }
        activeVignetteCoroutine = StartCoroutine(VignetteCoroutine(color, intensity, duration));
    }

    private System.Collections.IEnumerator VignetteCoroutine(Color color, float intensity, float duration)
    {
        vignette.color.value = color;
        vignette.intensity.value = intensity;

        yield return new WaitForSeconds(duration);

        // Fade out
        float elapsedTime = 0f;
        float startIntensity = vignette.intensity.value;
        while (elapsedTime < 1f) // 1 segundo de fade out
        {
            elapsedTime += Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(startIntensity, 0f, elapsedTime / 1f);
            yield return null;
        }

        vignette.intensity.value = 0f;
        activeVignetteCoroutine = null;
    }
}