using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(RectTransform))]
public class MobileLookZone : MonoBehaviour
{
    [SerializeField] private FirstPersonController controller;
    [SerializeField, Tooltip("Multiplicador extra solo para tactil")]
    private float touchSensitivity = 0.4f;

    private RectTransform rect;
    private int fingerId = -1; // -1 = ninguno asignado

    private void Awake()
    {
        rect = (RectTransform)transform;
    }

    private void Update()
    {
        // si no hay pantalla táctil o no hay controller, no hacemos nada
        if (Touchscreen.current == null || controller == null)
            return;

        var ts = Touchscreen.current;
        bool keepCurrentFinger = false;

        // recorrer todos los toques activos
        for (int i = 0; i < ts.touches.Count; i++)
        {
            TouchControl touch = ts.touches[i];
            if (!touch.press.isPressed)
                continue;

            int thisId = touch.touchId.ReadValue();
            Vector2 screenPos = touch.position.ReadValue();

            // si todavía no tenemos dedo asignado, vemos si este empezó dentro del rect
            if (fingerId == -1)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos))
                {
                    fingerId = thisId;
                    keepCurrentFinger = true;

                    Vector2 delta = touch.delta.ReadValue();
                    if (delta.sqrMagnitude > 0.001f)
                    {
                        controller.AddLookInput(delta * touchSensitivity);
                    }
                }
            }
            else if (fingerId == thisId)
            {
                // este es nuestro dedo actual, lo seguimos
                keepCurrentFinger = true;

                Vector2 delta = touch.delta.ReadValue();
                if (delta.sqrMagnitude > 0.001f)
                {
                    controller.AddLookInput(delta * touchSensitivity);
                }
            }
        }

        // si el dedo que teníamos ya no está presionado, liberamos
        if (fingerId != -1 && !keepCurrentFinger)
        {
            fingerId = -1;
        }
    }
}
