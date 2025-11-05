using UnityEngine;
using UnityEngine.VFX;

public class FireVFXGrowthController : MonoBehaviour
{
    [SerializeField] private VisualEffect vfx;

    // nombres del VFX
    private const string MAIN_RATE = "Fire_SpawnRate_Main";
    private const string SECONDARY_RATE = "Fire_SpawnRate_Secondary";
    private const string GLOBAL_SCALE = "Fire_GlobalScale";
    private const string Y_OFFSET = "Fire_YOffset";   // si lo sigues usando en el VFX

    [Header("VFX Ranges")]
    public float maxMainSpawnRate = 25f;
    public float maxSecondarySpawnRate = 10f;
    public float minScale = 1f;
    public float maxScale = 3f;

    [Header("Pivot compensation (transform)")]
    [Tooltip("Cuánto debe subir el objeto por cada punto de escala extra")]
    public float pivotCompensationFactor = 0.5f;

    // guardamos la posición original del objeto
    private Vector3 _originalLocalPos;

    private void Awake()
    {
        if (vfx == null)
            vfx = GetComponent<VisualEffect>();

        _originalLocalPos = transform.localPosition;
    }

    public void SetFireLevel(float value)
    {
        if (vfx == null) return;

        // tu FireTimer manda 10 → lo normalizamos a 0..1
        float t = Mathf.Clamp01(value / 10f);

        // 1) spawn
        if (vfx.HasFloat(MAIN_RATE))
            vfx.SetFloat(MAIN_RATE, Mathf.Lerp(0f, maxMainSpawnRate, t));

        if (vfx.HasFloat(SECONDARY_RATE))
            vfx.SetFloat(SECONDARY_RATE, Mathf.Lerp(0f, maxSecondarySpawnRate, t));

        // 2) scale (en el VFX)
        float scale = Mathf.Lerp(minScale, maxScale, t);
        if (vfx.HasFloat(GLOBAL_SCALE))
            vfx.SetFloat(GLOBAL_SCALE, scale);

        // 3) opcional: mandarlo también al VFX para Initialize
        if (vfx.HasFloat(Y_OFFSET))
        {
            float vfxOffset = Mathf.Max(0f, (scale - 1f) * pivotCompensationFactor);
            vfx.SetFloat(Y_OFFSET, vfxOffset);
        }

        // 4) **compensar el transform** para que la base no se hunda
        // si scale = 1 → offset = 0
        // si scale = 2 → sube un poco
        float transformOffset = Mathf.Max(0f, (scale - 1f) * pivotCompensationFactor);
        transform.localPosition = _originalLocalPos + Vector3.up * transformOffset;

        // encender/apagar
        if (t <= 0.01f)
            vfx.Stop();
        else
            vfx.Play();
    }
}
