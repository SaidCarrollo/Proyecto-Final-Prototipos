using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class FixScrollViewLayout : MonoBehaviour
{
    public RectTransform viewport;
    public RectTransform content;
    public Scrollbar verticalScrollbar; // opcional, solo si hay

    void Awake()
    {
        if (!viewport) viewport = GetComponent<ScrollRect>().viewport;
        if (!content) content = GetComponent<ScrollRect>().content;
        if (!verticalScrollbar) verticalScrollbar = GetComponent<ScrollRect>().verticalScrollbar;
    }

    void Start()
    {
        Realign();
    }

    void OnRectTransformDimensionsChange()
    {
        // Reajusta si cambia la resolución o orientación
        Realign();
    }

    void Realign()
    {
        if (!viewport || !content) return;

        // 1) Recortar viewport por ancho de scrollbar
        float sbWidth = 0f;
        if (verticalScrollbar && verticalScrollbar.gameObject.activeInHierarchy)
        {
            var sbRT = verticalScrollbar.GetComponent<RectTransform>();
            if (sbRT) sbWidth = sbRT.rect.width;
        }

        // Anchors del viewport deben ser stretch a izquierda/derecha
        viewport.anchorMin = new Vector2(0, 0);
        viewport.anchorMax = new Vector2(1, 1);
        viewport.pivot = new Vector2(0, 1);
        viewport.offsetMin = new Vector2(0, viewport.offsetMin.y);       // left = 0
        viewport.offsetMax = new Vector2(-sbWidth, viewport.offsetMax.y); // right = -scrollbar

        // 2) Content top-stretch y alineado al origen del viewport
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0, 1);
        content.offsetMin = new Vector2(0, content.offsetMin.y); // left = 0
        content.offsetMax = new Vector2(0, content.offsetMax.y); // right = 0
        content.anchoredPosition = new Vector2(0, content.anchoredPosition.y);

        // 3) Forzar recomposición de layout para aplicar cambios
        LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }
}
