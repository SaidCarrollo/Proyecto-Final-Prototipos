using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GrabbableObject))]
public class SartenCollider : MonoBehaviour
{
    [Header("Configuración de Snap del Trapo")]
    [SerializeField] private Transform snapPosition;
    [SerializeField] private string trapoTag = "TrapoMojado";
    [SerializeField] private string tapaTag = "TapaMetalica";
    [Header("Eventos de Fuego")]
    [SerializeField] private FloatEvent fireIntensityEvent;

    [Header("Sobrevivir o morir")]
    [SerializeField] private GameEvent onPlayerSurvivedEvent;
    [SerializeField] private GameEvent onPlayerDeathEvent;
    [Tooltip("Evento para activar la viñeta.")]
    [SerializeField] private VignetteEvent vignetteEvent;
    [Header("Interacción con Agua")]
    [SerializeField] private string waterTag = "Water";
    [SerializeField] private BadgeManager badgeManager;
    private bool fuegoEstaDescontrolado = false;
    private bool haSidoSumergida = false;
    public void ActivarFuegoDescontrolado()
    {
        Debug.Log($"FUEGO DESCONTROLADO ACTIVADO en {gameObject.name}. Ya no se pueden colocar más objetos.");
        gameObject.layer = LayerMask.NameToLayer("Default");
        fuegoEstaDescontrolado = true;
    }

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
        if (fuegoEstaDescontrolado) return;

        if (other.CompareTag(trapoTag))
        {
            HandleTrapoInteraction(other);
        }
        else if (other.CompareTag(tapaTag)) 
        {
            HandleTapaInteraction(other);
        }
        else if (other.CompareTag(waterTag))
        {
            HandlePanInWater(other);
        }
    }
    private void HandleTapaInteraction(Collider tapaCollider)
    {
        ObjectGrabber grabberHoldingTapa = tapaCollider.GetComponentInParent<ObjectGrabber>();
        if (grabberHoldingTapa != null && grabberHoldingTapa.IsHoldingObject()) return;

        Debug.Log($"Tapa '{tapaCollider.name}' detectada por la sartén '{gameObject.name}'. Apagando el fuego.", this);
        SnapObjectToSarten(tapaCollider.transform);

        if (fireIntensityEvent != null)
        {
            vignetteEvent.Raise(Color.green, 0.4f, 2f);
            fireIntensityEvent.Raise(0f);
            if (badgeManager != null)
            {
                badgeManager.UnlockBadge("FuegoApagadoConTapa");
            }
            if (onPlayerSurvivedEvent != null)
            {
                onPlayerSurvivedEvent.Raise();
                Debug.Log("EVENTO DE SUPERVIVENCIA PUBLICADO (Tapa)");
            }
        }
    }
    private void HandleTrapoInteraction(Collider trapoCollider)
    {
        GrabbableObject trapoGrabbable = trapoCollider.GetComponent<GrabbableObject>();

        ObjectGrabber grabberHoldingTrapo = trapoCollider.GetComponentInParent<ObjectGrabber>();

        if (trapoGrabbable != null && (grabberHoldingTrapo == null || !grabberHoldingTrapo.IsHoldingObject()))
        {
            Debug.Log($"Trapo '{trapoCollider.name}' detectado por la sartén '{gameObject.name}'. Intentando anclar.", trapoCollider.gameObject);
            SnapObjectToSarten(trapoCollider.transform);

            if (trapoGrabbable.EstaMojado)
            {
                Debug.Log($"El trapo '{trapoCollider.name}' está mojado. Apagando fuego.", this);
                if (fireIntensityEvent != null)
                {
                    vignetteEvent.Raise(Color.green, 0.4f, 2f);
                    fireIntensityEvent.Raise(0f);
                    if (badgeManager != null)
                    {
                        badgeManager.UnlockBadge("FuegoApagadoConTrapo");
                    }
                    if (onPlayerSurvivedEvent != null)
                    {
                        onPlayerSurvivedEvent.Raise();
                        Debug.Log("EVENTO DE SUPERVIVENCIA PUBLICADO");
                    }
                }
            }
            else
            {
                Debug.Log($"El trapo '{trapoCollider.name}' está seco. Aumentando fuego.", this);
                if (fireIntensityEvent != null)
                {
                    if (badgeManager != null)
                    {
                        badgeManager.UnlockBadge("FuegoConTrapoSeco"); 
                    }
                    vignetteEvent.Raise(Color.red, 0.5f, 3f);
                    fireIntensityEvent.Raise(0.8f);
                    if (onPlayerDeathEvent != null)
                    {
                        onPlayerDeathEvent.Raise();
                        Debug.Log("EVENTO DE MUERTE PUBLICADO");
                    }
                }
            }
        }
    }
    private void SnapObjectToSarten(Transform objectTransform)
    {
        Rigidbody rbObject = objectTransform.GetComponent<Rigidbody>();
        if (rbObject != null)
        {
            rbObject.isKinematic = true;
            rbObject.linearVelocity = Vector3.zero;
            rbObject.angularVelocity = Vector3.zero;
        }

        objectTransform.SetParent(snapPosition);
        objectTransform.localPosition = Vector3.zero;
        objectTransform.localRotation = Quaternion.identity;

        Collider objectCollider = objectTransform.GetComponent<Collider>();
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }
        Debug.Log($"Objeto '{objectTransform.name}' anclado a la sartén en '{snapPosition.name}'.", this);
    }
    private void HandlePanInWater(Collider waterCollider)
    {
        Debug.Log($"La sartén '{gameObject.name}' ha entrado en el agua: '{waterCollider.name}'. Se llamará a OnPanSubmerged.", this);
        OnPanSubmerged();
    }

    private void OnPanSubmerged()
    {
        if (haSidoSumergida) return; 
        haSidoSumergida = true; 

        Debug.Log($"¡PELIGRO! La sartén con aceite caliente ha entrado en contacto con el agua. ¡Reacción violenta!");

        if (badgeManager != null)
        {
            badgeManager.UnlockBadge("SartenEnAgua");
        }
        if (vignetteEvent != null)
        {
            vignetteEvent.Raise(Color.red, 0.8f, 3f);
        }
        if (onPlayerDeathEvent != null)
        {
            onPlayerDeathEvent.Raise();
            Debug.Log("EVENTO DE MUERTE PUBLICADO por sumergir la sartén en agua.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(waterTag))
        {
            Debug.Log($"La sartén '{gameObject.name}' ha salido del agua: '{other.name}'.", this);
        }
    }
}