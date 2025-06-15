using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MistakeDisplayUI : MonoBehaviour
{
    [SerializeField] private MistakeManager mistakeManager;
    [SerializeField] private TextMeshProUGUI mistakeText;

    void Start()
    {
        if (mistakeManager == null || mistakeText == null) return;

        List<string> mistakes = mistakeManager.GetRecordedMistakes();
        if (mistakes.Count > 0)
        {
            mistakeText.text = "Causa: " + FormatMistakeIDToText(mistakes[0]);
        }
        else
        {
            mistakeText.text = "";
        }
    }

    private string FormatMistakeIDToText(string id)
    {
        switch (id)
        {
            case "SartenEnAgua":
                return "Arrojaste aceite caliente al agua, causando una violenta reacción.";
            case "FuegoConTrapoSeco":
                return "Intentaste apagar el fuego con un trapo seco, avivando las llamas.";
            default:
                return "Cometiste un error.";
        }
    }
}