using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;

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

    private Coroutine fadeCoroutine;
    private Tween typingTween;

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
}