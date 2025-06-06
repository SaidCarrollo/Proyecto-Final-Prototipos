
using UnityEngine;
using UnityEngine.InputSystem; 

public class PhoneController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("El panel de la UI que representa el teléfono.")]
    [SerializeField] private GameObject phonePanel;

    [Header("Input Actions")]
    [Tooltip("Referencia a la acción para mostrar/ocultar el teléfono.")]
    [SerializeField] private InputActionReference togglePhoneAction;

    [Tooltip("Referencia a la acción para interactuar (llamar).")]
    [SerializeField] private InputActionReference interactAction; 

    [Header("Evento")]
    [Tooltip("El evento que se dispara cuando se realiza la llamada.")]
    [SerializeField] private GameEvent llamada116Event;

    private void OnEnable()
    {
        if (togglePhoneAction != null)
        {
            togglePhoneAction.action.Enable();
            togglePhoneAction.action.performed += OnTogglePhone;
        }
        if (interactAction != null)
        {
            interactAction.action.Enable(); //
            interactAction.action.performed += OnInteract; //
        }
    }

    private void OnDisable()
    {
        if (togglePhoneAction != null)
        {
            togglePhoneAction.action.performed -= OnTogglePhone;
            togglePhoneAction.action.Disable();
        }
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract; //
            interactAction.action.Disable(); //
        }
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
            if (llamada116Event != null)
            {
                Debug.Log("Llamando al 116 desde el panel...");
                llamada116Event.Raise(); 
            }

            phonePanel.SetActive(false);
        }
    }
}