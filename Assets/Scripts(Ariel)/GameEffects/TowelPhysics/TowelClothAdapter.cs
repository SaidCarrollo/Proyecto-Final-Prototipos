using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Cloth))]
public class TowelClothAdapter : MonoBehaviour
{
    [Header("Grab System")]
    [SerializeField] private ObjectGrabber grabber;
    [SerializeField] private string requiredTag = "Grab";

    [Header("Bordes (para caída/aplanado)")]
    [SerializeField] private bool autoDetectEdges = true;
    [SerializeField] private float edgeTolerance = 0.0005f;
    [SerializeField] private int[] edgeVertexIndices;

    [Header("PINCH en CRUZ (al agarrar)")]
    public bool pinchOnGrab = true;
    [Range(0f, 0.02f)] public float pinchCenterMax = 0f;
    [Range(0f, 0.1f)] public float pinchCrossMax = 0.04f;
    [Range(0f, 0.6f)] public float pinchRestMax = 0.35f;

    [Header("MaxDistance Presets (otros estados)")]
    [Min(0f)] public float pinnedAll = 0f;
    [Min(0f)] public float freeAll = 0.4f;
    [Min(0f)] public float fallingCenter = 0.3f;
    [Min(0f)] public float fallingEdge = 0.8f;
    [Min(0f)] public float flatCenter = 0.2f;
    [Min(0f)] public float flatEdge = 0.01f;

    [Header("Tuning (móvil)")]
    public float solverHz = 60f;

    [Header("— Idle —")]
    public bool idleUseGravity = false;
    [Range(0, 1)] public float idleDamping = 0.3f;

    [Header("— Grabbed —")]
    public bool grabbedUseGravity = true;
    [Range(0, 1)] public float grabbedStretch = 0.4f;
    [Range(0, 1)] public float grabbedBend = 0.3f;
    [Range(0, 1)] public float grabbedDamping = 0.2f;
    [Range(0, 1)] public float grabbedFriction = 0.3f;

    [Header("— Falling —")]
    public bool fallingUseGravity = true;
    [Range(0, 1)] public float fallingDamping = 0.2f;
    [Range(0, 1)] public float fallingFriction = 0.4f;

    [Header("— Flatten —")]
    public bool flatUseGravity = true;
    [Range(0, 1)] public float flatStretch = 0.6f;
    [Range(0, 1)] public float flatBend = 0.6f;
    [Range(0, 1)] public float flatDamping = 0.5f;
    [Range(0, 1)] public float flatFriction = 0.6f;
    [Range(0, 1)] public float sleepThreshold = 0.4f;

    [Header("— Anchored (drape, NO plano) —")]
    public bool anchoredUseGravity = true;
    [Range(0, 1)] public float anchoredStretch = 0.5f;
    [Range(0, 1)] public float anchoredBend = 0.4f;
    [Range(0, 1)] public float anchoredDamping = 0.35f;
    [Range(0, 1)] public float anchoredFriction = 0.55f;
    [Min(0f)] public float anchoredCenter = 0.35f;  // libertad general
    [Min(0f)] public float anchoredEdge = 0.25f;  // bordes NO clavados (que se note “toalla”)

    [Header("Anchorage (Transform)")]
    public Transform attachmentPivot;
    public bool alignRotationOnAnchor = false;
    public float anchorPosSmoothTime = 0.12f;
    public float anchorRotLerpSpeed = 10f;

    [Header("Anchor Stickiness")]
    public bool anchorSticky = true;
    public float stickEnterRadius = 0.05f;
    public float stickExitRadius = 0.12f;
    public float minStickTime = 0.20f;
    public float detachHoldTime = 0.12f;
    public float settleSleepAfter = 0.20f;

    // ======== WET: material y peso ========
    [Header("— Wet (efecto global de material/peso) —")]
    public bool isWet = false;

