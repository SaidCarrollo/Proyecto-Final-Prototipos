using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Referencia al objeto de texto para mostrar mensajes informativos.")]
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Animation Settings")]
    [Tooltip("Tiempo que tarda el texto en aparecer (fade-in).")]
    [SerializeField] private float fadeInTime = 0.5f;

    [Tooltip("Tiempo que el mensaje permanece visible en pantalla con alfa completo.")]
    [SerializeField] private float displayTime = 3f;

    [Tooltip("Tiempo que tarda el texto en desaparecer (fade-out).")]
    [SerializeField] private float fadeOutTime = 0.5f;

    private Coroutine fadeCoroutine;

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
    }

    public void OnMessageEventRaised(string message)
    {
        if (infoText != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeTextMessage(message));
        }
    }

    private IEnumerator FadeTextMessage(string message)
    {
        infoText.text = message;
        float elapsedTime = 0f;
        while (elapsedTime < fadeInTime)
        {
            elapsedTime += Time.deltaTime;
            infoText.alpha = Mathf.Clamp01(elapsedTime / fadeInTime);
            yield return null;
        }
        infoText.alpha = 1f;

        yield return new WaitForSeconds(displayTime);

        elapsedTime = 0f;
        while (elapsedTime < fadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            infoText.alpha = 1f - Mathf.Clamp01(elapsedTime / fadeOutTime);
            yield return null;
        }
        infoText.alpha = 0f;
    }
}