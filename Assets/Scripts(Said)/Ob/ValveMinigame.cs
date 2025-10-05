using UnityEngine;
using UnityEngine.Events;

public class ValveMinigame : MonoBehaviour
{
    [Header("Referencias de Escena")]
    [Tooltip("La c�mara principal del jugador (normalmente dentro del objeto del jugador).")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("Una c�mara secundaria que enfoca de cerca la v�lvula.")]
    [SerializeField] private Camera minigameCamera;
    [Tooltip("El componente FirstPersonController del jugador para desactivar su movimiento.")]
    [SerializeField] private FirstPersonController playerController;
    [Tooltip("El Transform del objeto de la v�lvula que va a rotar.")]
    [SerializeField] private Transform valveTransform;

    [Header("Configuraci�n del Minijuego")]
    [Tooltip("Sensibilidad con la que el movimiento del mouse se traduce en rotaci�n.")]
    [SerializeField] private float rotationSensitivity = 20f;
    [Tooltip("La cantidad total de grados que se debe girar la v�lvula para ganar.")]
    [SerializeField] private float requiredRotation = 360f;
    [Tooltip("Eje sobre el cual rotar� la v�lvula (p. ej., Vector3.up para el eje Y).")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Header("Eventos")]
    [Tooltip("Este evento se dispara cuando el minijuego se completa con �xito.")]
    public UnityEvent OnMinigameCompleted;

    private bool isMinigameActive = false;
    private float currentRotation = 0f;
    private float initialMouseX;

    void Start()
    {
        // Aseg�rate de que la c�mara del minijuego est� desactivada al empezar.
        if (minigameCamera != null)
        {
            minigameCamera.gameObject.SetActive(false);
        }
    }

    // Este m�todo p�blico ser� llamado por el script 'Interactable'
    public void StartMinigame()
    {
        if (isMinigameActive) return;

        isMinigameActive = true;
        currentRotation = 0f;

        // Cambiar c�maras
        mainCamera.gameObject.SetActive(false);
        minigameCamera.gameObject.SetActive(true);

        // Desactivar controles del jugador
        playerController.SetInputEnabled(false);

        // Mostrar y desbloquear el cursor para interactuar con la v�lvula
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Guardar la posici�n inicial del mouse para calcular el arrastre
        initialMouseX = Input.mousePosition.x;
    }

    void Update()
    {
        if (!isMinigameActive) return;

        // El jugador debe mantener presionado el clic izquierdo para girar
        if (Input.GetMouseButton(0))
        {
            // Calcula el delta (cambio) en la posici�n X del mouse
            float mouseDeltaX = Input.GetAxis("Mouse X");

            // Calcula la rotaci�n a aplicar en este frame
            float rotationAmount = mouseDeltaX * rotationSensitivity;

            // Aplica la rotaci�n al objeto de la v�lvula
            valveTransform.Rotate(rotationAxis, rotationAmount);

            // Acumula la rotaci�n
            currentRotation += Mathf.Abs(rotationAmount);
        }

        // Comprueba si se ha alcanzado la rotaci�n necesaria
        if (currentRotation >= requiredRotation)
        {
            CompleteMinigame();
        }
    }

    private void CompleteMinigame()
    {
        isMinigameActive = false;

        Debug.Log("�Minijuego de la v�lvula completado!");

        // Dispara el evento que notificar� a los otros scripts
        OnMinigameCompleted?.Invoke();

        // Restaurar el estado del jugador
        minigameCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
        playerController.SetInputEnabled(true); // Esto tambi�n bloquear� el cursor de nuevo

        // Opcional: Desactiva este script para que no se pueda volver a usar
        this.enabled = false;
    }
}