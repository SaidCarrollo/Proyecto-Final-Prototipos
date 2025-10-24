using UnityEngine;

public class AudioManager : SingletonPersistent<AudioManager>
{
    public AudioSettings audioSettings;
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private const string MasterVolumeKey = "MasterVolume"; 
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    public override void Awake()
    {
        base.Awake();

        if (Instance == this)
        {
            LoadVolumeSettings();
        }
    }

    private void LoadVolumeSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f); 
        SetVolume(MasterVolumeKey, masterVolume, false);

        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        SetVolume(MusicVolumeKey, musicVolume, false);

        float sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
        SetVolume(SFXVolumeKey, sfxVolume, false);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void SetVolume(string parameter, float value, bool save = true)
    {
        if (audioSettings != null)
        {
            audioSettings.SetVolume(parameter, value);

            if (save)
            {
                PlayerPrefs.SetFloat(parameter, value);
                PlayerPrefs.Save();
            }
        }
    }

    public float GetVolume(string parameter)
    {
        return PlayerPrefs.GetFloat(parameter, 1f);
    }
}