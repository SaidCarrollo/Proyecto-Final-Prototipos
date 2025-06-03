using UnityEngine;

public class SartenCollider : MonoBehaviour
{
    [Header("Configuraci�n")]
    [SerializeField] private Transform snapPosition; // Objeto vac�o que marca la posici�n de snap
    [SerializeField] private string trapoTag = "TrapoMojado";
    [SerializeField] private FloatEvent fireIntensityEvent;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(trapoTag))
        {
            GrabbableObject trapo = other.GetComponent<GrabbableObject>();
            ObjectGrabber grabber = other.GetComponentInParent<ObjectGrabber>();

            // Verificar que sea un trapo mojado y no est� siendo agarrado
            if (trapo != null && trapo.EstaMojado && (grabber == null || !grabber.IsHoldingObject()))
            {
                SnapTrapoToSarten(other.transform);
                // DISPARAR EL EVENTO PARA APAGAR EL FUEGO
                if (fireIntensityEvent != null)
                {
                    // Enviamos 0 para apagar completamente el fuego
                    fireIntensityEvent.Raise(0f);
                }
            }
        }
    }

    private void SnapTrapoToSarten(Transform trapoTransform)
    {
        // Desactivar f�sica temporalmente
        Rigidbody rb = trapoTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Parentear y posicionar el trapo
        trapoTransform.SetParent(snapPosition);
        trapoTransform.localPosition = Vector3.zero;
        trapoTransform.localRotation = Quaternion.identity;

        // Opcional: Desactivar collider para evitar m�ltiples snaps
        Collider trapoCollider = trapoTransform.GetComponent<Collider>();
        if (trapoCollider != null)
        {
            trapoCollider.enabled = false;
        }
    }
}