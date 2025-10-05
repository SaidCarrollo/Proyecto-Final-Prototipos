using UnityEngine;
using UnityEngine.Events;

public class ValveMinigame : MonoBehaviour
{
    [Header("Referencias de Escena")]
    [Tooltip("La cámara principal del jugador (normalmente dentro del objeto del jugador).")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("Una cámara secundaria que enfoca de cerca la válvula.")]
    [SerializeField] private Camera minigameCamera;
    [Tooltip("El componente FirstPersonController del jugador para desactivar su movimiento.")]
    [SerializeField] private FirstPersonController playerController;
    [Tooltip("El Transform del objeto de la válvula que va a rotar.")]
    [SerializeField] private Transform valveTransform;

    [Header("Configuración del Minijuego")]
    [Tooltip("Sensibilidad con la que el movimiento del mouse se traduce en rotación.")]
    [SerializeField] private float rotationSensitivity = 20f;
    [Tooltip("La cantidad total de grados que se debe girar la válvula para ganar.")]
    [SerializeField] private float requiredRotation = 360f;
    [Tooltip("Eje sobre el cual rotará la válvula (p. ej., Vector3.up para el eje Y).")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Header("Eventos")]
    [Tooltip("Este evento se dispara cuando el minijuego se completa con éxito.")]
    public UnityEvent OnMinigameCompleted;

    private bool isMinigameActive = false;
    private float currentRotation = 0f;
    private float initialMouseX;

    void Start()
    {
        // Asegúrate de que la cámara del minijuego esté desactivada al empezar.
        if (minigameCamera != null)
        {
            minigameCamera.gameObject.SetActive(false);
        }
    }

    // Este método público será llamado por el script 'Interactable'
    public void StartMinigame()
    {
        if (isMinigameActive) return;

        isMinigameActive = true;
        currentRotation = 0f;

        // Cambiar cámaras
        mainCamera.gameObject.SetActive(false);
        minigameCamera.gameObject.SetActive(true);

        // Desactivar controles del jugador
        playerController.SetInputEnabled(false);

        // Mostrar y desbloquear el cursor para interactuar con la válvula
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Guardar la posición inicial del mouse para calcular el arrastre
        initialMouseX = Input.mousePosition.x;
    }

    void Update()
    {
        if (!isMinigameActive) return;

        // El jugador debe mantener presionado el clic izquierdo para girar
        if (Input.GetMouseButton(0))
        {
            // Calcula el delta (cambio) en la posición X del mouse
            float mouseDeltaX = Input.GetAxis("Mouse X");

            // Calcula la rotación a aplicar en este frame
            float rotationAmount = mouseDeltaX * rotationSensitivity;

            // Aplica la rotación al objeto de la válvula
            valveTransform.Rotate(rotationAxis, rotationAmount);

            // Acumula la rotación
            currentRotation += Mathf.Abs(rotationAmount);
        }

        // Comprueba si se ha alcanzado la rotación necesaria
        if (currentRotation >= requiredRotation)
        {
            CompleteMinigame();
        }
    }

    private void CompleteMinigame()
    {
        isMinigameActive = false;

        Debug.Log("¡Minijuego de la válvula completado!");

        // Dispara el evento que notificará a los otros scripts
        OnMinigameCompleted?.Invoke();

        // Restaurar el estado del jugador
        minigameCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
        playerController.SetInputEnabled(true); // Esto también bloqueará el cursor de nuevo

        // Opcional: Desactiva este script para que no se pueda volver a usar
        this.enabled = false;
    }
}