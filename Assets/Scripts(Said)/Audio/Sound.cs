using UnityEngine;

[System.Serializable]
public enum SoundChannel
{
    Music,
    Ambience,
    SFX,
    Voice,
    UI,
    LoopingSFX
}

[System.Serializable]
public class Sound
{
    public string name;              // Ej: "ButtonClick", "Act1Music"
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 0.8f;

    [Header("Canal (afecta al AudioMixer Group)")]
    public SoundChannel channel = SoundChannel.SFX;

    [Tooltip("Para música/ambiente que deban quedarse sonando")]
    public bool loop = false;
}
