using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BadgeAudioPlayer : MonoBehaviour
{
    public static BadgeAudioPlayer Instance { get; private set; }

    [Header("Clips de Audio")]
    [Tooltip("El sonido que se reproducirá al obtener un badge correcto.")]
    [SerializeField] private AudioClip goodBadgeClip;

    [Tooltip("El sonido que se reproducirá al cometer un error (badge incorrecto).")]
    [SerializeField] private AudioClip badBadgeClip;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
    }

    public void PlayGoodBadgeSound()
    {
        if (goodBadgeClip != null)
        {
            audioSource.PlayOneShot(goodBadgeClip);
        }
        else
        {
            Debug.LogWarning("No se ha asignado un AudioClip para 'Good Badge' en BadgeAudioPlayer.");
        }
    }

    public void PlayBadBadgeSound()
    {
        if (badBadgeClip != null)
        {
            audioSource.PlayOneShot(badBadgeClip);
        }
        else
        {
            Debug.LogWarning("No se ha asignado un AudioClip para 'Bad Badge' en BadgeAudioPlayer.");
        }
    }
}