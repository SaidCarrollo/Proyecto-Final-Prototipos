using UnityEngine;
using UnityEngine.UI; // Necesario para el componente Slider

public class MobileGrabSlider : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El objeto Grabber del jugador.")]
    [SerializeField] private ObjectGrabber objectGrabber;

    [Tooltip("El GameObject del Slider (o su panel padre) para ocultarlo/mostrarlo.")]
    [SerializeField] private GameObject sliderVisualContainer;

    [Tooltip("El componente Slider en sí.")]
    [SerializeField] private Slider distanceSlider;

    private bool isHolding = false;

    void Start()
    {
        // 1. Validaciones
        if (objectGrabber == null)
        {
            Debug.LogError("MobileGrabSlider: No has asignado el ObjectGrabber.");
            enabled = false;
            return;
        }

        // 2. Suscribirse a los eventos del Grabber para saber cuándo aparecer
        objectGrabber.OnObjectGrabbed += HandleObjectGrabbed;
        objectGrabber.OnObjectReleased += HandleObjectReleased;

        // 3. Configurar el listener del Slider
        if (distanceSlider != null)
        {
            distanceSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        // 4. Empezar oculto
        if (sliderVisualContainer != null) sliderVisualContainer.SetActive(false);
    }

    void OnDestroy()
    {
        // Buena práctica: desuscribirse para evitar errores al cambiar de escena
        if (objectGrabber != null)
        {
            objectGrabber.OnObjectGrabbed -= HandleObjectGrabbed;
            objectGrabber.OnObjectReleased -= HandleObjectReleased;
        }
    }

    // Se llama automáticamente cuando el Grabber agarra algo
    private void HandleObjectGrabbed(GameObject obj)
    {
        isHolding = true;

        if (sliderVisualContainer != null && distanceSlider != null)
        {
            // A) Configurar límites del slider
            distanceSlider.minValue = objectGrabber.GetMinHoldDistance();
            distanceSlider.maxValue = objectGrabber.GetMaxHoldDistance();

            // B) Sincronizar el slider a la posición REAL del objeto al ser agarrado
            // USAR SetValueWithoutNotify ES LA CLAVE para que no salte el objeto de golpe
            distanceSlider.SetValueWithoutNotify(objectGrabber.GetCurrentHoldDistance());

            // C) Mostrar
            sliderVisualContainer.SetActive(true);
        }
    }

    // Se llama automáticamente cuando sueltas el objeto
    private void HandleObjectReleased(GameObject obj)
    {
        isHolding = false;
        if (sliderVisualContainer != null)
        {
            sliderVisualContainer.SetActive(false);
        }
    }

    // Se llama cada vez que mueves el slider con el dedo
    private void OnSliderValueChanged(float value)
    {
        if (isHolding && objectGrabber != null)
        {
            objectGrabber.SetHoldDistance(value);
        }
    }

    // Opcional: Update para sincronizar si TAMBIÉN usas la rueda del mouse a la vez (PC/Híbrido)
    void Update()
    {
        if (isHolding && objectGrabber != null && distanceSlider != null)
        {
            // Si el valor interno cambió por otra razón (ej. rueda del mouse), actualizamos el slider visual
            if (Mathf.Abs(distanceSlider.value - objectGrabber.GetCurrentHoldDistance()) > 0.01f)
            {
                distanceSlider.SetValueWithoutNotify(objectGrabber.GetCurrentHoldDistance());
            }
        }
    }
}