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
    public void PlayMusic(string name, float volume = 1.0f, bool loop = true, float fadeDuration = 1.5f)
    {
        Sound s = sounds.FirstOrDefault(sound => sound.name == name);
        if (s == null || s.clip == null)
        {
            Debug.LogWarning($"Sonido musical: '{name}' no encontrado.");
            return;
        }

        if (musicSource.clip == s.clip && musicSource.isPlaying) return;

        musicSource.DOKill();

        if (musicSource.isPlaying)
        {
            musicSource.DOFade(0, fadeDuration * 0.5f).OnComplete(() =>
            {
                StartNewMusic(s, volume, loop, fadeDuration * 0.5f);
            });
        }
        else
        {
            StartNewMusic(s, volume, loop, fadeDuration);
        }
    }

    private void StartNewMusic(Sound s, float volume, bool loop, float fadeDuration)
    {
        if (musicGroup) musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.clip = s.clip;
        musicSource.loop = loop || s.loop;
        musicSource.volume = 0f;
        musicSource.Play();
        musicSource.DOFade(Mathf.Clamp01(s.volume * volume), fadeDuration);
    }

    // ------------------- Ambiente -------------------
    public void PlayAmbience(string name, float volume = 1.0f, bool loop = true, float fadeDuration = 1.5f)
    {
        Sound s = sounds.FirstOrDefault(sound => sound.name == name);
        if (s == null || s.clip == null)
        {
            Debug.LogWarning($"Sonido de ambiente: '{name}' no encontrado.");
            return;
        }

        if (ambienceSource.clip == s.clip && ambienceSource.isPlaying) return;

        ambienceSource.DOKill();

        if (ambienceSource.isPlaying)
        {
            ambienceSource.DOFade(0, fadeDuration * 0.5f).OnComplete(() =>
            {
                StartNewAmbience(s, volume, loop, fadeDuration * 0.5f);
            });
        }
        else
        {
            StartNewAmbience(s, volume, loop, fadeDuration);
        }
    }

    private void StartNewAmbience(Sound s, float volume, bool loop, float fadeDuration)
    {
        if (ambienceGroup) ambienceSource.outputAudioMixerGroup = ambienceGroup;
        ambienceSource.clip = s.clip;
        ambienceSource.loop = loop || s.loop;
        ambienceSource.volume = 0f;
        ambienceSource.Play();
        ambienceSource.DOFade(Mathf.Clamp01(s.volume * volume), fadeDuration);
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
    public void StopMusic(float fadeDuration = 1.0f)
    {
        if (musicSource && musicSource.isPlaying)
            musicSource.DOFade(0, fadeDuration).OnComplete(() => musicSource.Stop());
    }

    public void StopAmbience(float fadeDuration = 1.0f)
    {
        if (ambienceSource && ambienceSource.isPlaying)
            ambienceSource.DOFade(0, fadeDuration).OnComplete(() => ambienceSource.Stop());
    }
}
