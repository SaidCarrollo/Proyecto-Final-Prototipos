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

    [Header("UI para Insignias")]
    [Tooltip("El objeto padre donde se instanciarán los íconos de insignias correctas.")]
    [SerializeField] private Transform goodBadgesContainer;
    [Tooltip("El objeto padre donde se instanciarán los íconos de insignias incorrectas.")]
    [SerializeField] private Transform badBadgesContainer;

    [Tooltip("El Prefab de UI para el ÍCONO de una insignia (debe tener Image y BadgeIconUI).")]
    [SerializeField] private GameObject badgeIconPrefab; 

    [Header("UI para Tooltip de Información")]
    [Tooltip("El Panel que muestra la info de la insignia al hacer hover.")]
    [SerializeField] private GameObject tooltipPanel;
    [Tooltip("El campo de texto para el ID de la insignia en el tooltip.")]
    [SerializeField] private TextMeshProUGUI tooltipTitle;
    [Tooltip("El campo de texto para la descripción en el tooltip.")]
    [SerializeField] private TextMeshProUGUI tooltipDescription;


    void Start()
    {
        if (badgeManager == null)
        {
            Debug.LogError("BadgeManager no está asignado en BadgeDisplayUI.");
            return;
        }
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false); 
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
            mainResultText.text = (principalBadge != null) ? principalBadge.Descripcion : "Misión Fallida";
        }

        List<Badge> goodSecondaryBadges = badgeManager.GetUnlockedBadges(BadgeType.Correcto, BadgePriority.Secundario);
        List<Badge> badBadges = badgeManager.GetUnlockedBadges(BadgeType.Incorrecto); 

        PopulateBadgeContainer(goodBadgesContainer, goodSecondaryBadges);

        PopulateBadgeContainer(badBadgesContainer, badBadges);
    }

    private void PopulateBadgeContainer(Transform container, List<Badge> badges)
    {
        if (container == null || badgeIconPrefab == null) return;

        foreach (var badge in badges)
        {
            GameObject badgeIconInstance = Instantiate(badgeIconPrefab, container);
            BadgeIconUI badgeIconScript = badgeIconInstance.GetComponent<BadgeIconUI>();

            if (badgeIconScript != null)
            {
                badgeIconScript.Inicializar(badge, tooltipPanel, tooltipTitle, tooltipDescription);
            }
        }
    }
}