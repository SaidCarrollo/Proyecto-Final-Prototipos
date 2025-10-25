using UnityEngine;

public class WaterTrigger : MonoBehaviour
{
    [Tooltip("Secar al salir del trigger (opcional)")]
    public bool dryOnExit = true;

    private void OnTriggerEnter(Collider other)
    {
        SoundManager.Instance.PlaySFX("TrapoAgua");
        if (!other.CompareTag("Grab")) return;

        // 1) Si es tu toalla, avisa directo al adapter
        var towel = other.GetComponentInParent<TowelClothAdapter>();
        if (towel != null) towel.SetWet(true);

        // 2) Mantén compatibilidad con tu sistema
        var grabbable = other.GetComponent<GrabbableObject>();
        if (grabbable != null) grabbable.SetWet(true);
    }

    private void OnTriggerExit(Collider other)
    {
        SoundManager.Instance.PlaySFX("TrapoAgua2");
        if (!dryOnExit) return;
        if (!other.CompareTag("Grab")) return;

        var towel = other.GetComponentInParent<TowelClothAdapter>();
        if (towel != null) towel.SetWet(false);

        var grabbable = other.GetComponent<GrabbableObject>();
        if (grabbable != null) grabbable.SetWet(false);
    }
}