    [Tooltip("Multiplica el peso: 1 = normal. 1.6 ~ 2.2 se siente mojado sin exagerar.")]
    public float wetGravityMultiplier = 1.8f;

    [Tooltip("Multiplica rigidez (stretch/bend). <1 = más flácida.")]
    [Range(0.2f, 1.0f)] public float wetStiffnessMul = 0.8f;

    [Tooltip("Suma al damping (más amortiguada).")]
    [Range(0f, 0.5f)] public float wetExtraDamping = 0.15f;

    [Tooltip("Suma a la fricción (se pega un poco más).")]
    [Range(0f, 0.6f)] public float wetExtraFriction = 0.2f;

    [Header("— Wet (perfil de distancia) —")]
    [Tooltip("Si está activo, asegura mínimos de maxDistance húmedos para evitar el ‘cono’.")]
    public bool wetOverrideMaxDistances = true;

    [Tooltip("Mínimo de maxDistance para interior/centro cuando está mojada.")]
    [Range(0f, 1.5f)] public float wetCenterMin = 0.55f;

    [Tooltip("Mínimo de maxDistance para bordes cuando está mojada.")]
    [Range(0f, 1.5f)] public float wetEdgeMin = 0.90f;

    [Tooltip("Si está mojada, desactiva tethers (recomendado para que caigan los bordes).")]
    public bool wetDisableTethers = true;

    [Header("— Wet: caída extra (aditivo sobre presets) —")]
    [Range(0f, 0.6f)] public float wetGrabbedEdgeBonus = 0.20f;
    [Range(0f, 0.6f)] public float wetFallingEdgeBonus = 0.15f;
    [Range(0f, 0.6f)] public float wetAnchoredEdgeBonus = 0.12f;

    private Cloth cloth;
    private SkinnedMeshRenderer smr;
    private MeshFilter mf;
    private TowelAnchorFollower anchorFollower;

    private enum ClothMode { Idle, Grabbed, Falling, Flatten, Anchored }
    private ClothMode currentMode = ClothMode.Idle;

    // PINCH CROSS
    private int centerIndex = -1;
    private int[] crossIndices;
    private int[] restIndices;

    void Awake()
    {
        cloth = GetComponent<Cloth>();
        smr = GetComponent<SkinnedMeshRenderer>();
        mf = GetComponent<MeshFilter>();

        anchorFollower = GetComponent<TowelAnchorFollower>();
        if (anchorFollower == null) anchorFollower = gameObject.AddComponent<TowelAnchorFollower>();
        anchorFollower.OnAutoDetached += HandleAutoDetach;

        if (grabber == null) grabber = FindObjectOfType<ObjectGrabber>();
        if (grabber != null)
        {
            grabber.OnObjectGrabbed += HandleGrabbed;
            grabber.OnObjectReleased += HandleReleased;
        }

        if (autoDetectEdges && (edgeVertexIndices == null || edgeVertexIndices.Length == 0))
            TryDetectEdgeIndices();

        ComputePinchCrossGroups();
    }

    void OnDestroy()
    {
        if (grabber != null)
        {
            grabber.OnObjectGrabbed -= HandleGrabbed;
            grabber.OnObjectReleased -= HandleReleased;
        }
        if (anchorFollower != null)
            anchorFollower.OnAutoDetached -= HandleAutoDetach;
    }

    void Start() => SetIdlePinned();

    // ---------- ESTADOS ----------
    public void SetIdlePinned()
    {
        SetMaxDistanceAll(pinnedAll);
        ApplyCommon(solverHz);
        cloth.useGravity = idleUseGravity;
        cloth.damping = idleDamping;
        currentMode = ClothMode.Idle;
    }

    public void SetGrabbed()
    {
        DetachAnchor();
        if (pinchOnGrab && centerIndex >= 0) SetGrabbedPinchCross();
        else SetGrabbedFree();
    }

