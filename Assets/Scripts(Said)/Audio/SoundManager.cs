using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources (asigna en Inspector)")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopingSfxSource;

    [Tooltip("Opcional: si no las asignas se usarán sfxSource")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioSource uiSource;

    [Header("AudioMixer Groups (asigna del mismo Mixer que usa tu UI)")]
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup ambienceGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup voiceGroup;
    [SerializeField] private AudioMixerGroup uiGroup;
    // Guarda última posición por nombre de clip (segundos)
    private readonly Dictionary<string, float> _savedMusicPositions = new();
    private readonly Dictionary<string, float> _savedAmbiencePositions = new();

    // Recuerda el "target" de volumen al hacer fade in nuevamente
    private float _lastMusicTargetVolume = 1f;
    private float _lastAmbienceTargetVolume = 1f;

    [Header("Librería de Sonidos")]
    public List<Sound> sounds;

    private readonly Dictionary<SoundChannel, AudioSource> _oneShotSourceByChannel = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Asegura rutas al Mixer correcto
        if (musicSource && musicGroup) musicSource.outputAudioMixerGroup = musicGroup;
        if (ambienceSource && ambienceGroup) ambienceSource.outputAudioMixerGroup = ambienceGroup;
        if (sfxSource && sfxGroup) sfxSource.outputAudioMixerGroup = sfxGroup;
        if (loopingSfxSource && sfxGroup) loopingSfxSource.outputAudioMixerGroup = sfxGroup;

        // Fallbacks
        if (!voiceSource) voiceSource = sfxSource;
        if (!uiSource) uiSource = sfxSource;

        if (voiceSource && voiceGroup) voiceSource.outputAudioMixerGroup = voiceGroup;
        if (uiSource && uiGroup) uiSource.outputAudioMixerGroup = uiGroup;

        // Mapa para one-shots por canal
        _oneShotSourceByChannel[SoundChannel.SFX] = sfxSource;
        _oneShotSourceByChannel[SoundChannel.UI] = uiSource;
        _oneShotSourceByChannel[SoundChannel.Voice] = voiceSource;
        _oneShotSourceByChannel[SoundChannel.Ambience] = ambienceSource;
        _oneShotSourceByChannel[SoundChannel.Music] = musicSource;
        _oneShotSourceByChannel[SoundChannel.LoopingSFX] = loopingSfxSource;
    }

    // ------------------- Música -------------------
    public void PlayMusic(string name, float volume = 1.0f, bool loop = true, float fadeDuration = 1.5f, bool resumeFromSaved = false)
    {
        Sound s = sounds.FirstOrDefault(sound => sound.name == name);
        if (s == null || s.clip == null)
        {
            Debug.LogWarning($"Sonido musical: '{name}' no encontrado.");
            return;
        }

        if (musicSource.clip == s.clip && musicSource.isPlaying) return;

        musicSource.DOKill();

        // calcula el volumen objetivo y recuerda para reanudar
        _lastMusicTargetVolume = Mathf.Clamp01(s.volume * volume);

        float? startTimeSec = null;
        if (resumeFromSaved && _savedMusicPositions.TryGetValue(s.clip.name, out var t))
            startTimeSec = Mathf.Clamp(t, 0f, s.clip.length - 0.05f);

        if (musicSource.isPlaying)
        {
            musicSource.DOFade(0, fadeDuration * 0.5f).OnComplete(() =>
            {
                StartNewMusic(s, volume, loop, fadeDuration * 0.5f, startTimeSec);
            });
        }
        else
        {
            StartNewMusic(s, volume, loop, fadeDuration, startTimeSec);
        }
    }

    private void StartNewMusic(Sound s, float volume, bool loop, float fadeDuration, float? startTimeSec = null)
    {
        if (musicGroup) musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.clip = s.clip;
        musicSource.loop = loop || s.loop;
        if (startTimeSec.HasValue) musicSource.time = startTimeSec.Value;
        musicSource.volume = 0f;
        musicSource.Play();
        musicSource.DOFade(_lastMusicTargetVolume, fadeDuration);
    }


    // ------------------- Ambiente -------------------
    public void PlayAmbience(string name, float volume = 1.0f, bool loop = true, float fadeDuration = 1.5f, bool resumeFromSaved = false)
    {
        Sound s = sounds.FirstOrDefault(sound => sound.name == name);
        if (s == null || s.clip == null)
        {
            Debug.LogWarning($"Sonido de ambiente: '{name}' no encontrado.");
            return;
        }

        if (ambienceSource.clip == s.clip && ambienceSource.isPlaying) return;

        ambienceSource.DOKill();
        _lastAmbienceTargetVolume = Mathf.Clamp01(s.volume * volume);

        float? startTimeSec = null;
        if (resumeFromSaved && _savedAmbiencePositions.TryGetValue(s.clip.name, out var t))
            startTimeSec = Mathf.Clamp(t, 0f, s.clip.length - 0.05f);

        if (ambienceSource.isPlaying)
        {
            ambienceSource.DOFade(0, fadeDuration * 0.5f).OnComplete(() =>
            {
                StartNewAmbience(s, volume, loop, fadeDuration * 0.5f, startTimeSec);
            });
        }
        else
        {
            StartNewAmbience(s, volume, loop, fadeDuration, startTimeSec);
        }
    }

    private void StartNewAmbience(Sound s, float volume, bool loop, float fadeDuration, float? startTimeSec = null)
    {
        if (ambienceGroup) ambienceSource.outputAudioMixerGroup = ambienceGroup;
        ambienceSource.clip = s.clip;
        ambienceSource.loop = loop || s.loop;
        if (startTimeSec.HasValue) ambienceSource.time = startTimeSec.Value;
        ambienceSource.volume = 0f;
        ambienceSource.Play();
        ambienceSource.DOFade(_lastAmbienceTargetVolume, fadeDuration);
    }


    // ------------------- One-Shots / Efectos -------------------
    /// <summary>
    /// Reproduce el sonido por su canal definido en la librería (recomendado).
    /// </summary>
    public void Play(string name, float volumeScale = 1.0f)
    {
        Sound s = sounds.FirstOrDefault(sound => sound.name == name);
        if (s == null || s.clip == null)
        {
            Debug.LogWarning($"Sonido: '{name}' no encontrado.");
            return;
        }

        switch (s.channel)
        {
            case SoundChannel.Music:
                PlayMusic(name, volume: volumeScale, loop: s.loop);
                return;

            case SoundChannel.Ambience:
                if (s.loop) PlayAmbience(name, volume: volumeScale, loop: true);
                else PlayOneShotInternal(ambienceSource, ambienceGroup, s, volumeScale);
                return;

            case SoundChannel.LoopingSFX:
                PlayLoopingSFX(name, s.volume * volumeScale);
                return;

            case SoundChannel.Voice:
                PlayOneShotInternal(voiceSource, voiceGroup, s, volumeScale);
                return;

            case SoundChannel.UI:
                PlayOneShotInternal(uiSource, uiGroup, s, volumeScale);
                return;

            default: // SFX
                PlayOneShotInternal(sfxSource, sfxGroup, s, volumeScale);
                return;
        }
    }

    /// <summary>
    /// Compatible con tu código existente. Respeta el canal del Sound.
    /// </summary>
    public void PlaySFX(string name, float volume = 1.0f)
    {
        Play(name, volume);
    }

    public void PlayVoice(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null)
        {
            Debug.LogWarning("Se intentó reproducir un AudioClip nulo (Voz).");
            return;
        }
        var src = voiceSource ? voiceSource : sfxSource;
        src.PlayOneShot(clip, volume);
    }

    private void PlayOneShotInternal(AudioSource src, AudioMixerGroup group, Sound s, float volumeScale)
    {
        if (src == null)
        {
            Debug.LogWarning($"No hay AudioSource asignado para el canal {s.channel}. Se usará SFX.");
            src = sfxSource;
            group = sfxGroup;
        }
        if (group != null) src.outputAudioMixerGroup = group;
        src.PlayOneShot(s.clip, Mathf.Clamp01(s.volume * volumeScale));
    }

    // ------------------- Looping SFX -------------------
    public void PlayLoopingSFX(string name, float volume = 1.0f)
    {
        if (loopingSfxSource == null) return;

        Sound s = sounds.FirstOrDefault(sound => sound.name == name);
        if (s == null || s.clip == null)
        {
            Debug.LogWarning($"Looping SFX: '{name}' no encontrado.");
            return;
        }

        if (sfxGroup) loopingSfxSource.outputAudioMixerGroup = sfxGroup;
        loopingSfxSource.clip = s.clip;
        loopingSfxSource.loop = true;
        loopingSfxSource.volume = Mathf.Clamp01(s.volume * volume);
        loopingSfxSource.Play();
    }

    public void StopLoopingSFX()
    {
        if (loopingSfxSource == null) return;
        loopingSfxSource.Stop();
        loopingSfxSource.clip = null;
    }

    public void SetLoopingSFXVolume(float volume)
    {
        if (loopingSfxSource != null)
            loopingSfxSource.volume = Mathf.Clamp01(volume);
    }

    // ------------------- Stop helpers -------------------
    // --- Música ---
    public void StopMusic(float fadeDuration = 1.0f, bool rememberPosition = false)
    {
        if (!musicSource || !musicSource.clip) return;

        if (rememberPosition)
            _savedMusicPositions[musicSource.clip.name] = musicSource.time;

        musicSource.DOKill();
        if (musicSource.isPlaying)
            musicSource.DOFade(0, fadeDuration).OnComplete(() => musicSource.Stop());
        else
            musicSource.Stop();
    }

    public void PauseMusic(bool rememberPosition = true)
    {
        if (!musicSource || !musicSource.isPlaying) return;
        if (rememberPosition && musicSource.clip)
            _savedMusicPositions[musicSource.clip.name] = musicSource.time;
        musicSource.Pause();
    }

    public void ResumeMusic(float fadeDuration = 0.75f)
    {
        if (!musicSource || !musicSource.clip) return;

        if (_savedMusicPositions.TryGetValue(musicSource.clip.name, out var t))
            musicSource.time = Mathf.Clamp(t, 0f, musicSource.clip.length - 0.05f);

        musicSource.volume = 0f;
        musicSource.Play();
        musicSource.DOFade(_lastMusicTargetVolume, fadeDuration);
    }

    // --- Ambience ---
    public void StopAmbience(float fadeDuration = 1.0f, bool rememberPosition = false)
    {
        if (!ambienceSource || !ambienceSource.clip) return;

        if (rememberPosition)
            _savedAmbiencePositions[ambienceSource.clip.name] = ambienceSource.time;

        ambienceSource.DOKill();
        if (ambienceSource.isPlaying)
            ambienceSource.DOFade(0, fadeDuration).OnComplete(() => ambienceSource.Stop());
        else
            ambienceSource.Stop();
    }

    public void PauseAmbience(bool rememberPosition = true)
    {
        if (!ambienceSource || !ambienceSource.isPlaying) return;
        if (rememberPosition && ambienceSource.clip)
            _savedAmbiencePositions[ambienceSource.clip.name] = ambienceSource.time;
        ambienceSource.Pause();
    }

    public void ResumeAmbience(float fadeDuration = 0.75f)
    {
        if (!ambienceSource || !ambienceSource.clip) return;

        if (_savedAmbiencePositions.TryGetValue(ambienceSource.clip.name, out var t))
            ambienceSource.time = Mathf.Clamp(t, 0f, ambienceSource.clip.length - 0.05f);

        ambienceSource.volume = 0f;
        ambienceSource.Play();
        ambienceSource.DOFade(_lastAmbienceTargetVolume, fadeDuration);
    }

    public void StopByName(string name, float fadeDuration = 0.5f, bool rememberPosition = false)
    {
        Sound s = sounds.FirstOrDefault(sound => sound.name == name);
        if (s == null || s.clip == null)
        {
            Debug.LogWarning($"StopByName: '{name}' no encontrado.");
            return;
        }

        switch (s.channel)
        {
            case SoundChannel.Music:
                if (musicSource && musicSource.clip == s.clip)
                    StopMusic(fadeDuration, rememberPosition);
                break;

            case SoundChannel.Ambience:
                if (ambienceSource && ambienceSource.clip == s.clip)
                    StopAmbience(fadeDuration, rememberPosition);
                break;

            case SoundChannel.LoopingSFX:
                // Ya tienes StopLoopingSFX dedicado
                StopLoopingSFX();
                break;

            default:
                // One-shots (SFX/UI/Voice): PlayOneShot no se puede "parar" individualmente.
                // Como alternativa, detén COMPLETO el source del canal (corta TODO lo que suene allí):
                if (_oneShotSourceByChannel.TryGetValue(s.channel, out var src) && src)
                    src.Stop();
                break;
        }
    }

}
