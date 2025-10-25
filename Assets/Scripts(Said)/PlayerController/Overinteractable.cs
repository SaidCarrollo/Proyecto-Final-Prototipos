using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Audio; // opcional si luego quieres mixer

public class OvenInteractable : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] private GameManager gameManager;

    [Header("Managers & Events")]
    [SerializeField] private BadgeManager badgeManager;
    [SerializeField] private VignetteEvent vignetteEvent;

    [Header("Default Interaction (Antes del descontrol)")]
    [SerializeField] private string defaultBadgeID = "PrevencionHornilla";

    [Header("Late Interaction (Después del descontrol)")]
    [SerializeField] private string lateBadgeID = "HornillaTarde";

    [Header("Events on Late Interaction")]
    [SerializeField] private GameEvent onPlayerDeathEvent;

    [SerializeField, TextArea(2, 5)] private string successMessage = "¡Acción preventiva correcta!";
    [SerializeField, TextArea(2, 5)] private string failureMessage = "¡Demasiado tarde! El fuego ya es incontrolable.";
    [SerializeField] private GameEventstring messageEvent; // tu tipo personalizado

    [Header("Audio (Fade out al interactuar)")]
    [Tooltip("AudioSource a atenuar (por ej., loop del gas/alarma).")]
    [SerializeField] private AudioSource loopToFade;
    [SerializeField, Min(0.05f)] private float fadeOutSeconds = 1.5f;
    [SerializeField] private bool stopAndResetAfterFade = true;
    [SerializeField] private bool useUnscaledTime = true; // <- IMPORTANTE

    [Header("Diagnóstico")]
    [Tooltip("Imprime información de audio y busca otras fuentes reproduciendo el mismo clip.")]
    [SerializeField] private bool verboseDiagnostics = true;
    [Tooltip("También escanea toda la escena para encontrar otros AudioSource reproduciendo el mismo clip.")]
    [SerializeField] private bool scanSceneForSameClip = true;

    private bool hasBeenUsed = false;
    private Coroutine fadeRoutine;
    private float initialLoopVolume = 1f;

    private void Awake()
    {
        if (loopToFade != null)
            initialLoopVolume = loopToFade.volume;
    }

    public void Interact()
    {
        if (hasBeenUsed)
        {
            Debug.Log("[OvenInteractable] Interact ignorado: hasBeenUsed = true");
            return;
        }

        if (gameManager == null || badgeManager == null || vignetteEvent == null)
        {
            Debug.LogError("[OvenInteractable] Faltan referencias: GameManager/BadgeManager/VignetteEvent.");
            return;
        }

        // --- LÓGICA DE JUEGO ---
        if (gameManager.IsFireUncontrolled)
        {
            badgeManager.UnlockBadge(lateBadgeID);
            vignetteEvent.Raise(Color.red, 0.5f, 3f);

            if (messageEvent != null && !string.IsNullOrEmpty(failureMessage))
                messageEvent.Raise(failureMessage);

            Debug.Log("[OvenInteractable] Interacción tardía: badge=" + lateBadgeID);
        }
        else
        {
            badgeManager.UnlockBadge(defaultBadgeID);
            vignetteEvent.Raise(Color.green, 0.4f, 2f);

            if (messageEvent != null && !string.IsNullOrEmpty(successMessage))
                messageEvent.Raise(successMessage);

            Debug.Log("[OvenInteractable] Interacción a tiempo: badge=" + defaultBadgeID);
        }

        // --- DIAGNÓSTICO PREVIO ---
        if (verboseDiagnostics)
            LogAudioContext("ANTES DEL FADE");

        if (scanSceneForSameClip)
            ScanSceneForSameClip(loopToFade);

        // --- FADE ---
        TryFadeOutLoop();

        hasBeenUsed = true;
    }

    private void TryFadeOutLoop()
    {
        if (loopToFade == null)
        {
            Debug.LogWarning("[OvenInteractable] loopToFade = null. No hay AudioSource asignado.");
            return;
        }

        // Aunque no esté isPlaying, podemos tener volumen > 0 (por ejemplo si otro script lo arranca).
        if (!loopToFade.isPlaying && loopToFade.volume <= 0.0001f)
        {
            Debug.Log("[OvenInteractable] El AudioSource no está reproduciendo y ya está a volumen ~0. Nada que atenuar.");
            return;
        }

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOutCR(loopToFade, fadeOutSeconds, initialLoopVolume, stopAndResetAfterFade, useUnscaledTime));
    }

    private IEnumerator FadeOutCR(AudioSource src, float seconds, float initialVolume, bool stopAndReset, bool unscaled)
    {
        if (src == null) yield break;

        if (verboseDiagnostics)
        {
            Debug.Log($"[OvenInteractable] Iniciando fade de '{(src.clip ? src.clip.name : "(sin clip)")}' " +
                      $"duración={seconds:0.00}s, unscaled={unscaled}, timeScale={Time.timeScale:0.###}, " +
                      $"volInicial={src.volume:0.###}, loop={src.loop}, spatialBlend={src.spatialBlend:0.##}");
        }

        if (seconds <= 0f)
        {
            src.volume = 0f;
            if (stopAndReset)
            {
                src.Stop();
                src.volume = initialVolume;
            }
            yield break;
        }

        float startVol = src.volume;
        float t = 0f;
        float logEvery = 0.5f; // log cada 0.5s para no spamear
        float nextLog = logEvery;

        // Nota: si el juego está pausado y unscaled=false, esto NO avanzará.
        if (!unscaled && Mathf.Approximately(Time.timeScale, 0f))
        {
            Debug.LogWarning("[OvenInteractable] timeScale=0 y useUnscaledTime=false → El fade no avanzará. Activa useUnscaledTime.");
        }

        while (t < seconds && src != null)
        {
            t += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(t / seconds);
            k = k * k * (3f - 2f * k); // SmoothStep
            src.volume = Mathf.Lerp(startVol, 0f, k);

            if (verboseDiagnostics && t >= nextLog)
            {
                Debug.Log($"[OvenInteractable] Fading... t={t:0.00}/{seconds:0.00}, vol={src.volume:0.###}");
                nextLog += logEvery;
            }

            yield return null;
        }

        if (src != null)
        {
            src.volume = 0f;

            if (stopAndReset)
            {
                src.Stop();
                src.volume = initialVolume;
            }
        }

        if (verboseDiagnostics)
            LogAudioContext("DESPUÉS DEL FADE");

        // Plan B: si aún se oye algo (porque otro script lo re-activa), asegurar silencio.
        // (No debería ser necesario, pero ayuda si hay un SoundManager que pisa el volumen.)
        yield return null; // esperar 1 frame por si otro script ejecuta
        if (src != null && src.isPlaying && src.volume > 0.001f)
        {
            Debug.LogWarning("[OvenInteractable] Tras el fade, el AudioSource volvió a sonar o recuperó volumen. Forzando mute+Stop.");
            src.mute = true;
            src.Stop();
            src.mute = false; // lo quitamos para usos futuros
            src.volume = initialVolume;
        }
    }

    private void LogAudioContext(string tag)
    {
        var sb = new StringBuilder();
        sb.Append($"[OvenInteractable] {tag} | timeScale={Time.timeScale:0.###}, unscaled={useUnscaledTime}");

        if (loopToFade == null)
        {
            sb.Append(" | loopToFade=null");
            Debug.Log(sb.ToString());
            return;
        }

        sb.Append($" | AudioSource='{loopToFade.name}'");
        sb.Append($" | clip='{(loopToFade.clip ? loopToFade.clip.name : "(sin clip)")}'");
        sb.Append($" | isPlaying={loopToFade.isPlaying}");
        sb.Append($" | vol={loopToFade.volume:0.###}/{initialLoopVolume:0.###}");
        sb.Append($" | loop={loopToFade.loop}");
        sb.Append($" | mixerGroup='{(loopToFade.outputAudioMixerGroup ? loopToFade.outputAudioMixerGroup.name : "(none)")}'");
        sb.Append($" | spatialBlend={loopToFade.spatialBlend:0.##}");
        Debug.Log(sb.ToString());
    }

    private void ScanSceneForSameClip(AudioSource reference)
    {
        if (reference == null || reference.clip == null) return;

        var all = FindObjectsOfType<AudioSource>(includeInactive: true);
        int countPlaying = 0;
        foreach (var s in all)
        {
            if (s != null && s.isPlaying && s.clip == reference.clip)
            {
                countPlaying++;
                Debug.Log($"[OvenInteractable][SCAN] Otro AudioSource está reproduciendo el MISMO clip: " +
                          $"obj='{s.name}', vol={s.volume:0.###}, loop={s.loop}, " +
                          $"mixerGroup='{(s.outputAudioMixerGroup ? s.outputAudioMixerGroup.name : "(none)")}', " +
                          $"pos={s.transform.position}");
            }
        }

        if (countPlaying == 0)
            Debug.Log("[OvenInteractable][SCAN] No se hallaron otros AudioSource reproduciendo el mismo clip.");
    }
}
