using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class TowelFlattenZone : MonoBehaviour
{
    public string requiredTag = "Grab";

    [Header("Anchors")]
    [Tooltip("Puntos de anclaje (Transform) en la superficie.")]
    public Transform[] anchors;

    [Tooltip("Alinear también la rotación de la toalla con el anchor.")]
    public bool alignRotation = false;

    [Tooltip("Suavizado de posición (s).")]
    public float posSmoothTime = 0.12f;

    [Tooltip("Velocidad de lerp rotacional.")]
    public float rotLerpSpeed = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        var adapter = other.GetComponentInParent<TowelClothAdapter>();
        if (adapter == null) return;

        // ¡OJO! NO planchamos aquí. El adapter hará SetAnchoredDrape() al anclar.
        if (anchors != null && anchors.Length > 0)
        {
            Transform best = anchors.Where(a => a != null)
                                    .OrderBy(a => (a.position - other.transform.position).sqrMagnitude)
                                    .FirstOrDefault();
            if (best != null)
            {
                adapter.alignRotationOnAnchor = alignRotation;
                adapter.anchorPosSmoothTime = Mathf.Max(0.0001f, posSmoothTime);
                adapter.anchorRotLerpSpeed = Mathf.Max(0f, rotLerpSpeed);
                adapter.AttachToAnchor(best);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        var adapter = other.GetComponentInParent<TowelClothAdapter>();
        if (adapter == null) return;

        adapter.DetachAnchor();
    }
}
