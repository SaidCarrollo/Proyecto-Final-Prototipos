// SartenCollider.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GrabbableObject))]
public class SartenCollider : MonoBehaviour
{
    [Header("Configuración de Snap del Trapo")]
    [SerializeField] private Transform snapPosition; // Objeto vacío hijo de la sartén que marca la posición de snap
    [SerializeField] private string trapoTag = "TrapoMojado"; // Tag del objeto "trapo"

    [Header("Eventos de Fuego")]
    [SerializeField] private FloatEvent fireIntensityEvent; // Evento para la intensidad del fuego (existente)

    [Header("Interacción con Agua")]
    [SerializeField] private string waterTag = "Water"; // Tag para los objetos de agua que la sartén detectará

    // private GrabbableObject sartenGrabbableObject; // Referencia al GrabbableObject de la sartén, si se necesita acceso directo a sus propiedades

    private void Awake()
    {
        // sartenGrabbableObject = GetComponent<GrabbableObject>(); // Se obtiene si es necesario

        if (snapPosition == null)
        {
            Debug.LogError("Error: El 'Snap Position' no está asignado en el SartenCollider del objeto '" + gameObject.name + "'. El trapo no se podrá colocar correctamente.", this);
        }
        // Es crucial que snapPosition sea un hijo (directo o indirecto) de la sartén
        // para que el trapo se mueva con la sartén cuando esta sea agarrada.
        if (snapPosition != null && !snapPosition.IsChildOf(this.transform))
        {
            Debug.LogWarning("Advertencia: El 'Snap Position' (" + snapPosition.name + ") no es parte de la jerarquía del objeto Sarten (" + gameObject.name + "). " +
                             "Si la sartén se mueve, el trapo anclado podría no moverse con ella.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Interacción con el Trapo (lógica existente mantenida)
        if (other.CompareTag(trapoTag))
        {
            HandleTrapoInteraction(other);
        }
        // 2. Interacción de la Sartén con el Agua (nueva lógica)
        else if (other.CompareTag(waterTag))
        {
            // Esta colisión ocurre cuando la sartén (el GameObject de este script)
            // entra en un trigger con el tag especificado en 'waterTag'.
            HandlePanInWater(other);
        }
    }

    private void HandleTrapoInteraction(Collider trapoCollider)
    {
        GrabbableObject trapoGrabbable = trapoCollider.GetComponent<GrabbableObject>();

        ObjectGrabber grabberHoldingTrapo = trapoCollider.GetComponentInParent<ObjectGrabber>();

        if (trapoGrabbable != null && (grabberHoldingTrapo == null || !grabberHoldingTrapo.IsHoldingObject()))
        {
            Debug.Log($"Trapo '{trapoCollider.name}' detectado por la sartén '{gameObject.name}'. Intentando anclar.", trapoCollider.gameObject);
            SnapTrapoToSarten(trapoCollider.transform);

            if (trapoGrabbable.EstaMojado)
            {
                Debug.Log($"El trapo '{trapoCollider.name}' está mojado. Apagando fuego.", this);
                if (fireIntensityEvent != null)
                {
                    fireIntensityEvent.Raise(0f);
                }
            }
            else // Trapo seco
            {
                Debug.Log($"El trapo '{trapoCollider.name}' está seco. Aumentando fuego.", this);
                if (fireIntensityEvent != null)
                {
                    fireIntensityEvent.Raise(0.8f); // Aumenta la intensidad del fuego
                }
            }
        }
    }

    private void SnapTrapoToSarten(Transform trapoTransform)
    {
        Rigidbody rbTrapo = trapoTransform.GetComponent<Rigidbody>();
        if (rbTrapo != null)
        {
            rbTrapo.isKinematic = true; // Hace el trapo kinemático para que no responda a la física externa.
            rbTrapo.linearVelocity = Vector3.zero;
            rbTrapo.angularVelocity = Vector3.zero;
        }

        // Emparentar el trapo al snapPosition y resetear su posición/rotación local.
        // snapPosition debe ser un hijo de la sartén para que el trapo se mueva con ella.
        trapoTransform.SetParent(snapPosition);
        trapoTransform.localPosition = Vector3.zero;
        trapoTransform.localRotation = Quaternion.identity;

        // Desactivar el collider del trapo para evitar interacciones adicionales, como en el script original.
        // Esto significa que el trapo no podrá ser agarrado de nuevo ni interactuará físicamente
        // a menos que su collider se reactive por otra lógica.
        Collider trapoCollider = trapoTransform.GetComponent<Collider>();
        if (trapoCollider != null)
        {
            trapoCollider.enabled = false;
        }
        Debug.Log($"Trapo '{trapoTransform.name}' anclado a la sartén en '{snapPosition.name}'.", this);
    }

    private void HandlePanInWater(Collider waterCollider)
    {
        // Este método se invoca cuando la sartén entra en un objeto con el tag 'waterTag'.
        Debug.Log($"La sartén '{gameObject.name}' ha entrado en el agua: '{waterCollider.name}'. Se llamará a OnPanSubmerged.", this);
        OnPanSubmerged();

        // Nota: Si la sartén tiene el componente GrabbableObject y el tag "Grab" (o el tag que use tu WaterTrigger.cs),
        // y el objeto 'waterCollider' tiene un script WaterTrigger, la sartén también se "mojará"
        // (cambio de color, etc.) automáticamente debido a la lógica en GrabbableObject.cs y WaterTrigger.cs.
        // No se necesita llamar a sartenGrabbableObject.SetWet(true) explícitamente aquí para ese efecto.
    }

    private void OnPanSubmerged()
    {
        // Este método se llama cuando la sartén entra en el agua.
        // Actualmente está vacío según tu solicitud.
        // Aquí puedes añadir la lógica futura que necesites (ej: apagar fuego, limpiar sartén, etc.).
        Debug.Log($"Método OnPanSubmerged llamado para la sartén '{gameObject.name}'. No hay acciones implementadas actualmente.", this);
    }

    // Opcional: Manejar la salida del agua si es necesario en el futuro.
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(waterTag))
        {
            Debug.Log($"La sartén '{gameObject.name}' ha salido del agua: '{other.name}'.", this);
            // Podrías llamar a un método OnPanExitedWater() aquí si necesitas lógica específica.
        }
    }
}