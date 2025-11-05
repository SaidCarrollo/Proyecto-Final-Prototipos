using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // <- IMPORTANTE para EventTrigger

public class VolumeButtonControl : MonoBehaviour
{
    [Header("Clave de volumen (PlayerPrefs/AudioManager)")]
    [Tooltip("Ej: 'MasterVolume', 'MusicVolume', 'SFXVolume'")]
    public string volumeKey;

    [Header("UI")]
    public Button minusButton;
    public Button plusButton;
    public RollingNumberDisplay numberDisplay;

    [Header("Configuración de rango")]
    [Tooltip("Valor mínimo en la UI (0 = mute)")]
    public int minValue = 0;
    [Tooltip("Valor máximo en la UI (100 = volumen completo)")]
    public int maxValue = 100;
    [Tooltip("Tamaño de paso por click (+/-)")]
    public int step = 5;

    [Header("Mantener presionado")]
    [Tooltip("Tiempo antes de que empiece el auto-repeat al mantener presionado (segundos)")]
    [SerializeField] private float holdInitialDelay = 0.4f;
    [Tooltip("Intervalo entre pasos mientras se mantiene presionado (segundos)")]
    [SerializeField] private float holdRepeatInterval = 0.08f;

    private int currentValue; // 0-100

    // Control de hold
    private bool isHolding = false;
    private int holdDirection = 0; // -1 = bajar, +1 = subir
    private Coroutine holdCoroutine;

    void Start()
    {
        if (numberDisplay == null)
        {
            Debug.LogError("[VolumeButtonControl] Asigna numberDisplay.");
            return;
        }

        if (string.IsNullOrEmpty(volumeKey))
        {
            Debug.LogWarning("[VolumeButtonControl] volumeKey vacío, asigna uno en el inspector.");
        }

        // Leer valor normalizado 0-1 del AudioManager y pasarlo a 0-100
        float normalized = AudioManager.Instance.GetVolume(volumeKey); // suponemos 0-1
        currentValue = Mathf.RoundToInt(normalized * maxValue);
        currentValue = Mathf.Clamp(currentValue, minValue, maxValue);
        numberDisplay.SetInstant(currentValue);

        // Click simple (un paso)
        if (minusButton != null)
        {
            minusButton.onClick.AddListener(OnMinusClicked);
            ConfigurarHold(minusButton, -1);
        }

        if (plusButton != null)
        {
            plusButton.onClick.AddListener(OnPlusClicked);
            ConfigurarHold(plusButton, +1);
        }
    }

    private void OnMinusClicked()
    {
        int newValue = Mathf.Clamp(currentValue - step, minValue, maxValue);
        ChangeVolume(newValue);
    }

    private void OnPlusClicked()
    {
        int newValue = Mathf.Clamp(currentValue + step, minValue, maxValue);
        ChangeVolume(newValue);
    }

    private void ChangeVolume(int newValue)
    {
        if (newValue == currentValue)
            return;

        currentValue = newValue;

        // Animar número (rodillo hacia arriba o abajo según si sube o baja)
        numberDisplay.AnimateTo(currentValue);

        // Convertir a 0-1 y mandar al AudioManager
        float normalized = (float)currentValue / maxValue;
        AudioManager.Instance.SetVolume(volumeKey, normalized, true);
    }

    #region HOLD LOGIC

    private void ConfigurarHold(Button button, int direction)
    {
        if (button == null) return;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // PointerDown -> empezar a preparar el hold
        EventTrigger.Entry entryDown = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        entryDown.callback.AddListener((data) =>
        {
            StartHold(direction);
        });
        trigger.triggers.Add(entryDown);

        // PointerUp -> parar hold
        EventTrigger.Entry entryUp = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        entryUp.callback.AddListener((data) =>
        {
            StopHold();
        });
        trigger.triggers.Add(entryUp);

        // PointerExit -> también parar (por si se sale del botón mientras mantiene)
        EventTrigger.Entry entryExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        entryExit.callback.AddListener((data) =>
        {
            StopHold();
        });
        trigger.triggers.Add(entryExit);
    }

    private void StartHold(int direction)
    {
        isHolding = true;
        holdDirection = direction;

        // Si ya hay una corrutina corriendo, la paramos
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
        }
        holdCoroutine = StartCoroutine(HoldRoutine());
    }

    private void StopHold()
    {
        isHolding = false;
        holdDirection = 0;
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }

    private System.Collections.IEnumerator HoldRoutine()
    {
        // Esperar un poco antes de empezar auto-repeat, para permitir taps normales
        yield return new WaitForSecondsRealtime(holdInitialDelay); // <- tiempo real

        while (isHolding)
        {
            if (holdDirection > 0)
            {
                OnPlusClicked();
            }
            else if (holdDirection < 0)
            {
                OnMinusClicked();
            }

            yield return new WaitForSecondsRealtime(holdRepeatInterval); // <- tiempo real
        }

        holdCoroutine = null;
    }

    #endregion
}
