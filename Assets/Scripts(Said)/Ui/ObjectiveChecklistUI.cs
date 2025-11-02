using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class ChecklistEntry
{
    [Tooltip("ID del Badge (el mismo que usas en UnlockBadge)")]
    public string badgeId;

    [TextArea]
    [Tooltip("Cómo quieres que se muestre en la checklist. Si lo dejas vacío, usa la descripción del badge y, si no hay, el ID.")]
    public string listTextOverride;
}

public class ObjectiveChecklistUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private RectTransform container;              // Si es null, usa this.transform
    [SerializeField] private GameObject checklistTextPrefab;       // Prefab con TextMeshProUGUI
    [SerializeField] private BadgeManager badgeManager;

    [Header("Entradas (ID + Texto en lista)")]
    [Tooltip("Lista ordenada: cada entrada indica el ID del badge y el texto que quieres mostrar en la checklist.")]
    [SerializeField] private List<ChecklistEntry> entries = new();

    [Header("Style")]
    [SerializeField] private Color pendingColor = new Color(1f, 1f, 1f, 0.90f);
    [SerializeField] private Color doneColor = new Color(0.30f, 0.85f, 0.30f, 1f);
    [SerializeField] private float fadeDelay = 0.8f;
    [SerializeField] private float fadeDuration = 0.4f;

    // ⬇️ NUEVO: configuración para cuando pasamos al segundo contador
    [Header("Segunda fase / reemplazo")]
    [Tooltip("Si lo marcas, cuando el GameManager te lo pida puedes pasar a otra lista.")]
    [SerializeField] private bool hasSecondPhase = false;

    [Tooltip("Las misiones que quieres que aparezcan cuando ya estás en el segundo contador (p. ej. Evacúa, No abras la puerta, etc.)")]
    [SerializeField] private List<ChecklistEntry> secondPhaseEntries = new();

    [Tooltip("Color del tachado cuando la misión se perdió / no se hizo a tiempo.")]
    [SerializeField] private Color failedColor = new Color(1f, 0.25f, 0.25f, 1f);

    [SerializeField] private float failFadeDelay = 0.35f;
    [SerializeField] private float failFadeDuration = 0.25f;

    // Mapa: badgeId -> TMP
    private readonly Dictionary<string, TextMeshProUGUI> map = new();

    private void Awake()
    {
        if (container == null) container = (RectTransform)transform;
    }

    private void OnEnable()
    {
        if (badgeManager != null)
            badgeManager.OnBadgeUnlocked += HandleBadgeUnlocked;
    }

    private void OnDisable()
    {
        if (badgeManager != null)
            badgeManager.OnBadgeUnlocked -= HandleBadgeUnlocked;
    }

    private void Start()
    {
        BuildList();
    }

    public void RebuildWithEntries(List<ChecklistEntry> newEntries)
    {
        // Guardamos las nuevas misiones
        entries = newEntries;
        // Y reconstruimos en el SIGUIENTE frame, cuando los Destroy ya se hicieron
        StartCoroutine(RebuildNextFrame());
    }

    private IEnumerator RebuildNextFrame()
    {
        // Espera 1 frame para que Unity destruya de verdad a los hijos viejos
        yield return null;
        BuildList();
    }

    private void BuildList()
    {
        if (container == null) container = (RectTransform)transform;
        if (container == null || checklistTextPrefab == null || badgeManager == null) return;

        // 1) limpiar hijos del container (los de la primera fase)
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }

        map.Clear();

        // 2) crear los nuevos usando EL MISMO container
        foreach (var e in entries)
        {
            if (string.IsNullOrWhiteSpace(e.badgeId))
                continue;

            string displayText = e.listTextOverride;
            if (string.IsNullOrWhiteSpace(displayText))
            {
                if (badgeManager.TryGetBadge(e.badgeId, out var badge) && !string.IsNullOrWhiteSpace(badge.Descripcion))
                    displayText = badge.Descripcion;
                else
                    displayText = e.badgeId;
            }

            // 👇 esto garantiza que quede bajo el layout del container
            var go = Instantiate(checklistTextPrefab, container);
            var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
            tmp.text = displayText;
            tmp.color = pendingColor;
            tmp.fontStyle &= ~FontStyles.Strikethrough;
            tmp.alpha = 1f;

            // muy importante: normalizar transform
            var rt = go.transform as RectTransform;
            rt.anchoredPosition3D = Vector3.zero;
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;

            map[e.badgeId] = tmp;

            // si ya estaba hecho el badge, se marca
            if (badgeManager.TryGetBadge(e.badgeId, out var b) && b.Desbloqueado)
                StartCoroutine(MarkDone(tmp));
        }

        // 3) forzar al layout a recalcular
        LayoutRebuilder.ForceRebuildLayoutImmediate(container);
    }

    public void ForceFailPendingsAndGoToSecondPhase()
    {
        if (!hasSecondPhase) return;
        StartCoroutine(FailPendingsThenSwap());
    }
    private void HandleBadgeUnlocked(string badgeID)
    {
        if (!map.TryGetValue(badgeID, out var tmp)) return;
        StartCoroutine(MarkDone(tmp));
    }

    private IEnumerator MarkDone(TextMeshProUGUI tmp)
    {
        if (tmp == null) yield break;

        tmp.DOKill();
        tmp.fontStyle |= FontStyles.Strikethrough;                 // tachado
        tmp.DOColor(doneColor, 0.20f).SetUpdate(true);             // verde

        yield return new WaitForSecondsRealtime(fadeDelay);
        yield return tmp.DOFade(0f, fadeDuration).SetUpdate(true).WaitForCompletion();

        Destroy(tmp.gameObject);                                   // sale de la lista
    }

    private IEnumerator FailPendingsThenSwap()
    {
        // 1) poner en rojo y tachar lo NO desbloqueado
        foreach (var kvp in map)
        {
            var id = kvp.Key;
            var tmp = kvp.Value;
            if (tmp == null) continue;

            bool unlocked = badgeManager.TryGetBadge(id, out var b) && b.Desbloqueado;
            if (!unlocked)
            {
                tmp.DOKill();
                tmp.fontStyle |= FontStyles.Strikethrough;
                tmp.DOColor(failedColor, 0.2f).SetUpdate(true);
            }
        }

        yield return new WaitForSecondsRealtime(failFadeDelay);

        // 2) fade-out solo de las falladas
        foreach (var kvp in map)
        {
            var id = kvp.Key;
            var tmp = kvp.Value;
            if (tmp == null) continue;

            bool unlocked = badgeManager.TryGetBadge(id, out var b) && b.Desbloqueado;
            if (!unlocked)
            {
                yield return tmp.DOFade(0f, failFadeDuration).SetUpdate(true).WaitForCompletion();
            }
        }

        // 3) reemplazar por la lista de la segunda fase
        RebuildWithEntries(secondPhaseEntries);
    }
}