using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class HandIconController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float focusedScale = 1.2f; 
    [SerializeField] private float animationDuration = 0.2f; 

    [Header("References")]
    [SerializeField] private Image cursorImage;
    [SerializeField] private ObjectGrabber grabber;

    private Vector3 initialScale;
    private Tweener scaleTween;

    void Start()
    {
        initialScale = cursorImage.transform.localScale;

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
        scaleTween?.Kill();
        scaleTween = cursorImage.transform.DOScale(initialScale * focusedScale, animationDuration)
                                           .SetEase(Ease.OutBack);
    }

    private void HandleReleased(GameObject obj)
    {
        scaleTween?.Kill();

        scaleTween = cursorImage.transform.DOScale(initialScale, animationDuration);
    }
}