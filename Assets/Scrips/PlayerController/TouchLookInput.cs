using UnityEngine;
using UnityEngine.EventSystems;

public class TouchLookInput : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public Vector2 LookDelta { get; private set; }

    private bool isDragging = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        LookDelta = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        LookDelta = eventData.delta;
    }
}
