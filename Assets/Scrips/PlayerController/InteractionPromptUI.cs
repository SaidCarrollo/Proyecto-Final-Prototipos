
using UnityEngine;
using UnityEngine.UI; 

public class InteractionPromptUI : MonoBehaviour
{
    [Tooltip("The UI GameObject that serves as the interaction prompt (e.g., an Image or Text).")]
    [SerializeField] private GameObject interactionPromptVisual;
    [Tooltip("Botón de la UI para interactuar desde móvil .")]
    [SerializeField] private GameObject interactUIButton;
    void Start()
    {
        if (interactionPromptVisual == null)
        {
            Debug.LogError("InteractionPromptUI: interactionPromptVisual no está asignado!", this);
            enabled = false;
            return;
        }

        interactionPromptVisual.SetActive(false);

        if (interactUIButton != null)
            interactUIButton.SetActive(false);
    }

    public void ShowPrompt(bool isGrabbable)
    {
        if (interactionPromptVisual != null)
            interactionPromptVisual.SetActive(true);

        if (interactUIButton != null)
            interactUIButton.SetActive(true);

    }

    public void HidePrompt()
    {
        if (interactionPromptVisual != null)
            interactionPromptVisual.SetActive(false);

        if (interactUIButton != null)
            interactUIButton.SetActive(false); // Ocultar botón también
    }
}