    private void SetGrabbedFree()
    {
        SetMaxDistanceAll(freeAll);
        ApplyCommon(solverHz);

        float s = grabbedStretch, b = grabbedBend, d = grabbedDamping, f = grabbedFriction;
        ApplyWetMaterialOverrides(ref s, ref b, ref d, ref f);

        cloth.useGravity = grabbedUseGravity;
        cloth.stretchingStiffness = s;
        cloth.bendingStiffness = b;
        cloth.damping = d;
        cloth.friction = f;

        if (isWet && edgeVertexIndices != null && edgeVertexIndices.Length > 0)
            AddMaxDistanceFor(edgeVertexIndices, wetGrabbedEdgeBonus);

        // Asegurar perfil húmedo anti-cono
        ApplyWetDistanceProfile();

        currentMode = ClothMode.Grabbed;
    }

    private void SetGrabbedPinchCross()
    {
        SetMaxDistanceAll(pinchRestMax);
        SetMaxDistanceFor(new[] { centerIndex }, pinchCenterMax);
        // Si quisieras, también la cruz:
        // if (crossIndices != null && crossIndices.Length > 0) SetMaxDistanceFor(crossIndices, pinchCrossMax);

        ApplyCommon(solverHz);

        float s = grabbedStretch, b = grabbedBend, d = grabbedDamping, f = grabbedFriction;
        ApplyWetMaterialOverrides(ref s, ref b, ref d, ref f);

        cloth.useGravity = grabbedUseGravity;
        cloth.stretchingStiffness = s;
        cloth.bendingStiffness = b;
        cloth.damping = d;
        cloth.friction = f;

        if (isWet && edgeVertexIndices != null && edgeVertexIndices.Length > 0)
            AddMaxDistanceFor(edgeVertexIndices, wetGrabbedEdgeBonus);

        // Perfil húmedo anti-cono pero respetando el centro fijado
        ApplyWetDistanceProfile();

        // Reafirmar centro fijo (por si el perfil subió algo indirecto)
        SetMaxDistanceFor(new[] { centerIndex }, pinchCenterMax);

        currentMode = ClothMode.Grabbed;
    }

    public void SetFallingEdgesLoose()
    {
        DetachAnchor();

        SetMaxDistanceAll(fallingCenter);
        if (edgeVertexIndices != null && edgeVertexIndices.Length > 0)
            SetMaxDistanceFor(edgeVertexIndices, fallingEdge);

        if (isWet && edgeVertexIndices != null && edgeVertexIndices.Length > 0)
            AddMaxDistanceFor(edgeVertexIndices, wetFallingEdgeBonus);

        ApplyCommon(solverHz);

        float s = cloth.stretchingStiffness, b = cloth.bendingStiffness;
        float d = fallingDamping, f = fallingFriction;
        ApplyWetMaterialOverrides(ref s, ref b, ref d, ref f);

        cloth.useGravity = fallingUseGravity;
        cloth.stretchingStiffness = s;
        cloth.bendingStiffness = b;
        cloth.damping = d;
        cloth.friction = f;

        // Anti-cono
        ApplyWetDistanceProfile();

        currentMode = ClothMode.Falling;
    }

    public void SetFlattenOnSurface()
    {
        SetMaxDistanceAll(flatCenter);
        if (edgeVertexIndices != null && edgeVertexIndices.Length > 0)
            SetMaxDistanceFor(edgeVertexIndices, flatEdge);

        ApplyCommon(solverHz);

        float s = flatStretch, b = flatBend, d = flatDamping, f = flatFriction;
        ApplyWetMaterialOverrides(ref s, ref b, ref d, ref f);

        cloth.useGravity = flatUseGravity;
        cloth.stretchingStiffness = s;
        cloth.bendingStiffness = b;
        cloth.damping = d;
        cloth.friction = f;
        cloth.sleepThreshold = sleepThreshold;

        // (en modo plano normalmente no aplicamos perfil húmedo)
        currentMode = ClothMode.Flatten;
    }

