using UnityEngine;
using TMPro; // O usa UnityEngine.UI si usas texto legacy

public class InteractionPromptUI : MonoBehaviour
{
    [Tooltip("El objeto visual completo (panel/botón)")]
    [SerializeField] private GameObject interactionPromptVisual;

    [Tooltip("El componente de texto dentro del botón")]
    [SerializeField] private TextMeshProUGUI promptText;

    void Start()
    {
        if (interactionPromptVisual == null)
        {
            Debug.LogError("InteractionPromptUI: Falta asignar interactionPromptVisual.");
            enabled = false;
            return;
        }
        interactionPromptVisual.SetActive(false);
    }

    public void ShowPrompt()
    {
        interactionPromptVisual.SetActive(true);
    }

    public void HidePrompt()
    {
        interactionPromptVisual.SetActive(false);
    }

    // --- NUEVO MÉTODO ---
    public void SetPromptText(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
        }
    }
}