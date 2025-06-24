using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening; 

public class UITimerController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Configuración de Colores")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = new Color(1f, 0.64f, 0f); // Naranja
    [SerializeField] private Color dangerColor = Color.red;

    [Header("Configuración de Tiempos y Animación")]
    [Tooltip("Segundos antes de que termine el timer para cambiar a color de advertencia.")]
    [SerializeField] private float warningThreshold = 5f;
    [Tooltip("La escala a la que crecerá el texto en el pulso.")]
    [SerializeField] private float pulseScale = 1.1f;
    [Tooltip("La duración de cada pulso (ida y vuelta).")]
    [SerializeField] private float pulseDuration = 0.5f;

    private Coroutine timerCoroutine;
    private Tween pulseTween;

    void Start()
    {
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false); 
        }
    }

    public void StartFireTimer(float duration)
    {
        StopCurrentTimer();
        timerText.gameObject.SetActive(true);
        timerText.color = normalColor;
        timerCoroutine = StartCoroutine(RunTimer(duration, true));
    }

    public void StartMortalTimer(float duration)
    {
        StopCurrentTimer();
        timerText.gameObject.SetActive(true);
        timerText.color = dangerColor;
        StartPulsing();
        timerCoroutine = StartCoroutine(RunTimer(duration, false));
    }
    public void HideTimer()
    {
        StopCurrentTimer();
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
    }

    private IEnumerator RunTimer(float duration, bool useWarningColor)
    {
        float remainingTime = duration;
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            if (useWarningColor && remainingTime <= warningThreshold)
            {
                timerText.color = warningColor;
            }

            timerText.text = remainingTime.ToString("0.0");
            yield return null;
        }

        timerText.text = "0.0";
    }

    private void StartPulsing()
    {
        pulseTween = timerText.transform
            .DOScale(pulseScale, pulseDuration / 2)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopCurrentTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        if (pulseTween != null && pulseTween.IsActive())
        {
            pulseTween.Kill();
            timerText.transform.localScale = Vector3.one; 
        }
    }
    private void OnDestroy()
    {
        pulseTween?.Kill();
    }
}