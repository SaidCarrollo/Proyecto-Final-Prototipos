using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BadgeIconUI : MonoBehaviour
{
    [Header("Componentes Internos del Prefab")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public void Inicializar(Badge badge)
    {
        // Asignamos los datos del badge a los elementos visuales de la tarjeta
        if (titleText != null) titleText.text = badge.ID; // O badge.Nombre si tienes ese campo
        if (descriptionText != null) descriptionText.text = badge.Descripcion;

        if (iconImage != null && badge.Icono != null)
        {
            iconImage.sprite = badge.Icono;
        }
    }
}