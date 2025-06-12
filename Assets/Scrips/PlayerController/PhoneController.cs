using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject phonePanel;

    [Header("Events")]
    [Tooltip("Evento que se dispara para mostrar un mensaje en la UI.")]
    [SerializeField] private GameEventstring messageEvent;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference togglePhoneAction;
    [SerializeField] private InputActionReference interactAction;


    private void OnEnable()
    {
        if (togglePhoneAction != null) { }
        if (interactAction != null) { }
    }

    private void OnDisable()
    {
        if (togglePhoneAction != null) {  }
        if (interactAction != null) {  }
    }

    private void OnTogglePhone(InputAction.CallbackContext context)
    {
        if (phonePanel != null)
        {
            phonePanel.SetActive(!phonePanel.activeSelf);
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (phonePanel != null && phonePanel.activeInHierarchy)
        {
            Debug.Log("Llamando al 116 desde el panel...");

            if (messageEvent != null)
            {
                messageEvent.Raise("Llamada de emergencia realizada.");
            }

            phonePanel.SetActive(false);
        }
    }
}