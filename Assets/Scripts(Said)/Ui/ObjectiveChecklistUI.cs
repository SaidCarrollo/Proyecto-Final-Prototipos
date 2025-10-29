using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

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

    /// <summary>
    /// Si quieres reconstruir dinámicamente (p. ej., otro tramo del nivel),
    /// llama esto con nuevas entradas.
    /// </summary>
    public void RebuildWithEntries(List<ChecklistEntry> newEntries)
    {
        entries = newEntries;
        BuildList();
    }

    private void BuildList()
    {
        if (container == null || checklistTextPrefab == null || badgeManager == null) return;

        foreach (Transform child in container) Destroy(child.gameObject);
        map.Clear();

        // Construye cada fila según el orden de 'entries'
        foreach (var e in entries)
        {
            if (string.IsNullOrWhiteSpace(e.badgeId)) continue;

            // Intenta leer el Badge para usar su Descripcion como fallback
            string displayText = e.listTextOverride;
            if (string.IsNullOrWhiteSpace(displayText))
            {
                if (badgeManager.TryGetBadge(e.badgeId, out var badge) && !string.IsNullOrWhiteSpace(badge.Descripcion))
                    displayText = badge.Descripcion;
                else
                    displayText = e.badgeId; // último fallback: el ID
            }

            var go = Instantiate(checklistTextPrefab, container);
            var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
            tmp.text = displayText;
            tmp.color = pendingColor;
            tmp.fontStyle &= ~FontStyles.Strikethrough;
            tmp.alpha = 1f;

            map[e.badgeId] = tmp;

            // Si ya estaba desbloqueado antes de crear la UI
            if (badgeManager.TryGetBadge(e.badgeId, out var b) && b.Desbloqueado)
                StartCoroutine(MarkDone(tmp));
        }
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
}