    // --- Anclado “drape” (no plano) ---
    public void SetAnchoredDrape()
    {
        SetMaxDistanceAll(anchoredCenter);
        if (edgeVertexIndices != null && edgeVertexIndices.Length > 0)
            SetMaxDistanceFor(edgeVertexIndices, anchoredEdge);

        if (isWet && edgeVertexIndices != null && edgeVertexIndices.Length > 0)
            AddMaxDistanceFor(edgeVertexIndices, wetAnchoredEdgeBonus);

        ApplyCommon(solverHz);

        float s = anchoredStretch, b = anchoredBend, d = anchoredDamping, f = anchoredFriction;
        ApplyWetMaterialOverrides(ref s, ref b, ref d, ref f);

        cloth.useGravity = anchoredUseGravity;
        cloth.stretchingStiffness = s;
        cloth.bendingStiffness = b;
        cloth.damping = d;
        cloth.friction = f;

        // Anti-cono
        ApplyWetDistanceProfile();

        currentMode = ClothMode.Anchored;
    }

    // ---------- Anclaje del TRANSFORM ----------
    public void AttachToAnchor(Transform worldAnchor)
    {
        if (worldAnchor == null) return;

        SetAnchoredDrape();

        // (El follower y su sticky/kinematic están en su script)
        var follower = GetComponent<TowelAnchorFollower>();
        if (follower != null)
        {
            follower.Attach(worldAnchor, attachmentPivot, alignRotationOnAnchor,
                            anchorPosSmoothTime, anchorRotLerpSpeed);
        }
    }

    public void DetachAnchor()
    {
        if (anchorFollower != null && anchorFollower.IsActive)
            anchorFollower.Detach();
    }

    void HandleAutoDetach()
    {
        SetFallingEdgesLoose();
    }

    // ---------- HELPERS ----------
    void ApplyCommon(float hz)
    {
        cloth.clothSolverFrequency = hz;

        TrySetBoolOrFloat(cloth, "enableContinuousCollision", true);
        TrySetBoolOrFloat(cloth, "useContinuousCollision", true);
        TrySetBoolOrFloat(cloth, "useVirtualParticles", true);

        // Tethers dinámicos: desactivarlos mojado ayuda a que los bordes no queden “anclados”
        bool useTethers = !(isWet && wetDisableTethers);
        TrySetBoolOrFloat(cloth, "useTethers", useTethers);

        cloth.worldVelocityScale = 0.5f;
        cloth.worldAccelerationScale = 1f;
        cloth.randomAcceleration = Vector3.zero;

        // Peso extra cuando está mojada (gravedad adicional)
        cloth.externalAcceleration = isWet
            ? Physics.gravity * (Mathf.Max(1f, wetGravityMultiplier) - 1f)
            : Vector3.zero;
    }

    static void TrySetBoolOrFloat(object target, string propName, bool value)
    {
        var p = target?.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
        if (p == null || !p.CanWrite) return;
        if (p.PropertyType == typeof(bool)) p.SetValue(target, value);
        else if (p.PropertyType == typeof(float)) p.SetValue(target, value ? 1f : 0f);
        else if (p.PropertyType == typeof(int)) p.SetValue(target, value ? 1 : 0);
    }

    void SetMaxDistanceAll(float d)
    {
        var c = cloth.coefficients;
        for (int i = 0; i < c.Length; i++) c[i].maxDistance = d;
        cloth.coefficients = c;
    }

    void SetMaxDistanceFor(IEnumerable<int> indices, float d)
    {
        var c = cloth.coefficients;
        foreach (var i in indices) if (i >= 0 && i < c.Length) c[i].maxDistance = d;
        cloth.coefficients = c;
    }

