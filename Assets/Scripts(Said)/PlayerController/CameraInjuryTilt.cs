using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class CameraInjuryTilt : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Transform cameraTransform;            // arrastra tu cámara (FirstPersonController.cameraTransform)
    [SerializeField] private FirstPersonController player;         // optional: auto-find

    [Header("Tilt Settings")]
    [Tooltip("Ángulo máximo de roll (grados).")]
    [SerializeField] private float maxRoll = 6f;
    [Tooltip("Frecuencia base de la oscilación.")]
    [SerializeField] private float frequency = 2f;
    [Tooltip("Suavizado del roll.")]
    [SerializeField] private float smoothTime = 0.12f;

    [Header("Intensidad por movimiento")]
    [Tooltip("Velocidad horizontal a la que se alcanza el roll máximo.")]
    [SerializeField] private float speedForMax = 6f;
    [Tooltip("Multiplicador del efecto si el jugador corre.")]
    [SerializeField] private float runMultiplier = 1.3f;

    [Header("Espasmos aleatorios (pinchazos)")]
    [SerializeField] private bool randomTwinges = true;
    [SerializeField] private Vector2 twingeEverySeconds = new Vector2(3f, 6f);
    [SerializeField] private float twingeExtraRoll = 4f;
    [SerializeField] private float twingeDuration = 0.25f;

    private Rigidbody rb;
    private float phase, targetRoll, currentRoll, rollVel;
    private bool active;
    private Coroutine twingeCo;

    void Awake()
    {
        if (player == null) player = GetComponentInParent<FirstPersonController>();
        if (cameraTransform == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam) cameraTransform = cam.transform;
        }
        rb = GetComponentInParent<Rigidbody>();
    }

    public void EnableInjuryTilt(bool value)
    {
        if (active == value) return;
        active = value;
        if (active && randomTwinges && twingeCo == null) twingeCo = StartCoroutine(Twinges());
        if (!active && twingeCo != null) { StopCoroutine(twingeCo); twingeCo = null; }
    }

    void LateUpdate()
    {
        if (!active || cameraTransform == null) return;

        // Velocidad horizontal
        float speed = 0f;
        if (rb != null)
        {
            var v = rb.linearVelocity;
            speed = new Vector2(v.x, v.z).magnitude;
        }

        // Escalado por estado
        float moveFactor = Mathf.InverseLerp(0f, speedForMax, speed);
        if (player != null)
        {
            if (player.IsCrouching) moveFactor *= 0.5f;
            if (player.IsRunning) moveFactor *= runMultiplier;
        }

        phase += Time.deltaTime * frequency * Mathf.Max(0.5f, moveFactor);
        float baseRoll = Mathf.Sin(phase) * maxRoll * moveFactor;
        targetRoll = baseRoll;

        // Suavizado
        currentRoll = Mathf.SmoothDamp(currentRoll, targetRoll, ref rollVel, smoothTime);

        // Componer con el pitch que tu FPC ya puso en Update
        var e = cameraTransform.localEulerAngles;
        float pitch = e.x;
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, currentRoll);
    }

    private IEnumerator Twinges()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(twingeEverySeconds.x, twingeEverySeconds.y));
            float t = 0f;
            while (t < twingeDuration)
            {
                t += Time.deltaTime;
                targetRoll += Mathf.Sign(Random.value - 0.5f) * twingeExtraRoll * (1f - t / twingeDuration);
                yield return null;
            }
        }
    }
}
