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
            Debug.LogError("UIManager: La referencia a infoText no est� asignada.");
        }

        if (objectiveText != null)
        {
            UpdateObjectiveText("Apaga el fuego.");
        }
        else
        {
            Debug.LogError("UIManager: La referencia a objectiveText no est� asignada.");
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
}
