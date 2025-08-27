using UnityEngine;
using UnityEngine.Events;

public class PhoneUsageTrigger : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Referencia al panel del tel�fono para saber si est� activo.")]
    [SerializeField] private GameObject phonePanel;

    [Tooltip("Tag que debe tener el objeto que entra al trigger (generalmente el jugador).")]
    [SerializeField] private string playerTag = "Player";

    [Header("Events")]
    [Tooltip("Se dispara cuando el jugador usa el tel�fono dentro de la zona.")]
    public UnityEvent OnPhoneUsedInZone;

    [Tooltip("Se dispara cuando el jugador deja de usar el tel�fono o sale de la zona.")]
    public UnityEvent OnPhoneUsageStoppedInZone;

    [Header("Optional Feedback")]
    [SerializeField] private BadgeManager badgeManager;
    [SerializeField] private string badgeID = "PhoneInRestrictedArea";
    [SerializeField] private GameEventstring messageEvent;
    [SerializeField, TextArea(2, 4)] private string messageToShow = "No deber�as usar el tel�fono aqu�.";

    private bool isPlayerInZone = false;
    private bool wasPhoneActiveInZone = false;

    private void Awake()
    {
        // Nos aseguramos de que el collider sea un trigger
        var triggerCollider = GetComponent<Collider>();
        if (!triggerCollider.isTrigger)
        {
            Debug.LogError($"El Collider en '{gameObject.name}' no est� configurado como 'Is Trigger'. El script no funcionar�.", this);
            enabled = false;
        }

        if (phonePanel == null)
        {
            Debug.LogError("La referencia a 'phonePanel' no est� asignada en el Inspector.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprobamos si quien entr� es el jugador
        if (other.CompareTag(playerTag))
        {
            isPlayerInZone = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Comprobamos si quien sali� es el jugador
        if (other.CompareTag(playerTag))
        {
            isPlayerInZone = false;
        }
    }

    private void Update()
    {
        // Verificamos si el jugador est� en la zona y el tel�fono est� activo
        bool isPhoneActiveNow = isPlayerInZone && phonePanel.activeSelf;

        // Si el estado cambi� (de no usar el tel�fono en la zona a s� usarlo)
        if (isPhoneActiveNow && !wasPhoneActiveInZone)
        {
            Debug.Log("<color=orange>ACCI�N DETECTADA:</color> El jugador est� usando el tel�fono en la zona prohibida.");

            // Disparamos el evento principal
            OnPhoneUsedInZone.Invoke();

            // L�gica de feedback opcional
            if (badgeManager != null)
            {
                badgeManager.UnlockBadge(badgeID);
            }
            if (messageEvent != null && !string.IsNullOrEmpty(messageToShow))
            {
                messageEvent.Raise(messageToShow);
            }
        }
        // Si el estado cambi� (de s� usar el tel�fono a no usarlo o salir de la zona)
        else if (!isPhoneActiveNow && wasPhoneActiveInZone)
        {
            Debug.Log("El jugador ha guardado el tel�fono o ha salido de la zona.");
            OnPhoneUsageStoppedInZone.Invoke();
        }

        // Actualizamos el estado para el pr�ximo frame
        wasPhoneActiveInZone = isPhoneActiveNow;
    }
}