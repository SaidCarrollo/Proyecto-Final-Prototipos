using UnityEngine;
using UnityEngine.Audio;
[CreateAssetMenu(fileName = "AudioSettings", menuName = "Audio/Settings")]
public class AudioSettings : ScriptableObject
{
    public AudioMixer mixer;

    public void SetVolume(string parameter, float value)
    {
        mixer.SetFloat(parameter, Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
    }

    public float GetVolume(string parameter)
    {
        float value;
        if (mixer.GetFloat(parameter, out value))
        {
            return Mathf.Pow(10, value / 20);
        }
        return 1f;
    }
}
