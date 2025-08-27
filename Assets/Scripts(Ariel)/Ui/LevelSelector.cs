using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))] // Asegura que el objeto tenga un RectTransform
public class LevelSelector : MonoBehaviour
{
    [Header("Configuración de Objetos")]
    [Tooltip("Arrastra aquí el objeto 'LevelsContainer' que contiene todos los paneles.")]
    public Transform levelsContainer; // Cambiado a Transform para que sea más general

    [Header("Configuración de Botones")]
    public Button botonSiguiente;
    public Button botonAnterior;

    [Header("Configuración de la Animación")]
    public float duracionTransicion = 0.5f;
    public Ease tipoDeEase = Ease.OutQuad;

    // --- Variables Privadas ---
    private List<RectTransform> niveles = new List<RectTransform>(); // La lista ahora es privada y de RectTransform
    private RectTransform containerRectTransform;
    private int nivelActualIndex = 0;
    private float anchoDePanel;
    private HorizontalLayoutGroup layoutGroup;

    void Awake()
    {
        // --- Inicialización y Configuración Automática ---
        containerRectTransform = levelsContainer.GetComponent<RectTransform>();
        layoutGroup = levelsContainer.GetComponent<HorizontalLayoutGroup>();

        if (layoutGroup == null)
        {
            Debug.LogError("¡Error! El 'levelsContainer' necesita un componente HorizontalLayoutGroup.");
            return;
        }

        // 1. Detección automática de niveles
        niveles.Clear();
        foreach (RectTransform child in levelsContainer)
        {
            if (child.gameObject.activeSelf) // Solo considera los hijos activos
            {
                niveles.Add(child);
            }
        }

        if (niveles.Count == 0)
        {
            Debug.LogWarning("No se encontraron niveles activos dentro del 'levelsContainer'.");
            return;
        }

        // 2. Cálculo automático del ancho del panel + espaciado
        anchoDePanel = niveles[0].rect.width + layoutGroup.spacing;

        // Asignación de listeners a los botones
        botonSiguiente.onClick.AddListener(SiguienteNivel);
        botonAnterior.onClick.AddListener(NivelAnterior);

        // Se posiciona en el primer nivel al iniciar
        PosicionarEnNivel(0);
    }

    public void SiguienteNivel()
    {
        nivelActualIndex++;
        if (nivelActualIndex >= niveles.Count)
        {
            nivelActualIndex = 0; // Lógica circular
        }
        MoverA_Nivel(nivelActualIndex);
    }

    public void NivelAnterior()
    {
        nivelActualIndex--;
        if (nivelActualIndex < 0)
        {
            nivelActualIndex = niveles.Count - 1; // Lógica circular
        }
        MoverA_Nivel(nivelActualIndex);
    }

    private void MoverA_Nivel(int index)
    {
        // El cálculo de la posición ahora incluye el espaciado
        float nuevaPosicionX = -index * anchoDePanel;

        // Animación con DOTween
        containerRectTransform.DOAnchorPosX(nuevaPosicionX, duracionTransicion).SetEase(tipoDeEase);
    }

    private void PosicionarEnNivel(int index)
    {
        // Posicionamiento inicial sin animación
        float posicionInicialX = -index * anchoDePanel;
        containerRectTransform.anchoredPosition = new Vector2(posicionInicialX, containerRectTransform.anchoredPosition.y);
    }
}