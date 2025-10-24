using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameplateSystem : MonoBehaviour
{
    public static NameplateSystem Instance { get; private set; }

    [Header("Canvas")]
    public Canvas rootCanvas;                // Se crea en runtime si no lo asignas
    public CanvasScaler canvasScaler;        // Opcional
    public GraphicRaycaster raycaster;       // Opcional

    [Header("Estilos por defecto")]
    public Font defaultFont;
    public int fontSize = 18;
    public Color normalColor = Color.white;  // Color del texto
    public Color outlineColor = new Color(0, 0, 0, 0.8f);

    private readonly List<Entry> entries = new();

    private class Entry
    {
        public Transform target;       // objeto dueño (para vida útil / grabbed)
        public Transform anchor;       // punto de anclaje para mostrar el texto
        public Vector3 worldOffset;    // offset extra opcional
        public RectTransform rect;
        public Text text;
        public Outline outline;
        public CanvasGroup cg;
        public float normalAlpha;
        public float grabbedAlpha;
        public float fadeSeconds;
        public bool grabbed;
        public float targetAlpha;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (rootCanvas == null)
        {
            var go = new GameObject("NameplateCanvas");
            rootCanvas = go.AddComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = 5000; // por encima

            canvasScaler = go.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            raycaster = go.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(go);
        }
    }

    void LateUpdate()
    {
        if (entries.Count == 0) return;

        var cam = Camera.main;
        var canvasRect = rootCanvas.transform as RectTransform;

        for (int i = entries.Count - 1; i >= 0; i--)
        {
            var e = entries[i];
            if (e.target == null || e.rect == null)
            {
                if (e.rect != null) Destroy(e.rect.gameObject);
                entries.RemoveAt(i);
                continue;
            }

            // Posición de mundo: preferimos el anchor si existe
            Vector3 worldPos;
            if (e.anchor != null)
                worldPos = e.anchor.position + e.worldOffset;
            else
                worldPos = e.target.position + e.worldOffset;

            // Cámara debe existir para proyección (si no, ocultamos)
            if (cam == null) { e.cg.alpha = 0f; continue; }

            // Si está detrás de cámara, ocultar
            var sp = cam.WorldToScreenPoint(worldPos);
            if (sp.z <= 0f) { e.cg.alpha = 0f; continue; }

            // Convertir a coords locales del canvas (funciona para Overlay y Camera)
            Vector2 localPoint;
            Camera eventCam = (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : rootCanvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, sp, eventCam, out localPoint))
            {
                e.rect.anchoredPosition = localPoint;
            }

            // Fade hacia alpha objetivo
            e.targetAlpha = e.grabbed ? e.grabbedAlpha : e.normalAlpha;
            float t = (e.fadeSeconds <= 0.0001f) ? 1f : (Time.unscaledDeltaTime / e.fadeSeconds);
            e.cg.alpha = Mathf.Lerp(e.cg.alpha, e.targetAlpha, Mathf.Clamp01(t));
        }
    }

    // --- Registro con anchor explícito ---
    public object Register(Transform target, Transform anchor, Vector3 worldOffset,
                           string displayText, float normalAlpha, float grabbedAlpha, float fadeSeconds)
    {
        if (target == null || rootCanvas == null) return null;

        // Crear UI item
        GameObject go = new GameObject($"NP_{displayText}");
        go.transform.SetParent(rootCanvas.transform, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0f); // base del texto en el punto

        var text = go.AddComponent<Text>();
        text.text = displayText;
        text.font = defaultFont ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = normalColor;
        text.alignment = TextAnchor.LowerCenter;
        text.raycastTarget = false;

        var outline = go.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        var cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0f; // aparecerá con fade

        var entry = new Entry
        {
            target = target,
            anchor = anchor,
            worldOffset = worldOffset,
            rect = rect,
            text = text,
            outline = outline,
            cg = cg,
            normalAlpha = Mathf.Clamp01(normalAlpha),
            grabbedAlpha = Mathf.Clamp01(grabbedAlpha),
            fadeSeconds = Mathf.Max(0f, fadeSeconds),
            grabbed = false,
            targetAlpha = normalAlpha
        };

        entries.Add(entry);
        return entry; // handle
    }

    // --- Registro sin anchor (offset desde target) ---
    public object Register(Transform target, Vector3 worldOffset,
                           string displayText, float normalAlpha, float grabbedAlpha, float fadeSeconds)
    {
        return Register(target, null, worldOffset, displayText, normalAlpha, grabbedAlpha, fadeSeconds);
    }

    public void Unregister(object handle)
    {
        var idx = entries.FindIndex(e => (object)e == handle);
        if (idx >= 0)
        {
            var e = entries[idx];
            if (e.rect != null) Destroy(e.rect.gameObject);
            entries.RemoveAt(idx);
        }
    }

    public void SetText(object handle, string t)
    {
        var e = Find(handle);
        if (e != null && e.text != null) e.text.text = t;
    }

    public void SetGrabbed(object handle, bool grabbed)
    {
        var e = Find(handle);
        if (e != null) e.grabbed = grabbed;
    }

    public void SetOffset(object handle, Vector3 worldOffset)
    {
        var e = Find(handle);
        if (e != null) e.worldOffset = worldOffset;
    }

    public void SetAnchor(object handle, Transform anchor)
    {
        var e = Find(handle);
        if (e != null) e.anchor = anchor;
    }

    private Entry Find(object handle)
    {
        foreach (var e in entries) if ((object)e == handle) return e;
        return null;
    }
}
