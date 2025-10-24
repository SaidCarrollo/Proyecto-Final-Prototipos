using UnityEngine;

[DisallowMultipleComponent]
public class GrabbableNameplate : MonoBehaviour
{
    [Header("Mostrar")]
    [Tooltip("Texto a mostrar. Si está vacío, usa el nombre del GameObject.")]
    public string displayName = "";

    [Header("Anchor de mundo")]
    [Tooltip("Coloca aquí un hijo (Empty) donde quieres que aparezca el nombre.")]
    public Transform nameplateAnchor;

    [Tooltip("Offset extra mundial (suma al anchor).")]
    public Vector3 worldOffset = new Vector3(0, 0.02f, 0);

    [Header("Apariencia")]
    [Range(0f, 1f)] public float normalAlpha = 1f;
    [Range(0f, 1f)] public float grabbedAlpha = 0.35f;
    [Tooltip("Segundos para el fade de alpha.")]
    public float fadeSeconds = 0.15f;

    [Header("Filtro")]
    [Tooltip("Debe coincidir con tu sistema (objetos 'grab' usan este tag).")]
    public string requiredTag = "Grab";

    private object handle;
    private ObjectGrabber grabber;

    void Awake()
    {
        if (string.IsNullOrEmpty(displayName))
            displayName = gameObject.name;

        // Si no te dan anchor, intentamos un fallback sensato: top del render
        if (nameplateAnchor == null)
        {
            // crea un anchor temporal como hijo
            GameObject tmp = new GameObject("NameplateAnchor_Auto");
            tmp.transform.SetParent(transform, false);
            tmp.transform.localPosition = ComputeTopLocal() + worldOffset;
            nameplateAnchor = tmp.transform;
            worldOffset = Vector3.zero; // ya lo llevamos al anchor
        }

        handle = NameplateSystem.Instance?.Register(
            target: transform,
            anchor: nameplateAnchor,
            worldOffset: worldOffset,
            displayText: displayName,
            normalAlpha: normalAlpha,
            grabbedAlpha: grabbedAlpha,
            fadeSeconds: fadeSeconds
        );

        // Suscribirse a eventos de tu sistema de agarre
        grabber = FindObjectOfType<ObjectGrabber>();
        if (grabber != null)
        {
            grabber.OnObjectGrabbed += HandleGrabbed;
            grabber.OnObjectReleased += HandleReleased;
        }
    }

    void OnDestroy()
    {
        if (grabber != null)
        {
            grabber.OnObjectGrabbed -= HandleGrabbed;
            grabber.OnObjectReleased -= HandleReleased;
        }
        NameplateSystem.Instance?.Unregister(handle);
    }

    void HandleGrabbed(GameObject go)
    {
        if (!IsThis(go)) return;
        NameplateSystem.Instance?.SetGrabbed(handle, true);
    }

    void HandleReleased(GameObject go)
    {
        if (!IsThis(go)) return;
        NameplateSystem.Instance?.SetGrabbed(handle, false);
    }

    bool IsThis(GameObject go)
    {
        if (go == null) return false;
        if (!string.IsNullOrEmpty(requiredTag) && !(CompareTag(requiredTag) || go.CompareTag(requiredTag)))
            return false;

        return go == gameObject || go.transform.IsChildOf(transform) || transform.IsChildOf(go.transform);
    }

    // Calcula un local offset hacia el "top" del bounds renderizado (fallback)
    Vector3 ComputeTopLocal()
    {
        var rends = GetComponentsInChildren<Renderer>();
        if (rends == null || rends.Length == 0) return new Vector3(0, 0.3f, 0);

        Bounds b = rends[0].bounds;
        foreach (var r in rends) b.Encapsulate(r.bounds);

        // Convertir top world a local
        Vector3 worldTop = new Vector3(b.center.x, b.max.y, b.center.z);
        return transform.InverseTransformPoint(worldTop) - Vector3.zero; // local pos desde pivot
    }

    // Por si ajustas el anchor en runtime y quieres actualizarlo
    [ContextMenu("Nameplate/Set current as anchor")]
    void UseCurrentAsAnchor()
    {
        if (nameplateAnchor == null) return;
        NameplateSystem.Instance?.SetAnchor(handle, nameplateAnchor);
    }

    [ContextMenu("Nameplate/Set world offset")]
    void SetWorldOffset()
    {
        NameplateSystem.Instance?.SetOffset(handle, worldOffset);
    }
}
