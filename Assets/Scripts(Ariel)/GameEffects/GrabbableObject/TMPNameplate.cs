using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class TMPNameplate : MonoBehaviour
{
    public enum BillboardMode { Full3D, YawOnly, Smart }

    [Header("Refs")]
    public Transform anchor;   // punto exacto donde va el rótulo
    public TMP_Text text;      // TMP (3D) del rótulo

    [Header("Contenido")]
    public string baseDisplayName = "";

    [Header("Billboard")]
    public BillboardMode billboard = BillboardMode.Smart;
    [Tooltip("En modo Smart, si la cámara supera este pitch (°) se pasa a Full3D.")]
    public float smartPitchThreshold = 20f;
    [Tooltip("Invierte el frente si lo ves al revés (180°).")]
    public bool invertFacing = false;
    [Tooltip("Desfase fino en Y si tu prefab está girado.")]
    public float yawOffsetDeg = 0f;

    [Header("Visibilidad")]
    public bool forceOverlayMaterial = true;

    [Header("Fade al agarrar")]
    [Range(0f, 1f)] public float normalAlpha = 1f;
    [Range(0f, 1f)] public float grabbedAlpha = 0.35f;
    public float fadeSeconds = 0.15f;

    [Header("Transición de texto (mojado)")]
    public float textTransitionSeconds = 0.25f;
    public string wetSuffix = " (Húmeda)";

    [Header("Filtro de tu sistema")]
    public string requiredTag = "Grab";

    // ---- priv ----
    private ObjectGrabber grabber;
    private TowelClothAdapter towel;
    private Camera cam;

    private float currentAlpha, targetAlpha, contentAlpha = 1f;
    private bool textTransitionRunning;
    private Material runtimeMat;
    private bool lastWet;
    private string currentBaseName;

    void Awake()
    {
        if (anchor == null) anchor = transform;
        if (text == null) text = GetComponentInChildren<TMP_Text>(true);

        if (string.IsNullOrEmpty(baseDisplayName))
            baseDisplayName = anchor.root.name;
        currentBaseName = baseDisplayName;

        // Detectar toalla / mojado (evento)
        towel = anchor.GetComponentInParent<TowelClothAdapter>();
        if (towel != null)
        {
            lastWet = towel.isWet;
            if (text != null) text.text = BuildDisplayName(lastWet);
            towel.OnWetChanged += HandleWetChanged;
        }
        else
        {
            lastWet = false;
            if (text != null) text.text = baseDisplayName;
        }

        // Overlay / orden de render para ver a través
        if (text != null && forceOverlayMaterial)
        {
            var overlayShader = Shader.Find("TextMeshPro/Distance Field Overlay");
            if (overlayShader != null)
            {
                runtimeMat = new Material(overlayShader);
                runtimeMat.CopyPropertiesFromMaterial(text.fontMaterial);
                text.fontMaterial = runtimeMat;
            }
            var rend = text.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.sortingOrder = 5000;
                var mats = rend.sharedMaterials;
                if (mats != null)
                    for (int i = 0; i < mats.Length; i++)
                        if (mats[i] != null) mats[i].renderQueue = 4000;
            }
        }

        // ✅ Arrancar visible SIEMPRE
        currentAlpha = normalAlpha;
        targetAlpha = normalAlpha;
        ApplyAlphaInstant(normalAlpha);

        cam = Camera.main;

        // Eventos de agarre
        grabber = FindObjectOfType<ObjectGrabber>();
        if (grabber != null)
        {
            grabber.OnObjectGrabbed += OnGrabbed;
            grabber.OnObjectReleased += OnReleased;
        }
    }

    void OnDestroy()
    {
        if (grabber != null)
        {
            grabber.OnObjectGrabbed -= OnGrabbed;
            grabber.OnObjectReleased -= OnReleased;
        }
        if (towel != null) towel.OnWetChanged -= HandleWetChanged;
        if (runtimeMat != null) Destroy(runtimeMat);
    }

    void LateUpdate()
    {
        if (text == null || anchor == null) return;

        transform.position = anchor.position;

        if (cam == null) cam = Camera.main;
        if (cam != null)
        {
            Vector3 toCam = cam.transform.position - transform.position;
            Vector3 toCamXZ = toCam; toCamXZ.y = 0f;

            Quaternion targetRot;

            switch (billboard)
            {
                case BillboardMode.Full3D:
                    {
                        Vector3 fwd = invertFacing ? cam.transform.forward : -cam.transform.forward;
                        Vector3 up = cam.transform.up;
                        targetRot = Quaternion.LookRotation(fwd, up);
                        break;
                    }
                case BillboardMode.YawOnly:
                    {
                        if (toCamXZ.sqrMagnitude < 1e-6f) toCamXZ = transform.forward;
                        Vector3 dir = invertFacing ? -toCamXZ : toCamXZ;
                        targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                        break;
                    }
                case BillboardMode.Smart:
                default:
                    {
                        float pitchDeg = Vector3.Angle(toCam.normalized, toCamXZ.normalized);
                        bool steep = pitchDeg > Mathf.Abs(smartPitchThreshold);

                        if (steep)
                        {
                            Vector3 fwd = invertFacing ? cam.transform.forward : -cam.transform.forward;
                            Vector3 up = cam.transform.up;
                            targetRot = Quaternion.LookRotation(fwd, up);
                        }
                        else
                        {
                            if (toCamXZ.sqrMagnitude < 1e-6f) toCamXZ = transform.forward;
                            Vector3 dir = invertFacing ? -toCamXZ : toCamXZ;
                            targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                        }
                        break;
                    }
            }

            if (Mathf.Abs(yawOffsetDeg) > 0.001f)
                targetRot *= Quaternion.Euler(0f, yawOffsetDeg, 0f);

            transform.rotation = targetRot;
        }

        // Fade agarrar/soltar
        float t = (fadeSeconds <= 0.0001f) ? 1f : (Time.unscaledDeltaTime / fadeSeconds);
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Mathf.Clamp01(t));
        ApplyAlpha(currentAlpha * contentAlpha);

        // Fallback por si no llegó el evento del adapter
        if (towel != null && towel.isWet != lastWet)
        {
            lastWet = towel.isWet;
            HandleWetChanged(lastWet);
        }
    }

    // --- eventos ---
    void OnGrabbed(GameObject go)
    {
        if (!IsThis(go)) return;
        targetAlpha = grabbedAlpha;
    }

    void OnReleased(GameObject go)
    {
        if (!IsThis(go)) return;
        targetAlpha = normalAlpha;
    }

    void HandleWetChanged(bool wet)
    {
        string next = BuildDisplayName(wet);
        if (textTransitionRunning)
        {
            StopAllCoroutines();
            textTransitionRunning = false;
            contentAlpha = 1f;
        }
        StartCoroutine(Co_TextTransition(next, textTransitionSeconds));
    }

    // --- helpers ---
    string BuildDisplayName(bool wet)
    {
        if (!wet) return baseDisplayName;
        string suffix = string.IsNullOrEmpty(wetSuffix) ? " (Húmeda)" : wetSuffix;
        return baseDisplayName + suffix;
    }

    IEnumerator Co_TextTransition(string newText, float seconds)
    {
        textTransitionRunning = true;
        float half = Mathf.Max(0.01f, seconds * 0.5f);
        float t = 0f;

        // Fade out contenido (sin tocar alpha de agarre)
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            contentAlpha = 1f - Mathf.Clamp01(t / half);
            ApplyAlpha(currentAlpha * contentAlpha);
            yield return null;
        }
        contentAlpha = 0f;
        ApplyAlpha(currentAlpha * contentAlpha);

        text.text = newText;

        // Fade in
        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            contentAlpha = Mathf.Clamp01(t / half);
            ApplyAlpha(currentAlpha * contentAlpha);
            yield return null;
        }
        contentAlpha = 1f;
        ApplyAlpha(currentAlpha * contentAlpha);

        textTransitionRunning = false;
    }

    bool IsThis(GameObject go)
    {
        if (go == null) return false;
        if (!string.IsNullOrEmpty(requiredTag) && !go.CompareTag(requiredTag))
            return false;
        return go == anchor.gameObject || go.transform.IsChildOf(anchor) || anchor.IsChildOf(go.transform);
    }

    void ApplyAlpha(float a)
    {
        if (text == null) return;
        var c = text.color;
        c.a = Mathf.Clamp01(a);
        text.color = c;
    }
    void ApplyAlphaInstant(float a)
    {
        currentAlpha = targetAlpha = Mathf.Clamp01(a);
        contentAlpha = 1f;
        ApplyAlpha(currentAlpha * contentAlpha);
    }
}
