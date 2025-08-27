using UnityEngine;
using UnityEngine.Events;

public class PhoneUsageTrigger : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Referencia al panel del teléfono para saber si está activo.")]
    [SerializeField] private GameObject phonePanel;

    [Tooltip("Tag que debe tener el objeto que entra al trigger (generalmente el jugador).")]
    [SerializeField] private string playerTag = "Player";

    [Header("Events")]
    [Tooltip("Se dispara cuando el jugador usa el teléfono dentro de la zona.")]
    public UnityEvent OnPhoneUsedInZone;

    [Tooltip("Se dispara cuando el jugador deja de usar el teléfono o sale de la zona.")]
    public UnityEvent OnPhoneUsageStoppedInZone;

    [Header("Optional Feedback")]
    [SerializeField] private BadgeManager badgeManager;
    [SerializeField] private string badgeID = "PhoneInRestrictedArea";
    [SerializeField] private GameEventstring messageEvent;
    [SerializeField, TextArea(2, 4)] private string messageToShow = "No deberías usar el teléfono aquí.";

    private bool isPlayerInZone = false;
    private bool wasPhoneActiveInZone = false;

    private void Awake()
    {
        // Nos aseguramos de que el collider sea un trigger
        var triggerCollider = GetComponent<Collider>();
        if (!triggerCollider.isTrigger)
        {
            Debug.LogError($"El Collider en '{gameObject.name}' no está configurado como 'Is Trigger'. El script no funcionará.", this);
            enabled = false;
        }

        if (phonePanel == null)
        {
            Debug.LogError("La referencia a 'phonePanel' no está asignada en el Inspector.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprobamos si quien entró es el jugador
        if (other.CompareTag(playerTag))
        {
            isPlayerInZone = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Comprobamos si quien salió es el jugador
        if (other.CompareTag(playerTag))
        {
            isPlayerInZone = false;
        }
    }

    private void Update()
    {
        // Verificamos si el jugador está en la zona y el teléfono está activo
        bool isPhoneActiveNow = isPlayerInZone && phonePanel.activeSelf;

        // Si el estado cambió (de no usar el teléfono en la zona a sí usarlo)
        if (isPhoneActiveNow && !wasPhoneActiveInZone)
        {
            Debug.Log("<color=orange>ACCIÓN DETECTADA:</color> El jugador está usando el teléfono en la zona prohibida.");

            // Disparamos el evento principal
            OnPhoneUsedInZone.Invoke();

            // Lógica de feedback opcional
            if (badgeManager != null)
            {
                badgeManager.UnlockBadge(badgeID);
            }
            if (messageEvent != null && !string.IsNullOrEmpty(messageToShow))
            {
                messageEvent.Raise(messageToShow);
            }
        }
        // Si el estado cambió (de sí usar el teléfono a no usarlo o salir de la zona)
        else if (!isPhoneActiveNow && wasPhoneActiveInZone)
        {
            Debug.Log("El jugador ha guardado el teléfono o ha salido de la zona.");
            OnPhoneUsageStoppedInZone.Invoke();
        }

        // Actualizamos el estado para el próximo frame
        wasPhoneActiveInZone = isPhoneActiveNow;
    }
}