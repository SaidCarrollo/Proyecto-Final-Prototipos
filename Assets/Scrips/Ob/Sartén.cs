
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GrabbableObject))]
public class SartenCollider : MonoBehaviour
{
    [Header("Configuración de Snap del Trapo")]
    [SerializeField] private Transform snapPosition; 
    [SerializeField] private string trapoTag = "TrapoMojado"; 

    [Header("Eventos de Fuego")]
    [SerializeField] private FloatEvent fireIntensityEvent; 

    [Header("Interacción con Agua")]
    [SerializeField] private string waterTag = "Water"; 


    private void Awake()
    {

        if (snapPosition == null)
        {
            Debug.LogError("Error: El 'Snap Position' no está asignado en el SartenCollider del objeto '" + gameObject.name + "'. El trapo no se podrá colocar correctamente.", this);
        }

        if (snapPosition != null && !snapPosition.IsChildOf(this.transform))
        {
            Debug.LogWarning("Advertencia: El 'Snap Position' (" + snapPosition.name + ") no es parte de la jerarquía del objeto Sarten (" + gameObject.name + "). " +
                             "Si la sartén se mueve, el trapo anclado podría no moverse con ella.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(trapoTag))
        {
            HandleTrapoInteraction(other);
        }
        else if (other.CompareTag(waterTag))
        {

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
            else 
            {
                Debug.Log($"El trapo '{trapoCollider.name}' está seco. Aumentando fuego.", this);
                if (fireIntensityEvent != null)
                {
                    fireIntensityEvent.Raise(0.8f); 
                }
            }
        }
    }

    private void SnapTrapoToSarten(Transform trapoTransform)
    {
        Rigidbody rbTrapo = trapoTransform.GetComponent<Rigidbody>();
        if (rbTrapo != null)
        {
            rbTrapo.isKinematic = true; 
            rbTrapo.linearVelocity = Vector3.zero;
            rbTrapo.angularVelocity = Vector3.zero;
        }

        trapoTransform.SetParent(snapPosition);
        trapoTransform.localPosition = Vector3.zero;
        trapoTransform.localRotation = Quaternion.identity;

        Collider trapoCollider = trapoTransform.GetComponent<Collider>();
        if (trapoCollider != null)
        {
            trapoCollider.enabled = false;
        }
        Debug.Log($"Trapo '{trapoTransform.name}' anclado a la sartén en '{snapPosition.name}'.", this);
    }

    private void HandlePanInWater(Collider waterCollider)
    {
        Debug.Log($"La sartén '{gameObject.name}' ha entrado en el agua: '{waterCollider.name}'. Se llamará a OnPanSubmerged.", this);
        OnPanSubmerged();

    }

    private void OnPanSubmerged()
    {

        Debug.Log($"Método OnPanSubmerged llamado para la sartén '{gameObject.name}'. No hay acciones implementadas actualmente.", this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(waterTag))
        {
            Debug.Log($"La sartén '{gameObject.name}' ha salido del agua: '{other.name}'.", this);

        }
    }
}