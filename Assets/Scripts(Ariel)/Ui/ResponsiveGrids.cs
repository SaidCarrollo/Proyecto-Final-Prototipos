// Assets/Scripts/UI/ResponsiveGrid.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
[DisallowMultipleComponent]
public class ResponsiveGrid : UIBehaviour
{
    public enum FitMode { FixedColumns, FixedRows, FitBoth }
    public FitMode fitMode = FitMode.FixedColumns;

    [Min(1)] public int columns = 3; // usado si FixedColumns
    [Min(1)] public int rows = 2;    // usado si FixedRows
    public bool keepSquare = true;   // mantiene celda cuadrada (recomendado para cards/iconos)

    // Margen extra opcional además del padding del Grid
    public Vector2 extraMargins = Vector2.zero;

    GridLayoutGroup grid;
    RectTransform rect;

    protected override void Awake() { base.Awake(); Cache(); Calculate(); }
    protected override void OnEnable() { base.OnEnable(); Calculate(); }
    void OnTransformChildrenChanged() { Calculate(); }
    protected override void OnRectTransformDimensionsChange() { Calculate(); }
    void OnValidate() { Cache(); Calculate(); }

    void Cache()
    {
        if (!grid) grid = GetComponent<GridLayoutGroup>();
        if (!rect) rect = transform as RectTransform;
    }

    void Calculate()
    {
        if (!grid || !rect) return;

        var p = grid.padding;
        float width = rect.rect.width - p.left - p.right - extraMargins.x;
        float height = rect.rect.height - p.top - p.bottom - extraMargins.y;

        int cols = Mathf.Max(1, columns);
        int rws = Mathf.Max(1, rows);

        if (fitMode == FitMode.FitBoth)
        {
            int n = Mathf.Max(1, transform.childCount);
            cols = Mathf.CeilToInt(Mathf.Sqrt(n));
            rws = Mathf.CeilToInt((float)n / cols);
        }
        else if (fitMode == FitMode.FixedRows)
        {
            int n = Mathf.Max(1, transform.childCount);
            rws = Mathf.Max(1, rows);
            cols = Mathf.CeilToInt((float)n / rws);
        }
        // si FixedColumns, usamos 'columns' y derivamos filas visualmente; no es necesario contarlas

        float cellW = (width - grid.spacing.x * (cols - 1)) / cols;
        float cellH = (height - grid.spacing.y * (rws - 1)) / rws;

        if (keepSquare)
        {
            float s = Mathf.Floor(Mathf.Min(cellW, cellH));
            cellW = cellH = s;
        }

        grid.cellSize = new Vector2(Mathf.Max(0f, cellW), Mathf.Max(0f, cellH));

        // Rebuild para aplicar en editor y en runtime inmediatamente
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }
}
