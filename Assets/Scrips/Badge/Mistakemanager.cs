using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MistakeManager", menuName = "Game/Mistake Manager")]
public class MistakeManager : ScriptableObject
{
    private Dictionary<string, bool> mistakes = new Dictionary<string, bool>();

    [SerializeField]
    private List<string> allMistakeIDs = new List<string>()
    {
        "SartenEnAgua",
        "FuegoConTrapoSeco" 
    };

    private void OnEnable()
    {
        ResetMistakes();
    }

    public void RecordMistake(string mistakeID)
    {
        if (mistakes.ContainsKey(mistakeID))
        {
            mistakes[mistakeID] = true;
            Debug.Log($"¡Error Registrado!: {mistakeID}");
        }
        else
        {
            Debug.LogWarning($"Se intentó registrar un error con un ID no existente: {mistakeID}");
        }
    }

    public List<string> GetRecordedMistakes()
    {
        List<string> recorded = new List<string>();
        foreach (var mistake in mistakes)
        {
            if (mistake.Value)
            {
                recorded.Add(mistake.Key);
            }
        }
        return recorded;
    }

    public void ResetMistakes()
    {
        mistakes.Clear();
        foreach (string id in allMistakeIDs)
        {
            mistakes.Add(id, false);
        }
        Debug.Log("Todos los registros de errores han sido reseteados.");
    }
}