using UnityEngine;

public class WaterTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grab"))
        {
            ObjectGrabber grabbable = other.GetComponent<ObjectGrabber>();
            if (grabbable != null)
            {
                grabbable.SetWet(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Grab"))
        {
            ObjectGrabber grabbable = other.GetComponent<ObjectGrabber>();
            if (grabbable != null)
            {
                grabbable.SetWet(false);
            }
        }
    }
}
