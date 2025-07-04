using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BadgeIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Badge badgeData;

    private GameObject tooltipPanel;
    private TextMeshProUGUI tooltipTitleText;
    private TextMeshProUGUI tooltipDescriptionText;
    private Image badgeImage;

    public void Inicializar(Badge data, GameObject panel, TextMeshProUGUI title, TextMeshProUGUI description)
    {
        badgeData = data;
        tooltipPanel = panel;
        tooltipTitleText = title;
        tooltipDescriptionText = description;

        badgeImage = GetComponent<Image>();
        if (badgeImage != null && badgeData.Icono != null)
        {
            badgeImage.sprite = badgeData.Icono;
        }
        else
        {
            Debug.LogWarning($"La insignia con ID '{badgeData.ID}' no tiene un ícono asignado.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null && badgeData != null)
        {
            tooltipTitleText.text = badgeData.ID;
            tooltipDescriptionText.text = badgeData.Descripcion;
            tooltipPanel.SetActive(true);

        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}