using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class EarthquakeController : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Transform cameraTransform;   // arrastra la cámara del jugador
    [SerializeField] private UIManager uiManager;         // opcional (para mensajes)

    [Header("Auto Start")]
    [Tooltip("Si está activo, el sismo comienza al iniciar la escena.")]
    [SerializeField] private bool autoStart = true;
    [Tooltip("Duración del sismo al auto iniciar. -1 = infinito hasta StopEarthquake()")]
    [SerializeField] private float autoDuration = 20f;

    [Header("Shake (posición)")]
    [SerializeField] private float posAmplitude = 0.06f;     // metros
    [SerializeField] private float posFrequency = 18f;       // Hz aprox (ruido Perlin sampleado por tiempo)

    [Header("Shake (rotación extra)")]
    [SerializeField] private float rotAmplitudeDeg = 1.5f;   // grados (roll/pitch pequeños)
    [SerializeField] private float rotFrequency = 9f;

    [Header("Intensidad en el tiempo")]
    [Tooltip("Escala global de intensidad (0..1).")]
    [Range(0f, 1f)] public float intensity = 1f;
    [Tooltip("Curva 0..1 a lo largo de la duración; X = progreso (0..1), Y = factor (0..1).")]
    [SerializeField]
    private AnimationCurve envelope =
        AnimationCurve.EaseInOut(0, 0, 0.15f, 1f); // sube rápido
    [SerializeField]
    private AnimationCurve envelopeTail =
        AnimationCurve.EaseInOut(0.85f, 1f, 1f, 0f); // cae al final

    [Header("FOV Pulse (opcional)")]
    [SerializeField] private bool pulseFOV = true;
    [SerializeField] private float fovPulseAmount = 3f;      // +/-
    [SerializeField] private float fovPulseFrequency = 0.9f;
    private float baseFOV = -1f;
    private Camera cam;

    [Header("Aftershocks")]
    [Tooltip("Permite pequeños remezones aleatorios durante el sismo.")]
    [SerializeField] private bool aftershocks = true;
    [SerializeField] private Vector2 aftershockEverySeconds = new Vector2(6f, 12f);
    [SerializeField] private float aftershockAmpMultiplier = 1.6f;
    [SerializeField] private float aftershockDuration = 0.8f;

    [Header("Rigidbodies opcionales")]
    [SerializeField] private List<Rigidbody> shakeBodies = new List<Rigidbody>();
    [SerializeField] private float bodyImpulse = 0.8f;

    [Header("Mensajes (opcionales)")]
    [SerializeField, TextArea(1, 3)] private string startMsg = "¡Sismo! Resguárdate.";
    [SerializeField, TextArea(1, 3)] private string tipMsg = "Agáchate bajo una mesa o en el marco de una puerta.";

    // --- estado ---
    private bool running;
    private float t0, dur;
    private float seedX, seedY, seedZ, seedR;
    private Vector3 camLocalPos0;
    private Quaternion camLocalRot0;
    private float aftershockUntil;
    private float aftershockFactor; // 1 = normal; >1 = más fuerte

    void Awake()
    {
        if (cameraTransform == null)
        {
            var camFound = GetComponentInChildren<Camera>();
            if (camFound != null) cameraTransform = camFound.transform;
        }
        cam = cameraTransform ? cameraTransform.GetComponent<Camera>() : null;
        if (cam != null) baseFOV = cam.fieldOfView;


        // semillas para Perlin
        seedX = Random.Range(0f, 1000f);
        seedY = Random.Range(0f, 1000f);
        seedZ = Random.Range(0f, 1000f);
        seedR = Random.Range(0f, 1000f);
    }

    void Start()
    {
        if (autoStart) StartEarthquake(autoDuration);
    }

    public void StartEarthquake(float duration = -1f)
    {
        if (cameraTransform == null)
        {
            Debug.LogError("[EarthquakeController] No hay cámara asignada.");
            return;
        }
        running = true;
        t0 = Time.time;
        dur = duration;
        camLocalPos0 = cameraTransform.localPosition;
        camLocalRot0 = cameraTransform.localRotation;
        aftershockUntil = 0f;
        aftershockFactor = 1f;

        if (uiManager != null)
        {
            if (!string.IsNullOrEmpty(startMsg)) uiManager.OnMessageEventRaised(startMsg);        // mensaje info
            if (!string.IsNullOrEmpty(tipMsg)) uiManager.UpdateObjectiveText(tipMsg);          // objetivo/guía
        }
        if (aftershocks) StartCoroutine(AftershockLoop());
    }

    public void StopEarthquake()
    {
        running = false;
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = camLocalPos0;
            cameraTransform.localRotation = camLocalRot0;
        }
        if (cam != null && baseFOV > 0f) cam.fieldOfView = baseFOV;
        StopAllCoroutines();
    }

    public void TriggerAftershock(float duration, float ampMultiplier = 2f)
    {
        aftershockUntil = Time.time + Mathf.Max(0.05f, duration);
        aftershockFactor = Mathf.Max(1f, ampMultiplier);
    }

    private IEnumerator AftershockLoop()
    {
        while (running)
        {
            yield return new WaitForSeconds(Random.Range(aftershockEverySeconds.x, aftershockEverySeconds.y));
            TriggerAftershock(aftershockDuration, aftershockAmpMultiplier);
            // Empujoncito a rigidbodies
            if (shakeBodies != null && shakeBodies.Count > 0)
            {
                foreach (var rb in shakeBodies)
                {
                    if (rb == null) continue;
                    Vector3 dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
                    rb.AddForce(dir * bodyImpulse, ForceMode.Impulse);
                }
            }
        }
    }

    void LateUpdate()
    {
        if (!running || cameraTransform == null) return;

        float t = Time.time - t0;
        float norm = (dur > 0f) ? Mathf.Clamp01(t / dur) : Mathf.Repeat(t * 0.1f, 1f);
        float env = intensity;
        if (dur > 0f)
        {
            // sobre (0..1) combinamos dos curvas para subir y bajar
            env *= (norm < 0.5f)
                ? envelope.Evaluate(Mathf.InverseLerp(0f, 0.5f, norm))
                : envelopeTail.Evaluate(Mathf.InverseLerp(0.5f, 1f, norm));
        }

        // aftershock
        float shock = (Time.time < aftershockUntil) ? aftershockFactor : 1f;
        float i = env * shock;

        // --- POSICIÓN (ruido perlin) ---
        Vector3 offset = Vector3.zero;
        offset.x = (Mathf.PerlinNoise(seedX + Time.time * posFrequency, 0f) - 0.5f) * 2f * posAmplitude * i;
        offset.y = (Mathf.PerlinNoise(seedY + Time.time * posFrequency, 1f) - 0.5f) * 2f * posAmplitude * i;
        offset.z = (Mathf.PerlinNoise(seedZ + Time.time * posFrequency, 2f) - 0.5f) * 2f * posAmplitude * i;

        // --- ROTACIÓN extra (pequeña) ---
        float roll = (Mathf.PerlinNoise(seedR + Time.time * rotFrequency, 3f) - 0.5f) * 2f * rotAmplitudeDeg * i;
        float pitch = (Mathf.PerlinNoise(seedR + 13f + Time.time * rotFrequency, 4f) - 0.5f) * 2f * (rotAmplitudeDeg * 0.5f) * i;

        // Componer con lo que ya puso el FPC/otros (nos basamos en lo actual cada frame)
        var baseRot = cameraTransform.localRotation;
        var deltaRot = Quaternion.Euler(pitch, 0f, roll);
        cameraTransform.localPosition = camLocalPos0 + offset;
        cameraTransform.localRotation = baseRot * deltaRot;

        // FOV pulse
        if (pulseFOV && cam != null && baseFOV > 0f)
        {
            cam.fieldOfView = baseFOV + Mathf.Sin(Time.time * fovPulseFrequency * Mathf.PI * 2f) * fovPulseAmount * i;
        }

        // detener al terminar (si tiene duración)
        if (dur > 0f && t >= dur)
        {
            StopEarthquake();
        }
    }
}
