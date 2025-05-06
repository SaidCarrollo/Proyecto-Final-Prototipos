using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSettings audioSettings;
    public AudioSource musicSource;
    public AudioSource sfxSource;

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void SetVolume(string parameter, float value)
    {
        audioSettings.SetVolume(parameter, value);
    }

    public float GetVolume(string parameter)
    {
        return audioSettings.GetVolume(parameter);
    }
}
