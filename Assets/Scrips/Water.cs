using UnityEngine;

public class WaterTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grab"))
        {
            GrabbableObject grabbable = other.GetComponent<GrabbableObject>();
            if (grabbable != null)
            {
                grabbable.SetWet(true);
            }
        }
    }
}