using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class PreserveAspectWithAnchors : MonoBehaviour
{
    public enum FitMode
    {
        FitInside,   // Encaja completamente dentro del área de anchors (sin recortar)
        Fill         // Rellena todo el área de anchors (puede recortar)
    }

    [Header("Ajustes")]
    [SerializeField] private FitMode fitMode = FitMode.FitInside;
    [SerializeField] private bool useGraphicAspect = true;   // Usa sprite/texture si existe
    [SerializeField] private float manualAspect = 1f;        // width / height

    private RectTransform _rect;
    private RectTransform _parentRect;

    private void Awake()
    {
        CacheRects();
        CacheAspect();
        Apply();
    }

    private void OnEnable()
    {
        CacheRects();
        CacheAspect();
        Apply();
    }

    private void OnValidate()
    {
        CacheRects();
        CacheAspect();
        Apply();
    }

    private void CacheRects()
    {
        if (_rect == null)
            _rect = transform as RectTransform;

        if (_rect != null && _rect.parent is RectTransform parent)
            _parentRect = parent;
    }

    private void CacheAspect()
    {
        if (_rect == null)
            return;

        if (!useGraphicAspect)
        {
            // Usar el aspect manual tal cual
            return;
        }

        // 1) Si tiene Image con sprite
        var img = GetComponent<Image>();
        if (img != null && img.sprite != null)
        {
            var r = img.sprite.rect;
            if (r.height > 0)
                manualAspect = r.width / r.height;
            return;
        }

        // 2) Si tiene RawImage con texture
        var raw = GetComponent<RawImage>();
        if (raw != null && raw.texture != null)
        {
            if (raw.texture.height > 0)
                manualAspect = (float)raw.texture.width / raw.texture.height;
            return;
        }

        // 3) Si no hay gráfico, usa el rect actual
        var rect = _rect.rect;
        if (rect.height > 0)
            manualAspect = rect.width / rect.height;
    }

    private void OnRectTransformDimensionsChange()
    {
        // Se llama cuando cambian resoluciones, anchors, etc.
        Apply();
    }

    private void Apply()
    {
        if (_rect == null || _parentRect == null || manualAspect <= 0f)
            return;

        Rect parentRect = _parentRect.rect;

        Vector2 anchorMin = _rect.anchorMin;
        Vector2 anchorMax = _rect.anchorMax;

        // Tamaño del área definida por los anchors (en el espacio del padre)
        float areaWidth = parentRect.width * (anchorMax.x - anchorMin.x);
        float areaHeight = parentRect.height * (anchorMax.y - anchorMin.y);

        if (areaWidth <= 0f || areaHeight <= 0f)
            return;

        float targetWidth;
        float targetHeight;

        if (fitMode == FitMode.FitInside)
        {
            // Encaja completamente, puede quedar “letterbox”
            targetWidth = areaWidth;
            targetHeight = targetWidth / manualAspect;

            if (targetHeight > areaHeight)
            {
                targetHeight = areaHeight;
                targetWidth = targetHeight * manualAspect;
            }
        }
        else // Fill
        {
            // Rellena todo el área (puede recortar)
            targetHeight = areaHeight;
            targetWidth = targetHeight * manualAspect;

            if (targetWidth < areaWidth)
            {
                targetWidth = areaWidth;
                targetHeight = targetWidth / manualAspect;
            }
        }

        // Ajusta el tamaño respetando los anchors actuales
        _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);

        // Centrar dentro del área de anchors
        _rect.anchoredPosition = Vector2.zero;
    }
}