    // ---- Wet helpers ----
    void ApplyWetMaterialOverrides(ref float stretch, ref float bend, ref float damping, ref float friction)
    {
        if (!isWet) return;
        stretch = Mathf.Clamp01(stretch * wetStiffnessMul);
        bend = Mathf.Clamp01(bend * wetStiffnessMul);
        damping = Mathf.Clamp01(damping + wetExtraDamping);
        friction = Mathf.Clamp01(friction + wetExtraFriction);
    }

    // Sube maxDistance de índices dados en delta (sin bajar nada)
    void AddMaxDistanceFor(IEnumerable<int> indices, float delta, float clampMax = 1.5f)
    {
        if (delta <= 0f) return;
        var c = cloth.coefficients;
        foreach (var i in indices)
        {
            if (i < 0 || i >= c.Length) continue;
            c[i].maxDistance = Mathf.Min(clampMax, c[i].maxDistance + delta);
        }
        cloth.coefficients = c;
    }

    // Asegura mínimos húmedos: evita el “cono” dejando más libertad en bordes
    void ApplyWetDistanceProfile()
    {
        if (!isWet || !wetOverrideMaxDistances) return;

        var c = cloth.coefficients;

        // 1) Resto (todo menos centro y cruz): al menos wetCenterMin
        if (restIndices != null)
        {
            foreach (var i in restIndices)
            {
                if (i < 0 || i >= c.Length) continue;
                c[i].maxDistance = Mathf.Max(c[i].maxDistance, wetCenterMin);
            }
        }

        // 2) Bordes: al menos wetEdgeMin
        if (edgeVertexIndices != null && edgeVertexIndices.Length > 0)
        {
            foreach (var i in edgeVertexIndices)
            {
                if (i < 0 || i >= c.Length) continue;
                c[i].maxDistance = Mathf.Max(c[i].maxDistance, wetEdgeMin);
            }
        }

        // (El centro NO se toca, así no rompemos el pinch/fijación)
        cloth.coefficients = c;
    }

    void ReapplyCurrentMode()
    {
        switch (currentMode)
        {
            case ClothMode.Idle: SetIdlePinned(); break;
            case ClothMode.Grabbed: SetGrabbed(); break;
            case ClothMode.Falling: SetFallingEdgesLoose(); break;
            case ClothMode.Flatten: SetFlattenOnSurface(); break;
            case ClothMode.Anchored: SetAnchoredDrape(); break;
        }
    }
    public event System.Action<bool> OnWetChanged;
    public void SetWet(bool wet)
    {
        isWet = wet;
        ReapplyCurrentMode();
        OnWetChanged?.Invoke(isWet); // <-- avisa cambio
    }

    // ---- Eventos del grabber ----
    void HandleGrabbed(GameObject go)
    {
        if (!enabled) return;
        if (!string.IsNullOrEmpty(requiredTag) && !(CompareTag(requiredTag) || go.CompareTag(requiredTag))) return;
        if (!IsThisObject(go)) return;
        SetGrabbed();
    }

    void HandleReleased(GameObject go)
    {
        if (!enabled) return;
        if (!string.IsNullOrEmpty(requiredTag) && !(CompareTag(requiredTag) || go.CompareTag(requiredTag))) return;
        if (!IsThisObject(go)) return;
        SetFallingEdgesLoose();
    }

