using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Audio;
using UnityEngine.UI; // <-- Slider

public class ValveMinigame : MonoBehaviour
{
    [Header("Referencias de Escena")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera minigameCamera;
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private Transform valveTransform;
    [SerializeField] private GameObject ValveHelp;

    [Header("Configuración del Minijuego")]
    [SerializeField] private float rotationSensitivity = 20f;
    [SerializeField] private float requiredRotation = 360f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Header("UI Progreso")]
    [Tooltip("Slider que refleja el progreso (0..1). Si 'allowSliderControl' está activo, también controla la válvula.")]
    [SerializeField] private Slider progressSlider;
    [Tooltip("Permite arrastrar el slider para mover la válvula.")]
    [SerializeField] private bool allowSliderControl = true;

    [Header("Audio de la Válvula (Scrub)")]
    [SerializeField] private AudioClip valveScrubClip;
    [SerializeField, Range(0f, 1f)] private float valveScrubVolume = 0.9f;
    [SerializeField] private AudioMixerGroup outputMixerGroup; // opcional: asigna SFX
    [SerializeField] private bool spatializeAtValve = true;
    [SerializeField] private float endGuardSeconds = 0.03f;
    [SerializeField] private Vector2 pitchBySpeed = new Vector2(0.9f, 1.25f);
    [SerializeField] private float fastDegPerSec = 180f;

    [Header("Eventos")]
    public UnityEvent OnMinigameCompleted;

    private bool isMinigameActive = false;
    private float netRotation = 0f;    // 0..requiredRotation
    private AudioSource valveAudio;

    // Anti-bucle para el slider (cuando actualizamos por código)
    private bool _updatingSlider = false;

