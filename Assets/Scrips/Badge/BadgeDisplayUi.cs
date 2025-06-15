using UnityEngine;
using TMPro; 
using System.Collections.Generic;

public class BadgeDisplayUI : MonoBehaviour
{
    [Header("Managers")]
    [Tooltip("Arrastra aqu� tu asset de BadgeManager.")]
    [SerializeField] private BadgeManager badgeManager;

    [Header("UI Setup")]
    [Tooltip("El objeto padre donde se instanciar�n los badges.")]
    [SerializeField] private Transform badgesContainer;
    [Tooltip("El Prefab de UI para un badge (debe tener un componente TextMeshProUGUI).")]
    [SerializeField] private GameObject badgeUIPrefab;

    void Start()
    {
        if (badgeManager == null || badgesContainer == null || badgeUIPrefab == null)
        {
            Debug.LogError("Faltan referencias en BadgeDisplayUI. Aseg�rate de asignar todo en el Inspector.");
            return;
        }

        DisplayUnlockedBadges();
    }

    private void DisplayUnlockedBadges()
    {
        foreach (Transform child in badgesContainer)
        {
            Destroy(child.gameObject);
        }

        List<string> unlockedBadges = badgeManager.GetUnlockedBadges();

        if (unlockedBadges.Count == 0)
        {
            Debug.Log("No se desbloque� ning�n badge.");
            return;
        }

        foreach (string badgeID in unlockedBadges)
        {
            GameObject badgeUI = Instantiate(badgeUIPrefab, badgesContainer);

            TextMeshProUGUI badgeText = badgeUI.GetComponentInChildren<TextMeshProUGUI>();
            if (badgeText != null)
            {
                badgeText.text = FormatBadgeIDToText(badgeID); 
            }
        }
    }

    private string FormatBadgeIDToText(string id)
    {
        switch (id)
        {
            case "FuegoApagadoConTrapo":
                return "Apagaste el fuego con el trapo.";
            case "LlamadaDeEmergencia":
                return "Pediste ayuda por tel�fono.";
            case "SartenBajoElAgua":
                return "Sumergiste la sart�n en agua.";
            case "CerrasteLaventana":
                return "Cerraste la ventana para evitar que el fuego absorva mas oxigeno.";
            default:
                return id;
        }
    }
}