using UnityEngine;

[DisallowMultipleComponent]
public class SortingOrderSetter : MonoBehaviour
{
    [Header("Sorting Order deseado")]
    public int sortingOrder = 0;

    [Tooltip("Si es UI, fuerza overrideSorting en el Canvas.")]
    public bool isUI = false;

    void Awake()
    {
        ApplySortingOrder();
    }

    /// <summary>
    /// Aplica el sortingOrder al objeto según si es UI o no.
    /// </summary>
    public void ApplySortingOrder()
    {
        if (isUI)
        {
            // Caso UI: Canvas
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }
        else
        {
            // Caso no UI: SpriteRenderer o cualquier Renderer
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = sortingOrder;
                return;
            }

            Renderer genericRenderer = GetComponent<Renderer>();
            if (genericRenderer != null)
            {
                genericRenderer.sortingOrder = sortingOrder;
                return;
            }

            Debug.LogWarning($"[SortingOrderSetter] No se encontró Renderer o SpriteRenderer en {name}.");
        }
    }

    // ---------- MÉTODOS ESTÁTICOS ÚTILES ----------

    /// <summary>
    /// Coloca el objeto en un sortingOrder específico (auto detecta UI / Renderer).
    /// </summary>
    public static void SetSortingOrder(GameObject obj, int order, bool isUI = false)
    {
        if (obj == null) return;

        SortingOrderSetter setter = obj.GetComponent<SortingOrderSetter>();
        if (setter == null)
        {
            setter = obj.AddComponent<SortingOrderSetter>();
        }

        setter.sortingOrder = order;
        setter.isUI = isUI;
        setter.ApplySortingOrder();
    }

    /// <summary>
    /// Versión con Transform.
    /// </summary>
    public static void SetSortingOrder(Transform t, int order, bool isUI = false)
    {
        if (t == null) return;
        SetSortingOrder(t.gameObject, order, isUI);
    }
}
