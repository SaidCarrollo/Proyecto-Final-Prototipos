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

    void Update()
    {
        // Cambia el sprite basado en si se está agarrando un objeto
        if (grabber.IsHoldingObject()) // Asegúrate de tener este método en ObjectGrabber.cs
        {
            handImage.sprite = handClosed;
        }
        else
        {
            handImage.sprite = handOpen;
        }
    }
}