    // ---- Cruz ortogonal (centro + vecinos) ----
    void ComputePinchCrossGroups()
    {
        var mesh = GetMesh();
        if (mesh == null || mesh.vertexCount == 0) return;

        var vtx = mesh.vertices;
        var bounds = mesh.bounds;

        centerIndex = 0;
        float best = (vtx[0] - bounds.center).sqrMagnitude;
        for (int i = 1; i < vtx.Length; i++)
        {
            float d2 = (vtx[i] - bounds.center).sqrMagnitude;
            if (d2 < best) { best = d2; centerIndex = i; }
        }

        var xs = vtx.Select(p => p.x).Distinct().OrderBy(x => x).ToArray();
        var zs = vtx.Select(p => p.z).Distinct().OrderBy(z => z).ToArray();
        int nx = xs.Length, nz = zs.Length;
        int[,] grid = new int[nx, nz];
        for (int ix = 0; ix < nx; ix++)
            for (int iz = 0; iz < nz; iz++)
                grid[ix, iz] = -1;

        for (int i = 0; i < vtx.Length; i++)
        {
            int ix = ClosestIndex(xs, vtx[i].x);
            int iz = ClosestIndex(zs, vtx[i].z);
            grid[ix, iz] = i;
        }

        int cx = ClosestIndex(xs, vtx[centerIndex].x);
        int cz = ClosestIndex(zs, vtx[centerIndex].z);

        var cross = new List<int>(4);
        if (cx - 1 >= 0 && grid[cx - 1, cz] >= 0) cross.Add(grid[cx - 1, cz]);
        if (cx + 1 < nx && grid[cx + 1, cz] >= 0) cross.Add(grid[cx + 1, cz]);
        if (cz - 1 >= 0 && grid[cx, cz - 1] >= 0) cross.Add(grid[cx, cz - 1]);
        if (cz + 1 < nz && grid[cx, cz + 1] >= 0) cross.Add(grid[cx, cz + 1]);

        cross = cross.Where(i => i >= 0 && i != centerIndex).Distinct().ToList();
        crossIndices = cross.ToArray();

        var rest = new List<int>(vtx.Length);
        for (int i = 0; i < vtx.Length; i++)
        {
            if (i == centerIndex) continue;
            if (crossIndices.Contains(i)) continue;
            rest.Add(i);
        }
        restIndices = rest.ToArray();
    }

    Mesh GetMesh()
    {
        if (smr != null && smr.sharedMesh != null) return smr.sharedMesh;
        if (mf != null && mf.sharedMesh != null) return mf.sharedMesh;
        return null;
    }

    static int ClosestIndex(float[] sorted, float value)
    {
        int idx = 0; float best = Mathf.Abs(sorted[0] - value);
        for (int i = 1; i < sorted.Length; i++)
        {
            float d = Mathf.Abs(sorted[i] - value);
            if (d < best) { best = d; idx = i; }
        }
        return idx;
    }

    void TryDetectEdgeIndices()
    {
        Mesh mesh = GetMesh();
        if (mesh == null) return;

        var vtx = mesh.vertices;
        if (vtx == null || vtx.Length == 0) return;

        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
        float minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;

        for (int i = 0; i < vtx.Length; i++)
        {
            var p = vtx[i];
            if (p.x < minX) minX = p.x; if (p.x > maxX) maxX = p.x;
            if (p.z < minZ) minZ = p.z; if (p.z > maxZ) maxZ = p.z;
        }

        var edges = new List<int>(vtx.Length / 2);
        float tol = Mathf.Max(edgeTolerance, 0.00001f);
        for (int i = 0; i < vtx.Length; i++)
        {
            var p = vtx[i];
            bool isEdge =
                Mathf.Abs(p.x - minX) <= tol || Mathf.Abs(p.x - maxX) <= tol ||
                Mathf.Abs(p.z - minZ) <= tol || Mathf.Abs(p.z - maxZ) <= tol;
            if (isEdge) edges.Add(i);
        }
        edgeVertexIndices = edges.ToArray();
    }

    bool IsThisObject(GameObject go)
    {
        if (go == null) return false;
        return go == gameObject || go.transform.IsChildOf(transform) || transform.IsChildOf(go.transform);
    }

    // Helpers de testeo
    [ContextMenu("TEST/PinAll")] void __Test_PinAll() { SetIdlePinned(); }
    [ContextMenu("TEST/FreeAll")] void __Test_FreeAll() { SetGrabbedFree(); }
    [ContextMenu("TEST/PinchCross")] void __Test_Pinch() { SetGrabbed(); }
}
