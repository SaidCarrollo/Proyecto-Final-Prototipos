using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI; // por si usamos LayoutUtility

public class RollingNumberDisplay : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Texto que muestra el valor actual visible.")]
    public TextMeshProUGUI currentText;

    [Tooltip("Texto auxiliar que entra/sale animado.")]
    public TextMeshProUGUI nextText;

    [Header("Animación")]
    [SerializeField] private float rollDuration = 0.12f; // bajito para aguantar hold rápido
    [SerializeField] private Ease rollEase = Ease.OutQuad;

    // Estado interno robusto
    private int displayedValue;        // Lo que se muestra ahora mismo en pantalla
    private int? queuedValue = null;   // Último valor pendiente durante animación
    private bool isAnimating = false;
    private Sequence activeSeq;

    public int CurrentValue => displayedValue;
    public bool IsAnimating => isAnimating;

    void Awake()
    {
        if (!currentText || !nextText)
        {
            Debug.LogError("[RollingNumberDisplay] Asigna currentText y nextText en el inspector.");
            enabled = false;
            return;
        }

        // Estado visual estable
        currentText.rectTransform.anchoredPosition = Vector2.zero;
        nextText.rectTransform.anchoredPosition = Vector2.zero;
        nextText.text = "";
    }

    void OnDisable()
    {
        if (activeSeq != null && activeSeq.IsActive()) activeSeq.Kill(false);
        isAnimating = false;
        queuedValue = null;
    }

    /// <summary>Setea el valor sin animación (por ejemplo al iniciar).</summary>
    public void SetInstant(int value)
    {
        displayedValue = value;
        currentText.text = displayedValue.ToString();

        // Dejar el "siguiente" fuera y vacío
        var crt = currentText.rectTransform;
        var nrt = nextText.rectTransform;

        crt.anchoredPosition = Vector2.zero;
        nrt.anchoredPosition = Vector2.zero;
        nextText.text = "";

        // Cancelar cualquier animación
        if (activeSeq != null && activeSeq.IsActive()) activeSeq.Kill(false);
        isAnimating = false;
        queuedValue = null;
    }

    /// <summary>
    /// Pide animar hasta un nuevo valor. Si está animando, encola el último.
    /// </summary>
    public void AnimateTo(int newValue)
    {
        if (!isActiveAndEnabled) return;

        // Si ya estamos mostrando ese valor, no hay nada que hacer
        if (newValue == displayedValue) return;

        // Si está animando, encola y sal (nos quedamos con el ÚLTIMO pedido)
        if (isAnimating)
        {
            queuedValue = newValue;
            return;
        }

        PlayRoll(displayedValue, newValue);
    }

    // ================== Internals ==================

    private void PlayRoll(int fromValue, int toValue)
    {
        isAnimating = true;

        // Asegurar estado base limpio
        if (activeSeq != null && activeSeq.IsActive()) activeSeq.Kill(false);

        var crt = currentText.rectTransform;
        var nrt = nextText.rectTransform;

        // Texto actual/entrante
        currentText.text = fromValue.ToString();
        nextText.text = toValue.ToString();

        // Altura del "rodillo"
        float h = GetLineHeight(crt, currentText);
        bool goingUp = toValue > fromValue;

        // Posiciones iniciales
        crt.anchoredPosition = Vector2.zero;
        nrt.anchoredPosition = goingUp
            ? new Vector2(0f, h)   // entra desde arriba si sube
            : new Vector2(0f, -h); // entra desde abajo si baja

        // Animación (UNSCALED TIME)
        activeSeq = DOTween.Sequence();
        activeSeq.SetUpdate(true); // <- importante: tiempo real, ignora timeScale
        activeSeq.Join(
            crt.DOAnchorPosY(goingUp ? -h : h, rollDuration)
               .SetEase(rollEase)
        );
        activeSeq.Join(
            nrt.DOAnchorPosY(0f, rollDuration)
               .SetEase(rollEase)
        );

        activeSeq.OnComplete(() =>
        {
            // Fijar estado estable al nuevo valor
            displayedValue = toValue;
            currentText.text = displayedValue.ToString();
            crt.anchoredPosition = Vector2.zero;

            // Dejar el "next" preparado fuera de vista
            nextText.text = "";
            nrt.anchoredPosition = goingUp ? new Vector2(0f, h) : new Vector2(0f, -h);

            isAnimating = false;
            activeSeq = null;

            // Si durante la animación llegaron nuevos cambios, lánzalos ahora
            if (queuedValue.HasValue && queuedValue.Value != displayedValue)
            {
                int pending = queuedValue.Value;
                queuedValue = null;
                PlayRoll(displayedValue, pending);
            }
            else
            {
                queuedValue = null;
            }
        });
    }

    private float GetLineHeight(RectTransform rt, TMP_Text tmp)
    {
        // Intento 1: usar el rect
        float h = rt.rect.height;
        if (h > 0f) return h;

        // Intento 2: preferredHeight de TMP
        float ph = tmp.preferredHeight;
        if (ph > 0f) return ph;

        // Fallback razonable
        return Mathf.Max(32f, tmp.fontSize * 1.25f);
    }
}
