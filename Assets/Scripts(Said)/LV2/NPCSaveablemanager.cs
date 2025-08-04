using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class NPCSaveableManager : MonoBehaviour
{
    [Header("Badges")]
    [SerializeField] private BadgeManager badgeManager;
    [SerializeField] private string allSavedBadgeID = "TodosSalvos";
    [SerializeField] private string notAllSavedBadgeID = "AlguienQuedoAtras";

    [Header("Optional Events")]
    [SerializeField] private GameEventstring messageEvent;
    [SerializeField, TextArea(2, 4)] private string allSavedMessage = "¡Has logrado poner a todos a salvo!";

    private List<NPCSaveable> npcsInScene;
    private bool allHaveBeenSaved = false;

    void Awake()
    {
        npcsInScene = new List<NPCSaveable>(FindObjectsOfType<NPCSaveable>());

        foreach (var npc in npcsInScene)
        {
            npc.manager = this;
        }
        Debug.Log($"NPCSaveableManager encontró {npcsInScene.Count} NPCs para salvar.");
    }

    public void CheckForAllSaved()
    {
        if (allHaveBeenSaved) return;

        bool allAreSavedNow = npcsInScene.All(npc => npc.IsSaved);

        if (allAreSavedNow)
        {
            allHaveBeenSaved = true;
            Debug.Log("<color=green>¡ÉXITO!</color> Todos los NPCs han sido salvados.");

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
            Debug.Log("<color=red>FALLO:</color> El juego terminó y no todos los NPCs fueron salvados.");
            if (badgeManager != null)
            {
                badgeManager.UnlockBadge(notAllSavedBadgeID);
            }
        }
    }
}