    void Start()
    {
        if (minigameCamera != null) minigameCamera.gameObject.SetActive(false);
        CreateValveAudioSource();

        // Config básica del slider si está asignado
        if (progressSlider)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.wholeNumbers = false;
            progressSlider.value = 0f;
            progressSlider.interactable = allowSliderControl;

            // Suscribir cambios (cuando el usuario arrastra)
            progressSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    void OnDestroy()
    {
        if (progressSlider)
            progressSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    void CreateValveAudioSource()
    {
        Transform host = (spatializeAtValve && valveTransform) ? valveTransform : this.transform;

        valveAudio = host.gameObject.AddComponent<AudioSource>();
        valveAudio.playOnAwake = false;
        valveAudio.loop = false;
        valveAudio.clip = valveScrubClip;
        valveAudio.volume = valveScrubVolume;
        valveAudio.spatialBlend = spatializeAtValve ? 1f : 0f;
        if (outputMixerGroup) valveAudio.outputAudioMixerGroup = outputMixerGroup;
    }

    public void StartMinigame()
    {
        if (isMinigameActive) return;

        isMinigameActive = true;
        netRotation = 0f;
        ValveHelp.SetActive(true);
        if (valveTransform) valveTransform.localRotation = Quaternion.identity;
        PrepareAudioAtProgress(0f, playNow: false);

        if (mainCamera) mainCamera.gameObject.SetActive(false);
        if (minigameCamera) minigameCamera.gameObject.SetActive(true);
        if (playerController) playerController.SetInputEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateSliderFromProgress();
    }

    void Update()
    {
        if (!isMinigameActive) return;

        // Arrastre con mouse (clic izquierdo sostenido)
        if (Input.GetMouseButton(0))
        {
            float mouseDeltaX = Input.GetAxis("Mouse X");
            float rotationAmount = mouseDeltaX * rotationSensitivity;
            float target = Mathf.Clamp(netRotation + rotationAmount, 0f, requiredRotation);

            ApplyTargetRotation(target);
        }

        // ¿completado?
        if (netRotation >= requiredRotation - 0.1f)
        {
            CompleteMinigame();
        }
    }

    // ========= Slider: two-way binding ==================

    private void OnSliderValueChanged(float value01)
    {
        if (_updatingSlider) return;           // cambio disparado por código, ignorar
        if (!isMinigameActive) return;         // solo cuando el minijuego está activo
        if (!allowSliderControl) return;       // opcional, por si solo quieres reflejar

        float target = Mathf.Clamp01(value01) * requiredRotation;
        ApplyTargetRotation(target);
    }

    private void UpdateSliderFromProgress()
    {
        if (!progressSlider) return;
        _updatingSlider = true;
        progressSlider.value = (requiredRotation > 0f) ? netRotation / requiredRotation : 0f;
        _updatingSlider = false;
    }

    // ========= Núcleo de rotación + audio + UI ==========

    /// <summary>
    /// Lleva la válvula a 'targetDegrees' (0..requiredRotation), rotando solo el delta necesario.
    /// Actualiza audio (scrub) y slider.
    /// </summary>
    private void ApplyTargetRotation(float targetDegrees)
    {
        float clamped = Mathf.Clamp(targetDegrees, 0f, requiredRotation);
        float delta = clamped - netRotation;
        if (Mathf.Abs(delta) < 0.0001f) return;

        // Rotación visual
        if (valveTransform)
            valveTransform.Rotate(rotationAxis, delta, Space.Self);

        // Estado
        netRotation = clamped;

        // Audio scrubbing + pitch por velocidad
        UpdateValveScrubAudio(delta);

        // UI
        UpdateSliderFromProgress();
    }

    private void UpdateValveScrubAudio(float deltaDegrees)
    {
        if (!valveAudio || !valveScrubClip) return;

        bool movingThisFrame = Mathf.Abs(deltaDegrees) > 0.0001f;

        float progress01 = (requiredRotation > 0f)
            ? Mathf.Clamp01(netRotation / requiredRotation)
            : 0f;

        float maxTime = Mathf.Max(0f, valveScrubClip.length - endGuardSeconds);
        float targetTime = Mathf.Clamp(progress01 * maxTime, 0f, maxTime);

        if (movingThisFrame)
        {
            float speedDegPerSec = Mathf.Abs(deltaDegrees) / Mathf.Max(Time.deltaTime, 1e-6f);
            float t = Mathf.Clamp01(speedDegPerSec / Mathf.Max(1e-3f, fastDegPerSec));
            valveAudio.pitch = Mathf.Lerp(pitchBySpeed.x, pitchBySpeed.y, t);

            if (!valveAudio.isPlaying)
            {
                valveAudio.time = targetTime;
                valveAudio.Play();
            }
            else
            {
                valveAudio.time = targetTime;
            }
        }
        else
        {
            if (valveAudio.isPlaying)
                valveAudio.Pause();
        }
    }

    private void PrepareAudioAtProgress(float progress01, bool playNow)
    {
        if (!valveAudio || !valveScrubClip) return;

        float maxTime = Mathf.Max(0f, valveScrubClip.length - endGuardSeconds);
        valveAudio.time = Mathf.Clamp01(progress01) * maxTime;

        if (playNow)
        {
            if (!valveAudio.isPlaying) valveAudio.Play();
        }
        else
        {
            if (valveAudio.isPlaying) valveAudio.Pause();
        }
    }

    private void CompleteMinigame()
    {
        isMinigameActive = false;

        // audio al final y detener
        PrepareAudioAtProgress(1f, playNow: false);
        if (valveAudio) valveAudio.Stop();

        // UI al 100%
        UpdateSliderFromProgress();

        OnMinigameCompleted?.Invoke();

        if (minigameCamera) minigameCamera.gameObject.SetActive(false);
        if (mainCamera) mainCamera.gameObject.SetActive(true);
        if (playerController) playerController.SetInputEnabled(true);
        ValveHelp.SetActive(false);
        this.enabled = false;
    }

    private void OnDisable()
    {
        // Si se desactiva el objeto/escena, dejamos el audio pausado en su punto
        if (valveAudio && valveAudio.isPlaying)
            valveAudio.Pause();
    }
}

