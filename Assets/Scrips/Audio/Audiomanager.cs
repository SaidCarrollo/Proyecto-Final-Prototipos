using UnityEngine;

public class AudioManager : SingletonPersistent<AudioManager>
{
    public AudioSettings audioSettings;
    public AudioSource musicSource;
    public AudioSource sfxSource;

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

    public void SetVolume(string parameter, float value)
    {
        if (audioSettings != null)
        {
            audioSettings.SetVolume(parameter, value);
        }
    }

    public float GetVolume(string parameter)
    {
        if (audioSettings != null)
        {
            return audioSettings.GetVolume(parameter);
        }
        return 1f; 
    }
}