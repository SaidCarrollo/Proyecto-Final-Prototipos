using UnityEngine;
using UnityEngine.UI;

public class DangerIndicatorUI : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Cámara que mira la escena (normalmente la del jugador). Si se deja vacío, usa Camera.main.")]
    [SerializeField] private Camera targetCamera;

    [Tooltip("Transform del peligro (centro del fuego / fuga de gas).")]
    [SerializeField] private Transform hazardTarget;

    [Tooltip("Transform del jugador (FirstPersonController).")]
    [SerializeField] private Transform player;

    [Tooltip("RectTransform del icono de peligro en la UI.")]
    [SerializeField] private RectTransform iconRect;

    [Header("Zona segura (collider)")]
    [Tooltip("Collider que define el área donde NO se muestra el icono. Si el jugador está dentro, el icono se oculta.")]
    [SerializeField] private Collider safeZoneCollider;

    [Tooltip("Si está activo y hay collider, se ignora el umbral de distancia.")]
    [SerializeField] private bool useSafeZoneCollider = true;

    [Header("Fallback por distancia (si no hay collider)")]
    [Tooltip("Si NO se usa collider de zona, el icono aparece solo si estás más lejos que esta distancia.")]
    [SerializeField] private float showWhenFurtherThan = 3f;

    [Header("Comportamiento")]
    [Tooltip("Si está en TRUE, oculta el icono cuando realmente estás mirando al peligro y está en pantalla.")]
    [SerializeField] private bool hideWhenLookingAtTarget = true;

    [Tooltip("Umbral de 'estoy mirando al objetivo' (1 = exactamente enfrente, 0 = 90°, -1 = completamente detrás).")]
    [SerializeField] private float lookDotThreshold = 0.6f;

    private bool indicatorEnabledByGame = false;
    private Quaternion initialIconRotation;
    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (iconRect != null)
        {
            initialIconRotation = iconRect.rotation; // 🔹 Guardamos la rotación original
            iconRect.gameObject.SetActive(false);
        }
    }
    public void SetIndicatorEnabled(bool enabled)
    {
        indicatorEnabledByGame = enabled;

        if (!enabled && iconRect != null)
        {
            iconRect.gameObject.SetActive(false);
        }
    }

    public void SetHazardTarget(Transform newTarget)
    {
        hazardTarget = newTarget;
    }

    private void LateUpdate()
    {
        if (!indicatorEnabledByGame ||
            hazardTarget == null ||
            player == null ||
            iconRect == null ||
            targetCamera == null)
        {
            return;
        }

        // 1) Comprobar si estamos dentro de la zona segura (collider)
        if (useSafeZoneCollider && safeZoneCollider != null)
        {
            bool insideSafeZone = safeZoneCollider.bounds.Contains(player.position);

            if (insideSafeZone)
            {
                // Dentro de la zona => icono apagado
                if (iconRect.gameObject.activeSelf)
                    iconRect.gameObject.SetActive(false);

                return;
            }
        }
        else
        {
            // Fallback: usamos distancia al peligro si no hay collider
            float distance = Vector3.Distance(player.position, hazardTarget.position);
            if (distance <= showWhenFurtherThan)
            {
                if (iconRect.gameObject.activeSelf)
                    iconRect.gameObject.SetActive(false);
                return;
            }
        }

        // 2) Dirección hacia el objetivo para saber si lo estoy mirando
        Vector3 toTarget = (hazardTarget.position - player.position).normalized;
        float dot = Vector3.Dot(targetCamera.transform.forward, toTarget);

        // 3) Posición en pantalla del objetivo
        Vector3 screenPos = targetCamera.WorldToScreenPoint(hazardTarget.position);
        bool isBehind = screenPos.z < 0f;

        if (isBehind)
        {
            // Si está detrás de la cámara, espejamos posición
            screenPos.x = Screen.width - screenPos.x;
            screenPos.y = Screen.height - screenPos.y;
            screenPos.z = 0f;
        }

        bool isOnScreen =
            screenPos.x >= 0f && screenPos.x <= Screen.width &&
            screenPos.y >= 0f && screenPos.y <= Screen.height &&
            !isBehind;

        // 4) Si lo estoy mirando y está en pantalla, puedo ocultar el icono
        if (hideWhenLookingAtTarget && isOnScreen && dot >= lookDotThreshold)
        {
            if (iconRect.gameObject.activeSelf)
                iconRect.gameObject.SetActive(false);
            return;
        }

        // 5) Asegurar que el icono está activo
        if (!iconRect.gameObject.activeSelf)
            iconRect.gameObject.SetActive(true);

        // 6) Si está en pantalla, lo ponemos justo encima del objetivo.
        //    Si está fuera, lo clampeamos al borde (tipo "se queda en una esquina").
        Vector3 finalScreenPos = screenPos;

        if (!isOnScreen)
        {
            float padding = 40f; // puedes exponer esto si quieres
            finalScreenPos.x = Mathf.Clamp(screenPos.x, padding, Screen.width - padding);
            finalScreenPos.y = Mathf.Clamp(screenPos.y, padding, Screen.height - padding);
            finalScreenPos.z = 0f;
        }

        //iconRect.position = finalScreenPos;

        //// 7) Rotación del icono para que apunte hacia el objetivo (opcional)
        //Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        //Vector2 dir = ((Vector2)finalScreenPos - screenCenter).normalized;
        //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //iconRect.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        iconRect.position = finalScreenPos;

        // 🔹 Mantener siempre la misma rotación (sin girar)
        iconRect.rotation = initialIconRotation;
    }
}

