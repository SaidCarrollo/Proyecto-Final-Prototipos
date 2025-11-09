using UnityEngine;
using UnityEngine.Events; 

public class HazardTimer : MonoBehaviour
{
    [Header("Configuración de Tiempo")]
    [SerializeField] private float tiempoParaResultado = 30f;

    [Header("UI")]
    [SerializeField] private UITimerController uiTimerController;

    [Header("Eventos de Resultado")]
    [Tooltip("El evento que se dispara si el tiempo se acaba SIN que se haya desactivado el peligro.")]
    public UnityEvent OnTimerExpiredBadOutcome;

    [Tooltip("El evento que se dispara si el tiempo se acaba DESPUÉS de haber desactivado el peligro.")]
    public UnityEvent OnTimerExpiredGoodOutcome;

    [Tooltip("Evento que se dispara INMEDIATAMENTE al desactivar el peligro (ej. para dar un badge).")]
    public UnityEvent OnHazardDefused;

    [Header("Arranque")]
    [SerializeField] private bool autoStart = true;

    private bool isDefused = false;
    private Coroutine timerCoroutine;
    [SerializeField] private bool fastForwardToGoodOnDefuse = true; 
    private bool goodOutcomeTriggered = false;
    private void Start()
    {
        if (autoStart)
            StartTimer();
    }

    public void StartTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        isDefused = false; 
        timerCoroutine = StartCoroutine(Countdown());

        if (uiTimerController != null)
        {
            uiTimerController.StartFireTimer(tiempoParaResultado);
        }
    }

    private System.Collections.IEnumerator Countdown()
    {
        yield return new WaitForSeconds(tiempoParaResultado);

        if (isDefused)
        {
            Debug.Log("El tiempo ha terminado, pero el peligro fue neutralizado. ¡Buen resultado!");
            OnTimerExpiredGoodOutcome?.Invoke();
        }
        else
        {
            Debug.Log("El tiempo ha terminado y el peligro no fue neutralizado. ¡Mal resultado!");
            OnTimerExpiredBadOutcome?.Invoke();
        }
      
    }

    public void DefuseHazard()
    {
        if (isDefused) return;

        isDefused = true;
        Debug.Log("¡Peligro DESACTIVADO! El resultado del temporizador ahora será positivo.");
        OnHazardDefused?.Invoke();

        if (fastForwardToGoodOnDefuse)
            TriggerGoodOutcomeNow();
    }
    public void TriggerGoodOutcomeNow()
    {
        if (goodOutcomeTriggered) return;
        goodOutcomeTriggered = true;
        uiTimerController?.HideTimer();
        OnTimerExpiredGoodOutcome?.Invoke();
    }

}