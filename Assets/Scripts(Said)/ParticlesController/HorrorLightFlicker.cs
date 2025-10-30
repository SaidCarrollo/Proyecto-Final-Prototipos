using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class HorrorLightFlicker : MonoBehaviour
{
    [Header("Luces (arrastra los GameObjects que contienen la Light o Light2D)")]
    public List<GameObject> lights = new List<GameObject>();

    [Header("Reglas generales")]
    [Tooltip("Número mínimo de luces que deben permanecer ENCENDIDAS en todo momento.")]
    public int minLightsOn = 1;

    [Header("Tiempos (segundos)")]
    public float minOnTime = 0.8f;
    public float maxOnTime = 4f;
    public float minOffTime = 0.05f;
    public float maxOffTime = 2.5f;

    [Header("Flicker (rápidos parpadeos)")]
    [Range(0f, 1f)] public float flickerProbability = 0.35f; // probabilidad de que ocurra un flicker antes de apagarse
    public int flickerMinPulses = 2;
    public int flickerMaxPulses = 6;
    public float flickerPulseMinDuration = 0.03f;
    public float flickerPulseMaxDuration = 0.12f;

    [Header("Burst aleatorio (secuencia rápida)")]
    [Range(0f, 1f)] public float burstChance = 0.07f; // ocasionalmente una luz hará una ráfaga más larga/caótica
    public int burstExtraPulses = 6;

    [Header("Fundidos (si true usa cambios suaves de intensidad)")]
    public bool useSmoothFade = true;
    public float minFadeDuration = 0.05f;
    public float maxFadeDuration = 0.25f;

    // Internals
    List<LightData> _lightDatas = new List<LightData>();
    int _currentlyOff = 0;
    object _lock = new object();

    void Start()
    {
        if (minLightsOn < 0) minLightsOn = 0;
        if (minLightsOn > lights.Count) minLightsOn = Mathf.Max(0, lights.Count - 1);

        // Construir wrapper por cada GameObject
        foreach (var go in lights)
        {
            if (go == null) continue;
            var ld = new LightData(go);
            if (!ld.HasAnyLight)
            {
                Debug.LogWarning($"HorrorLightFlicker: GameObject '{go.name}' no tiene Light (3D) ni Light2D detectada. Ignorado.");
                continue;
            }
            _lightDatas.Add(ld);
        }

        // Ajustar minLightsOn si hay menos luces válidas
        if (minLightsOn > _lightDatas.Count) minLightsOn = Mathf.Max(0, _lightDatas.Count - 1);

        // Iniciar corutinas
        for (int i = 0; i < _lightDatas.Count; i++)
        {
            var index = i;
            StartCoroutine(LightLoop(_lightDatas[index]));
        }
    }

    IEnumerator LightLoop(LightData ld)
    {
        // Asegurar estado inicial: ON
        ld.SetEnabled(true, immediate: true);

        yield return new WaitForSeconds(Random.Range(0f, 0.5f)); // ligero offset para variedad
        while (true)
        {
            // Tiempo encendida
            float onT = Random.Range(minOnTime, maxOnTime);
            yield return new WaitForSeconds(onT);

            // antes de apagar: puede hacer flicker
            if (Random.value < flickerProbability)
            {
                int pulses = Random.Range(flickerMinPulses, flickerMaxPulses + 1);
                if (Random.value < burstChance) pulses += burstExtraPulses;

                for (int p = 0; p < pulses; p++)
                {
                    // alterna on/off rápido
                    ld.ToggleImmediate(); // cambio rápido
                    float dur = Random.Range(flickerPulseMinDuration, flickerPulseMaxDuration);
                    yield return new WaitForSeconds(dur);
                }
            }

            // Intentar apagar (respetando minLightsOn)
            bool turnedOff = TryTurnOff(ld);
            if (turnedOff)
            {
                float offT = Random.Range(minOffTime, maxOffTime);
                yield return new WaitForSeconds(offT);
                // volver a encender
                ld.SetEnabled(true, immediate: false, fadeDuration: Random.Range(minFadeDuration, maxFadeDuration), useFade: useSmoothFade);
                lock (_lock) { _currentlyOff = Mathf.Max(0, _currentlyOff - 1); }
            }
            else
            {
                // No pudo apagarse porque se violaría minLightsOn -> esperar un corto tiempo antes de reintentar
                float retry = Random.Range(0.1f, 0.6f);
                yield return new WaitForSeconds(retry);
            }
        }
    }

    bool TryTurnOff(LightData ld)
    {
        lock (_lock)
        {
            int total = _lightDatas.Count;
            int allowedOffMax = Mathf.Max(0, total - minLightsOn);
            if (_currentlyOff >= allowedOffMax)
            {
                // no se permite más apagadas
                return false;
            }
            // podemos apagar
            _currentlyOff++;
        }

        // apagar (con fade opcional)
        ld.SetEnabled(false, immediate: false, fadeDuration: Random.Range(minFadeDuration, maxFadeDuration), useFade: useSmoothFade);
        return true;
    }

    // Para debugging o control runtime
    [ContextMenu("Apagar todas (forzado)")]
    public void ForceAllOff()
    {
        foreach (var ld in _lightDatas) ld.SetEnabled(false, immediate: true);
    }

    [ContextMenu("Encender todas (forzado)")]
    public void ForceAllOn()
    {
        lock (_lock) { _currentlyOff = 0; }
        foreach (var ld in _lightDatas) ld.SetEnabled(true, immediate: true);
    }

    class LightData
    {
        public GameObject go;
        public Light unityLight; // componente Light normal
        public Component light2D; // componente Light2D (si lo hay) - accedido por reflexión
        public float initialIntensity = 1f;
        public bool HasAnyLight => unityLight != null || light2D != null;

        System.Type light2DType;
        PropertyInfo intensityProperty; // para Light2D

        public LightData(GameObject go)
        {
            this.go = go;
            unityLight = go.GetComponent<Light>();
            if (unityLight != null)
            {
                initialIntensity = unityLight.intensity;
            }
            else
            {
                // intentar detectar Light2D por nombre (compatibilidad con distintas versiones URP)
                light2D = GetLight2DComponent(go);
                if (light2D != null)
                {
                    light2DType = light2D.GetType();
                    intensityProperty = light2DType.GetProperty("intensity");
                    if (intensityProperty != null)
                    {
                        object val = intensityProperty.GetValue(light2D, null);
                        if (val is float f) initialIntensity = f;
                    }
                }
            }
        }

        Component GetLight2DComponent(GameObject g)
        {
            // buscar cualquier componente cuyo tipo se llame "Light2D"
            var comps = g.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                var t = c.GetType();
                if (t.Name == "Light2D") return c;
            }
            return null;
        }

        public void SetEnabled(bool on, bool immediate = false, float fadeDuration = 0.15f, bool useFade = true)
        {
            if (unityLight != null)
            {
                if (useFade && !immediate)
                    go.GetComponentInParent<MonoBehaviour>()?.StartCoroutine(FadeLightUnity(unityLight, on, fadeDuration));
                else
                    unityLight.enabled = on;
            }
            else if (light2D != null)
            {
                if (intensityProperty != null)
                {
                    if (useFade && !immediate)
                        go.GetComponentInParent<MonoBehaviour>()?.StartCoroutine(FadeLight2D(light2D, on, fadeDuration, intensityProperty, initialIntensity));
                    else
                    {
                        intensityProperty.SetValue(light2D, on ? initialIntensity : 0f, null);
                    }
                }
            }
        }

        public void ToggleImmediate()
        {
            if (unityLight != null)
            {
                unityLight.enabled = !unityLight.enabled;
            }
            else if (light2D != null && intensityProperty != null)
            {
                float cur = (float)intensityProperty.GetValue(light2D, null);
                intensityProperty.SetValue(light2D, cur > 0.001f ? 0f : initialIntensity, null);
            }
        }

        IEnumerator FadeLightUnity(Light l, bool turnOn, float duration)
        {
            if (duration <= 0f)
            {
                l.enabled = turnOn;
                yield break;
            }

            float start = l.enabled ? l.intensity : 0f;
            float target = turnOn ? initialIntensity : 0f;

            // if currently disabled but turning on, enable to let intensity change
            if (!l.enabled && turnOn) l.enabled = true;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                l.intensity = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
            l.intensity = target;

            if (!turnOn)
            {
                // fully off -> actually disable component to save cost (optional)
                l.enabled = false;
                l.intensity = start; // restore intensity to original for when re-enabled (we store initialIntensity separately)
                // set actual intensity to 0 by disabling - alternative: keep intensity = initialIntensity but toggled off
            }
        }

        IEnumerator FadeLight2D(Component c, bool turnOn, float duration, PropertyInfo intensityProp, float initIntensity)
        {
            if (duration <= 0f)
            {
                intensityProp.SetValue(c, turnOn ? initIntensity : 0f, null);
                yield break;
            }

            float cur = (float)intensityProp.GetValue(c, null);
            float start = cur;
            float target = turnOn ? initIntensity : 0f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float v = Mathf.Lerp(start, target, t / duration);
                intensityProp.SetValue(c, v, null);
                yield return null;
            }
            intensityProp.SetValue(c, target, null);
        }
    }
}
