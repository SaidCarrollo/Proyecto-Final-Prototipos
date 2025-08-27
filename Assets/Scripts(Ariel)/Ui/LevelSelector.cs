using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))] // Asegura que el objeto tenga un RectTransform
public class LevelSelector : MonoBehaviour
{
    [Header("Configuraci�n de Objetos")]
    [Tooltip("Arrastra aqu� el objeto 'LevelsContainer' que contiene todos los paneles.")]
    public Transform levelsContainer; // Cambiado a Transform para que sea m�s general

    [Header("Configuraci�n de Botones")]
    public Button botonSiguiente;
    public Button botonAnterior;

    [Header("Configuraci�n de la Animaci�n")]
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
        // --- Inicializaci�n y Configuraci�n Autom�tica ---
        containerRectTransform = levelsContainer.GetComponent<RectTransform>();
        layoutGroup = levelsContainer.GetComponent<HorizontalLayoutGroup>();

        if (layoutGroup == null)
        {
            Debug.LogError("�Error! El 'levelsContainer' necesita un componente HorizontalLayoutGroup.");
            return;
        }

        // 1. Detecci�n autom�tica de niveles
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

        // 2. C�lculo autom�tico del ancho del panel + espaciado
        anchoDePanel = niveles[0].rect.width + layoutGroup.spacing;

        // Asignaci�n de listeners a los botones
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
            nivelActualIndex = 0; // L�gica circular
        }
        MoverA_Nivel(nivelActualIndex);
    }

    public void NivelAnterior()
    {
        nivelActualIndex--;
        if (nivelActualIndex < 0)
        {
            nivelActualIndex = niveles.Count - 1; // L�gica circular
        }
        MoverA_Nivel(nivelActualIndex);
    }

    private void MoverA_Nivel(int index)
    {
        // El c�lculo de la posici�n ahora incluye el espaciado
        float nuevaPosicionX = -index * anchoDePanel;

        // Animaci�n con DOTween
        containerRectTransform.DOAnchorPosX(nuevaPosicionX, duracionTransicion).SetEase(tipoDeEase);
    }

    private void PosicionarEnNivel(int index)
    {
        // Posicionamiento inicial sin animaci�n
        float posicionInicialX = -index * anchoDePanel;
        containerRectTransform.anchoredPosition = new Vector2(posicionInicialX, containerRectTransform.anchoredPosition.y);
    }
}