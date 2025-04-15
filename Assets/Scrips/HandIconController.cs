using UnityEngine;
using UnityEngine.UI;
public class HandIconController : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite handOpen;
    [SerializeField] private Sprite handClosed;

    [Header("References")]
    [SerializeField] private Image handImage;
    [SerializeField] private ObjectGrabber grabber; // Tu script de agarre

    void Start()
    {
        grabber.OnObjectGrabbed += HandleGrabbed;
        grabber.OnObjectReleased += HandleReleased;
    }

    void OnDestroy()
    {
        grabber.OnObjectGrabbed -= HandleGrabbed;
        grabber.OnObjectReleased -= HandleReleased;
    }

    private void HandleGrabbed(GameObject obj)
    {
        handImage.sprite = handClosed;
    }
    private void HandleReleased(GameObject obj)
    {
        handImage.sprite = handOpen;
    }
}