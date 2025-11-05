using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))] // El objeto que tiene este script debe tener RectTransform (por si acaso)
public class LevelSelector : MonoBehaviour
{
    [Header("Configuración de Objetos")]
    [Tooltip("Arrastra aquí el objeto 'LevelsContainer' que contiene todos los paneles.")]
    public Transform levelsContainer; // Debe tener RectTransform

    [Header("Configuración de Botones")]
    public Button botonSiguiente;
    public Button botonAnterior;

    [Header("Configuración de la Animación")]
    public float duracionTransicion = 0.5f;
    public Ease tipoDeEase = Ease.OutQuad;

    // --- Variables Privadas ---
    // Lista de niveles (sus RectTransform), detectados automáticamente
    private List<RectTransform> niveles = new List<RectTransform>();

    // Posiciones objetivo en X del container para que cada nivel quede centrado
    private List<float> posicionesNivelX = new List<float>();

    private RectTransform containerRectTransform;
    private int nivelActualIndex = 0;

    // Exponer cantidad de niveles si lo necesitas en otros scripts
    public int TotalNiveles => niveles.Count;

    void Awake()
    {
        if (levelsContainer == null)
        {
            Debug.LogError("[LevelSelector] No se asignó 'levelsContainer' en el inspector.");
            return;
        }

        containerRectTransform = levelsContainer.GetComponent<RectTransform>();
        if (containerRectTransform == null)
        {
            Debug.LogError("[LevelSelector] 'levelsContainer' necesita un RectTransform.");
            return;
        }

        // 1. Detección automática de niveles (hijos activos)
        niveles.Clear();
        posicionesNivelX.Clear();

        foreach (RectTransform child in levelsContainer)
        {
            if (child.gameObject.activeSelf)
            {
                niveles.Add(child);
            }
        }

        if (niveles.Count == 0)
        {
            Debug.LogWarning("[LevelSelector] No se encontraron niveles activos dentro del 'levelsContainer'.");
            return;
        }

        // 2. Calcular la posición objetivo del container para cada nivel
        //
        // Idea:
        //  - Si el nivel i tiene anchoredPosition.x = P_i
        //  - Para que ese nivel quede centrado respecto al container,
        //    movemos el container a X = -P_i
        //
        //  => guardamos esos valores en una lista
        foreach (var nivel in niveles)
        {
            float targetX = -nivel.anchoredPosition.x;
            posicionesNivelX.Add(targetX);
        }

        // 3. Asignación de listeners a los botones
        if (botonSiguiente != null)
            botonSiguiente.onClick.AddListener(SiguienteNivel);

        if (botonAnterior != null)
            botonAnterior.onClick.AddListener(NivelAnterior);

        // 4. Posicionamos en el primer nivel al iniciar (sin animación)
        PosicionarEnNivel(0);
    }

    public void SiguienteNivel()
    {
        if (niveles.Count == 0) return;

        nivelActualIndex++;
        if (nivelActualIndex >= niveles.Count)
        {
            nivelActualIndex = 0; // Lógica circular
        }
        MoverA_Nivel(nivelActualIndex);
    }

    public void NivelAnterior()
    {
        if (niveles.Count == 0) return;

        nivelActualIndex--;
        if (nivelActualIndex < 0)
        {
            nivelActualIndex = niveles.Count - 1; // Lógica circular
        }
        MoverA_Nivel(nivelActualIndex);
    }

    private void MoverA_Nivel(int index)
    {
        if (index < 0 || index >= posicionesNivelX.Count)
        {
            Debug.LogWarning("[LevelSelector] Índice de nivel fuera de rango: " + index);
            return;
        }

        float nuevaPosicionX = posicionesNivelX[index];

        // Animación con DOTween
        containerRectTransform
            .DOAnchorPosX(nuevaPosicionX, duracionTransicion)
            .SetEase(tipoDeEase);
    }

    private void PosicionarEnNivel(int index)
    {
        if (index < 0 || index >= posicionesNivelX.Count)
        {
            Debug.LogWarning("[LevelSelector] Índice de nivel fuera de rango al posicionar: " + index);
            return;
        }

        float posicionInicialX = posicionesNivelX[index];
        Vector2 pos = containerRectTransform.anchoredPosition;
        pos.x = posicionInicialX;
        containerRectTransform.anchoredPosition = pos;

        nivelActualIndex = index;
    }
}
