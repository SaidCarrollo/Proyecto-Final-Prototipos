using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class NPCSaveableManager : MonoBehaviour
{
    [Header("Badges")]
    [SerializeField] private BadgeManager badgeManager;
    [SerializeField] private string allSavedBadgeID = "Evacuacion de NPC's";
    [SerializeField] private string notAllSavedBadgeID = "AlguienQuedoAtras";
    [SerializeField] private VignetteEvent vignetteEvent;
    [Header("Optional Events")]
    [SerializeField] private GameEventstring messageEvent;
    [SerializeField, TextArea(2, 4)] private string allSavedMessage = "¡Has logrado poner a todos a salvo!";

    [Header("NPC Configuration")]
    [Tooltip("Arrastra aquí todos los GameObjects de los NPCs que deben ser salvados en esta escena.")]
    [SerializeField] private List<NPCSaveable> npcsInScene = new List<NPCSaveable>();

    private bool allHaveBeenSaved = false;

    void Awake()
    {
        if (npcsInScene.Count == 0)
        {
            Debug.LogWarning($"La lista de NPCs en NPCSaveableManager está vacía. Asegúrate de asignarlos en el Inspector.", this);
        }

        foreach (var npc in npcsInScene)
        {
            if (npc != null)
            {
                npc.manager = this;
            }
            else
            {
                Debug.LogError("Hay un campo vacío en la lista de NPCs del NPCSaveableManager. Revisa las asignaciones en el Inspector.", this);
            }
        }
        Debug.Log($"NPCSaveableManager gestionará {npcsInScene.Count} NPCs asignados manualmente.");
    }

    public void CheckForAllSaved()
    {
        if (allHaveBeenSaved || npcsInScene.Count == 0) return;

        bool allAreSavedNow = npcsInScene.All(npc => npc.IsSaved);

        if (allAreSavedNow)
        {
            allHaveBeenSaved = true;
            Debug.Log("<color=green>¡ÉXITO!</color> Todos los NPCs han sido salvados.");
            vignetteEvent.Raise(Color.green, 0.4f, 2f);
            if (badgeManager != null)
            {
                badgeManager.UnlockBadge(allSavedBadgeID);
            }

            if (messageEvent != null && !string.IsNullOrEmpty(allSavedMessage))
            {
                messageEvent.Raise(allSavedMessage);
            }
        }
    }

    public void EvaluateAtGameEnd()
    {
        if (!allHaveBeenSaved)
        {
            vignetteEvent.Raise(Color.red, 0.5f, 3f);
            Debug.Log("<color=red>FALLO:</color> El juego terminó y no todos los NPCs fueron salvados.");
            if (badgeManager != null)
            {
                badgeManager.UnlockBadge(notAllSavedBadgeID);
            }
        }
    }
}