using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BadgeManager", menuName = "Game/Badge Manager")]
public class BadgeManager : ScriptableObject
{
    private Dictionary<string, bool> badges = new Dictionary<string, bool>();

    [SerializeField]
    private List<string> allBadgeIDs = new List<string>()
    {
        "FuegoApagadoConTrapo",
        "LlamadaDeEmergencia",
        "SartenBajoElAgua",
        "CerrasteLaVentana" 
    };

    private void OnEnable()
    {
        ResetBadges();
    }

    public void UnlockBadge(string badgeID)
    {
        if (badges.ContainsKey(badgeID))
        {
            badges[badgeID] = true;
            Debug.Log($"¡Badge Desbloqueado!: {badgeID}");
        }
        else
        {
            Debug.LogWarning($"Se intentó desbloquear un badge con un ID no existente: {badgeID}");
        }
    }

    public bool IsBadgeUnlocked(string badgeID)
    {
        if (badges.ContainsKey(badgeID))
        {
            return badges[badgeID];
        }
        return false;
    }

    public List<string> GetUnlockedBadges()
    {
        List<string> unlocked = new List<string>();
        foreach (var badge in badges)
        {
            if (badge.Value)
            {
                unlocked.Add(badge.Key);
            }
        }
        return unlocked;
    }

    public void ResetBadges()
    {
        badges.Clear();
        foreach (string id in allBadgeIDs)
        {
            badges.Add(id, false);
        }
        Debug.Log("Todos los badges han sido reseteados.");
    }
}