using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq; 

public class BadgeDisplayUI : MonoBehaviour
{
    [Header("Manager")]
    [Tooltip("Arrastra aquí tu asset de BadgeManager.")]
    [SerializeField] private BadgeManager badgeManager;

    [Header("UI para Resultado Principal")]
    [Tooltip("El campo de texto para mostrar el resultado principal (victoria/derrota).")]
    [SerializeField] private TextMeshProUGUI mainResultText;

    [Header("UI para Acciones Secundarias Correctas")]
    [Tooltip("El objeto padre donde se mostrarán los logros secundarios.")]
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

        Badge principalBadge = badgeManager.GetUnlockedBadges(BadgeType.Correcto, BadgePriority.Principal).FirstOrDefault();
        if (mainResultText != null)
        {
            if (principalBadge != null)
            {
                mainResultText.text = principalBadge.Descripcion;
            }
            else
            {
                mainResultText.text = "Misión Fallida";
            }
        }

        List<Badge> goodSecondaryBadges = badgeManager.GetUnlockedBadges(BadgeType.Correcto, BadgePriority.Secundario);
        if (goodBadgesContainer != null && goodBadgeUIPrefab != null)
        {
            foreach (var badge in goodSecondaryBadges)
            {
                GameObject badgeUI = Instantiate(goodBadgeUIPrefab, goodBadgesContainer);
                var badgeText = badgeUI.GetComponentInChildren<TextMeshProUGUI>();
                if (badgeText != null)
                {
                    badgeText.text = badge.Descripcion;
                }
            }
        }

        List<Badge> badBadges = badgeManager.GetUnlockedBadges(BadgeType.Incorrecto, BadgePriority.Secundario);
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