
using UnityEngine;
using UnityEngine.UI; 

public class InteractionPromptUI : MonoBehaviour
{
    [Tooltip("The UI GameObject that serves as the interaction prompt (e.g., an Image or Text).")]
    [SerializeField] private GameObject interactionPromptVisual;

    void Start()
    {
        if (interactionPromptVisual == null)
        {
            Debug.LogError("InteractionPromptUI: interactionPromptVisual is not assigned!", this);
            enabled = false; 
            return;
        }
        interactionPromptVisual.SetActive(false);
    }

    public void ShowPrompt()
    {
        if (interactionPromptVisual != null)
        {
            interactionPromptVisual.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (interactionPromptVisual != null)
        {
            interactionPromptVisual.SetActive(false);
        }
    }
}