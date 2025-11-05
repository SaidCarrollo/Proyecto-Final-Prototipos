using UnityEngine;

public class AudioSettingsController : MonoBehaviour
{
    [Header("Controles de Volumen")]
    public VolumeButtonControl masterControl;
    public VolumeButtonControl musicControl;
    public VolumeButtonControl sfxControl;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    void Awake()
    {
        // Asegurar que cada control tiene la clave correcta
        if (masterControl != null)
            masterControl.volumeKey = MasterVolumeKey;

        if (musicControl != null)
            musicControl.volumeKey = MusicVolumeKey;

        if (sfxControl != null)
            sfxControl.volumeKey = SFXVolumeKey;
    }
}
