using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Referencia al objeto de texto para mostrar mensajes informativos.")]
    [SerializeField] private TextMeshProUGUI infoText;

    [Tooltip("Referencia al objeto de texto para mostrar el objetivo actual.")]
    [SerializeField] private TextMeshProUGUI objectiveText;

    [Header("Objective Settings")]
    [Tooltip("El texto que se mostrará como objetivo inicial. Se puede cambiar por nivel en el Inspector.")]
    [SerializeField] private string initialObjectiveText = "Apaga el fuego.";

    [Header("Animation Settings")]
    [Tooltip("Tiempo que tarda el texto en aparecer (fade-in).")]
    [SerializeField] private float fadeInTime = 0.5f;

    [Tooltip("Tiempo que el mensaje permanece visible en pantalla con alfa completo.")]
    [SerializeField] private float displayTime = 3f;

    [Tooltip("Tiempo que tarda el texto en desaparecer (fade-out).")]
    [SerializeField] private float fadeOutTime = 0.5f;

    [Tooltip("Tiempo entre letras del objetivo")]
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("Window Injury Overlay")]
    [Tooltip("Imagen pantalla completa con el sprite de 'vidrio roto / corte'.")]
    [SerializeField] private Image windowInjuryOverlay;

    [Tooltip("Duración del fade in del overlay de herida.")]
    [SerializeField] private float overlayFadeIn = 0.15f;

    [Tooltip("Cuánto tiempo permanece visible el overlay al máximo.")]
    [SerializeField] private float overlayHold = 0.4f;

    [Tooltip("Duración del fade out del overlay.")]
    [SerializeField] private float overlayFadeOut = 0.4f;


    private Coroutine fadeCoroutine;
    private Tween typingTween;
    private Coroutine overlayCoroutine;
    void Start()
    {
        if (infoText != null)
        {
            infoText.alpha = 0f;
        }
        else
        {
            Debug.LogError("UIManager: La referencia a infoText no está asignada.");
        }

        if (objectiveText != null)
        {

            UpdateObjectiveText(initialObjectiveText);
        }
        else
        {
            Debug.LogError("UIManager: La referencia a objectiveText no está asignada.");
        }
    }

    public void OnMessageEventRaised(string message)
    {
        if (infoText != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            infoText.DOKill();
            fadeCoroutine = StartCoroutine(FadeTextMessage(message));
        }
    }

    public void UpdateObjectiveText(string message)
    {
        if (objectiveText == null) return;

        objectiveText.DOKill();
        typingTween?.Kill();

        objectiveText.maxVisibleCharacters = 0;
        objectiveText.text = message;

        typingTween = DOTween.To(() => objectiveText.maxVisibleCharacters,
                                 x => objectiveText.maxVisibleCharacters = x,
                                 message.Length,
                                 message.Length * typingSpeed)
                               .SetEase(Ease.Linear);
    }

    private IEnumerator FadeTextMessage(string message)
    {
        infoText.text = message;
        infoText.DOFade(0f, 0f);
        yield return infoText.DOFade(1f, fadeInTime).WaitForCompletion();

        yield return new WaitForSeconds(displayTime);

        yield return infoText.DOFade(0f, fadeOutTime).WaitForCompletion();
    }

    public void FadeObjectiveOut(float duration = 0.5f)
    {
        if (objectiveText == null) return;
        objectiveText.DOKill();
        typingTween?.Kill();
        objectiveText.DOFade(0f, duration);
    }

    public void FadeObjectiveIn(float duration = 0.3f)
    {
        if (objectiveText == null) return;
        objectiveText.DOKill();
        typingTween?.Kill();
        objectiveText.DOFade(1f, duration);
    }

    // Muestra nuevo objetivo y lo oculta luego de unos segundos
    public void UpdateObjectiveTextAndFadeLater(string message, float holdSeconds = 2.5f, float fadeOut = 0.6f)
    {
        if (objectiveText == null) return;
        StopCoroutine(nameof(_UpdateAndFade));
        StartCoroutine(_UpdateAndFade(message, holdSeconds, fadeOut));
    }

    private System.Collections.IEnumerator _UpdateAndFade(string message, float holdSeconds, float fadeOut)
    {
        // Asegura visibilidad y escribe con tu efecto tipo máquina
        objectiveText.alpha = 1f;
        UpdateObjectiveText(message);
        yield return new WaitForSeconds(holdSeconds);
        FadeObjectiveOut(fadeOut);
    }
    public void ShowWindowInjuryOverlay()
    {
        if (windowInjuryOverlay == null)
        {
            Debug.LogWarning("UIManager.ShowWindowInjuryOverlay llamado pero no hay overlay asignado.");
            return;
        }

        if (overlayCoroutine != null)
        {
            StopCoroutine(overlayCoroutine);
        }

        overlayCoroutine = StartCoroutine(PlayWindowInjuryOverlay());
    }

    private IEnumerator PlayWindowInjuryOverlay()
    {
        // aseguramos alpha inicial 0
        windowInjuryOverlay.DOKill();
        Color c0 = windowInjuryOverlay.color;
        c0.a = 0f;
        windowInjuryOverlay.color = c0;

        // fade in rápido
        yield return windowInjuryOverlay.DOFade(1f, overlayFadeIn).WaitForCompletion();

        // mantener visible un ratito
        yield return new WaitForSeconds(overlayHold);

        // fade out
        yield return windowInjuryOverlay.DOFade(0f, overlayFadeOut).WaitForCompletion();

        overlayCoroutine = null;
    }
    public void RestoreInitialObjective()
    {
        UpdateObjectiveText(initialObjectiveText);
    }
}