using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.EventSystems; // IMPORTANTE para detectar la UI

[RequireComponent(typeof(RectTransform))]
public class MobileLookZone : MonoBehaviour
{
    [SerializeField] private FirstPersonController controller;
    [SerializeField, Tooltip("Multiplicador extra solo para tactil")]
    private float touchSensitivity = 0.4f;

    [Header("Suavizado (Smoothness)")]
    [SerializeField, Tooltip("Qué tan rápido responde la cámara. Valores altos = más rígido, Valores bajos = más como 'hielo'. ~15-20 es ideal.")]
    private float smoothSpeed = 15f;

    private RectTransform rect;
    private int fingerId = -1; // -1 = ninguno asignado

    // Memoria para el suavizado matemático
    private Vector2 targetDelta;
    private Vector2 currentDelta;

    private void Awake()
    {
        rect = (RectTransform)transform;
    }

    private void Update()
    {
        // Si no hay pantalla táctil o no hay controller, no hacemos nada
        if (Touchscreen.current == null || controller == null)
            return;

        var ts = Touchscreen.current;
        bool keepCurrentFinger = false;

        // Reseteamos el objetivo a 0 por defecto cada frame
        targetDelta = Vector2.zero;

        for (int i = 0; i < ts.touches.Count; i++)
        {
            TouchControl touch = ts.touches[i];
            if (!touch.press.isPressed)
                continue;

            int thisId = touch.touchId.ReadValue();
            Vector2 screenPos = touch.position.ReadValue();

            // Si todavía no tenemos dedo asignado...
            if (fingerId == -1)
            {
                // 1. SOLUCIÓN ROBUSTA: Exigimos que el toque ACABE DE EMPEZAR (Began)
                // Así evitamos robar un dedo que viene arrastrándose desde otro lado.
                var phase = touch.phase.ReadValue();
                if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    // 2. SOLUCIÓN UI: Preguntamos si este toque está sobre un elemento de UI (Slider, Botón)
                    bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(thisId);

                    // 3. Si NO es UI y está en nuestro cuadro, nos adueñamos del dedo
                    if (!isOverUI && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos))
                    {
                        fingerId = thisId;
                        keepCurrentFinger = true;
                        targetDelta = touch.delta.ReadValue();
                    }
                }
            }
            else if (fingerId == thisId)
            {
                // Este es nuestro dedo actual, lo seguimos
                keepCurrentFinger = true;
                targetDelta = touch.delta.ReadValue();
            }
        }

        // Si el dedo que teníamos ya no está presionado, liberamos
        if (fingerId != -1 && !keepCurrentFinger)
        {
            fingerId = -1;
        }

        // --- SOLUCIÓN SUAVIDAD: INTERPOLACIÓN LINEAL (LERP) ---
        // En lugar de enviar el salto brusco, acercamos el valor actual al valor objetivo fluidamente
        currentDelta = Vector2.Lerp(currentDelta, targetDelta, Time.deltaTime * smoothSpeed);

        // Aplicamos a la cámara
        if (currentDelta.sqrMagnitude > 0.001f)
        {
            controller.AddLookInput(currentDelta * touchSensitivity);
        }
    }
}