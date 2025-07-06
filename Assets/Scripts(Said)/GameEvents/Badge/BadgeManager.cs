using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "BadgeManager", menuName = "Game/Badge Manager")]
public class BadgeManager : ScriptableObject
{
    [Tooltip("Aquí configuras todos los posibles badges y errores del juego.")]
    [SerializeField]
    private List<Badge> todosLosBadges = new List<Badge>();

    private Dictionary<string, Badge> badgesDict = new Dictionary<string, Badge>();

    private void OnEnable()
    {
        InicializarManager();
    }

    private void InicializarManager()
    {
        badgesDict.Clear();
        foreach (Badge badge in todosLosBadges)
        {
            badge.Desbloqueado = false;
            if (!badgesDict.ContainsKey(badge.ID))
            {
                badgesDict.Add(badge.ID, badge);
            }
        }
        Debug.Log("Badge Manager inicializado. Todos los badges y errores reseteados.");
    }

    public void UnlockBadge(string badgeID)
    {
        if (badgesDict.TryGetValue(badgeID, out Badge badge))
        {
            if (badge.Desbloqueado) return;

            badge.Desbloqueado = true;
            Debug.Log($"'{badge.Tipo}' | '{badge.Prioridad}' Desbloqueado: {badgeID}");

            if (BadgeAudioPlayer.Instance != null)
            {
                if (badge.Tipo == BadgeType.Correcto)
                {
                    BadgeAudioPlayer.Instance.PlayGoodBadgeSound();
                }
                else if (badge.Tipo == BadgeType.Incorrecto)
                {
                    BadgeAudioPlayer.Instance.PlayBadBadgeSound();
                }
            }
            else
            {
                Debug.LogWarning("Se intentó reproducir un sonido de badge, pero no se encontró una instancia de BadgeAudioPlayer en la escena.");
            }
        }
        else
        {
            Debug.LogWarning($"Se intentó usar un ID de badge no existente: {badgeID}");
        }
    }

    public List<Badge> GetUnlockedBadges(BadgeType? tipo = null, BadgePriority? prioridad = null)
    {
        var unlockedBadges = todosLosBadges.Where(b => b.Desbloqueado);

        if (tipo.HasValue)
        {
            unlockedBadges = unlockedBadges.Where(b => b.Tipo == tipo.Value);
        }

        if (prioridad.HasValue)
        {
            unlockedBadges = unlockedBadges.Where(b => b.Prioridad == prioridad.Value);
        }

        return unlockedBadges.ToList();
    }

    public void ResetBadges()
    {
        InicializarManager();
    }
}