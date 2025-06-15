using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BadgeDisplayUI : MonoBehaviour
{
    [Header("Manager")]
    [Tooltip("Arrastra aquí tu asset de BadgeManager.")]
    [SerializeField] private BadgeManager badgeManager;

    [Header("UI para Acciones Correctas")]
    [Tooltip("El objeto padre donde se mostrarán los logros.")]
    [SerializeField] private Transform goodBadgesContainer;
    [Tooltip("El Prefab de UI para un logro (debe tener TextMeshProUGUI).")]
    [SerializeField] private GameObject goodBadgeUIPrefab;

    [Header("UI para Errores (Causa de la derrota)")]
    [Tooltip("El objeto padre donde se mostrarán los errores.")]
    [SerializeField] private Transform badBadgesContainer;
    [Tooltip("El Prefab de UI para un error (debe tener TextMeshProUGUI).")]
    [SerializeField] private GameObject badBadgeUIPrefab;

    void Start()
    {
        if (badgeManager == null)
        {
            Debug.LogError("BadgeManager no está asignado en BadgeDisplayUI. Por favor, asígnalo en el Inspector.");
            return;
        }
        DisplayResults();
    }

    private void DisplayResults()
    {
        if (goodBadgesContainer != null) foreach (Transform child in goodBadgesContainer) Destroy(child.gameObject);
        if (badBadgesContainer != null) foreach (Transform child in badBadgesContainer) Destroy(child.gameObject);

        // --- Mostrar Badges Correctos ---
        List<Badge> goodBadges = badgeManager.GetUnlockedBadges(BadgeType.Correcto);
        if (goodBadgesContainer != null && goodBadgeUIPrefab != null)
        {
            foreach (var badge in goodBadges)
            {
                GameObject badgeUI = Instantiate(goodBadgeUIPrefab, goodBadgesContainer);
                var badgeText = badgeUI.GetComponentInChildren<TextMeshProUGUI>();
                if (badgeText != null)
                {
                    badgeText.text = badge.Descripcion; 
                }
            }
        }

        List<Badge> badBadges = badgeManager.GetUnlockedBadges(BadgeType.Incorrecto);
        if (badBadgesContainer != null && badBadgeUIPrefab != null)
        {
            foreach (var badge in badBadges)
            {
                GameObject badgeUI = Instantiate(badBadgeUIPrefab, badBadgesContainer);
                var badgeText = badgeUI.GetComponentInChildren<TextMeshProUGUI>();
                if (badgeText != null)
                {
                    badgeText.text = "Causa: " + badge.Descripcion;
                }
            }
        }
    }
}