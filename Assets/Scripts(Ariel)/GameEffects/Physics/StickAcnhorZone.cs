using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StickAnchorZone : MonoBehaviour
{
    [Header("Anchor destino (si lo dejas vacío, usa este transform)")]
    public Transform anchor;

    [Header("Tags permitidos (se comprueban en el collider, sus padres y el root)")]
    public string[] allowedTags = new[] { "Grab", "TapaMetalica" };

    [Header("Alineación")]
    public bool alignRotation = true;

    [Header("Smoothing al adjuntar")]
    public float posSmoothTime = 0.10f;
    public float rotLerpSpeed = 12f;

    [Header("Follow Mode al adjuntar")]
    public AnchorFollower.FollowMode followModeOnAttach = AnchorFollower.FollowMode.Dynamic;

    [Header("Distancias (histeresis)")]
    [Tooltip("Distancia para ENTRAR al estado pegado (mientras está AGARRADO).")]
    public float enterRadius = 0.06f;
    [Tooltip("Distancia para SALIR del estado pegado (tirando mientras está AGARRADO).")]
    public float exitRadius = 0.11f;

    [Header("Tiempos (histeresis)")]
    [Tooltip("Tiempo mínimo que debe estar pegado antes de poder despegar (evita rebotes).")]
    public float minStickTime = 0.2f;
    [Tooltip("Tiempo que hay que 'tirar' (mantenerse fuera de exitRadius) para despegar.")]
    public float detachHoldTime = 0.12f;
    [Tooltip("Tiempo en reposo para que el Rigidbody se 'duerma' (optimización).")]
    public float settleSleepAfter = 0.2f;

    [Header("Salir del anchor tirando")]
    [Tooltip("Se pasa al AnchorFollower si lo soporta (facilita el despegue).")]
    public float exitBoost = 1.2f;

    [Header("Adjuntar dentro del trigger (sin chequear enterRadius)")]
    [Tooltip("Si true, basta con estar dentro del trigger mientras está agarrado para adjuntar.")]
    public bool attachWhenInsideTrigger = true;

    [Header("Opcional: auto detach al salir del trigger")]
    public bool autoDetachOnExit = false;

    // runtime
    private ObjectGrabber grabber;
    private GameObject currentHeld;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;

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
    }

    void HandleGrabbed(GameObject go)
    {
        currentHeld = go;

        // Asegura: al agarrar, que no quede pegado de antes
        var follower = GetFollower(go);
        if (follower != null && follower.IsActive) follower.Detach();
    }

    void HandleReleased(GameObject go)
    {
        // Requisito: al soltar NO debe pegarse; soltamos si estuviera pegado.
        var follower = GetFollower(go);
        if (follower != null && follower.IsActive) follower.Detach();

        if (currentHeld == go) currentHeld = null;
    }

    void OnTriggerStay(Collider other)
    {
        // 1) Debe estar agarrado... (sin cambios)
        if (currentHeld == null) return;

        // 2) Filtrado por tags... (sin cambios)
        if (!HasAnyAllowedTag(other.transform)) return;

        // 3) ¿Es el mismo objeto...? (sin cambios)
        if (!IsSameObject(other.gameObject, currentHeld)) return;

        // 4) Distancia real... (sin cambios)
        Transform dst = (anchor != null ? anchor : transform);
        Vector3 closest = other.ClosestPoint(dst.position);
        float dist = Vector3.Distance(closest, dst.position);

        var follower = GetFollower(currentHeld);
        if (follower == null) follower = currentHeld.AddComponent<AnchorFollower>();

        follower.followMode = followModeOnAttach;
        follower.exitBoostFactor = Mathf.Max(1f, exitBoost);

        // *** CAMBIO AQUÍ: Configurar el follower ***
        // Pasamos nuestros valores al follower para que ÉL gestione el despegue.
        follower.ConfigureStickiness(
            true, // Esta zona siempre es 'sticky'
            enterRadius,
            exitRadius,
            minStickTime,
            detachHoldTime,
            settleSleepAfter
        );

        // 5) Adjuntar solo mientras está agarrado
        if (!follower.IsActive)
        {
            // Usamos 'enterRadius' para la decisión inicial de pegar
            bool shouldAttach = attachWhenInsideTrigger || (dist <= enterRadius);
            if (shouldAttach)
            {
                // Usa el transform del objeto (no pivot opcional global) para evitar offsets raros
                Transform pivot = currentHeld.transform;
                follower.Attach(dst, pivot, alignRotation, posSmoothTime, rotLerpSpeed);

                // Toalla: preset drape (no plana)
                var towel = currentHeld.GetComponentInParent<TowelClothAdapter>();
                if (towel != null) towel.SetAnchoredDrape();
            }
        }
        else
        {
            // 6) *** CAMBIO AQUÍ: Lógica eliminada ***
            // El AnchorFollower (con su lógica 'sticky' y 'detachHoldTime') 
            // se encargará de esto automáticamente.
            /*
            if (dist >= exitRadius)
                follower.Detach();
            */
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!autoDetachOnExit) return;
        if (currentHeld == null) return;
        if (!IsSameObject(other.gameObject, currentHeld)) return;

        var follower = GetFollower(currentHeld);
        if (follower != null && follower.IsActive) follower.Detach();
    }

    // --------- helpers ----------
    AnchorFollower GetFollower(GameObject go)
    {
        if (go == null) return null;
        return go.GetComponentInParent<AnchorFollower>();
    }

    bool HasAnyAllowedTag(Transform t)
    {
        if (allowedTags == null || allowedTags.Length == 0) return true;
        // revisa en el collider, padres y root
        Transform p = t;
        while (p != null)
        {
            for (int i = 0; i < allowedTags.Length; i++)
                if (p.CompareTag(allowedTags[i])) return true;
            p = p.parent;
        }
        // por si el root tiene otro GO con tag
        var root = t.root;
        for (int i = 0; i < allowedTags.Length; i++)
            if (root.CompareTag(allowedTags[i])) return true;

        return false;
    }

    bool IsSameObject(GameObject a, GameObject b)
    {
        if (a == null || b == null) return false;
        if (a == b) return true;
        return a.transform.IsChildOf(b.transform) || b.transform.IsChildOf(a.transform);
    }
}