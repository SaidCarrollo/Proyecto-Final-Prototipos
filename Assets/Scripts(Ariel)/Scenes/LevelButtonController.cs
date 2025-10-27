using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LevelButtonController : MonoBehaviour
{
    [Header("Configuración del Nivel")]
    [Tooltip("La definición de la escena que este botón debe cargar.")]
    [SerializeField] private SceneDefinitionSO sceneToLoad;

    [Header("Memoria de último nivel")]
    [Tooltip("Asset que guarda el último nivel jugado para el botón Reintentar.")]
    [SerializeField] private LastPlayedLevelSO lastPlayedLevel;

    [Header("Canales de Eventos")]
    [SerializeField] private SceneLoadEventChannelSO sceneLoadChannel;
    [SerializeField] private SceneChannelSO activatePreloadedSceneChannel;

    [Header("Animación")]
    [Tooltip("El componente gráfico que parpadeará. Si se deja vacío, buscará o añadirá un CanvasGroup en este objeto.")]
    [SerializeField] private CanvasGroup buttonCanvasGroup;
    [SerializeField] private float blinkFadeValue = 0.5f;
    [SerializeField] private float blinkDuration = 0.4f;

    private bool _isSelected = false;
    private Tween _blinkTween;

    private void Awake()
    {
        if (buttonCanvasGroup == null)
        {
            buttonCanvasGroup = GetComponent<CanvasGroup>();
            if (buttonCanvasGroup == null)
                buttonCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnLevelSelected()
    {
        if (_isSelected) return;

        if (sceneToLoad == null)
        {
            Debug.LogError("[LevelButtonController] No se ha asignado 'sceneToLoad'.");
            return;
        }

        _isSelected = true;
        StartCoroutine(LoadLevelSequence());
    }

    private System.Collections.IEnumerator LoadLevelSequence()
    {
        // 1) Guardar el último nivel para el botón Reintentar
        if (lastPlayedLevel != null)
        {
            lastPlayedLevel.lastLevel = sceneToLoad;
        }
        else
        {
            Debug.LogWarning("[LevelButtonController] 'lastPlayedLevel' no asignado. El botón Retry no sabrá a qué nivel volver.");
        }

        // 2) Asegurar tiempo normal (por si vienes de una pausa/pantalla)
        Time.timeScale = 1f;

        // 3) Animación blink del botón (opcional)
        if (buttonCanvasGroup != null)
        {
            _blinkTween = buttonCanvasGroup
                .DOFade(blinkFadeValue, blinkDuration)
                .SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // 4) PRE-CARGA asíncrona con tu sistema
        sceneLoadChannel.RaiseEvent(sceneToLoad, UnityEngine.SceneManagement.LoadSceneMode.Single, true);

        // 5) Pequeña espera en TIEMPO REAL (funciona aunque hayas pausado)
        yield return new WaitForSecondsRealtime(0.1f);

        // 6) ACTIVAR pre-cargada (dispara fade + allowSceneActivation)
        activatePreloadedSceneChannel.RaiseEvent();
    }

    private void OnDestroy()
    {
        if (_blinkTween != null && _blinkTween.IsActive())
            _blinkTween.Kill();
    }
}
