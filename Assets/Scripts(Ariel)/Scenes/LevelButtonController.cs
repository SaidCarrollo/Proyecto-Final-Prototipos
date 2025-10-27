using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LevelButtonController : MonoBehaviour
{
    [Header("Configuraci�n del Nivel")]
    [Tooltip("La definici�n de la escena que este bot�n debe cargar.")]
    [SerializeField] private SceneDefinitionSO sceneToLoad;

    [Header("Memoria de �ltimo nivel")]
    [Tooltip("Asset que guarda el �ltimo nivel jugado para el bot�n Reintentar.")]
    [SerializeField] private LastPlayedLevelSO lastPlayedLevel;

    [Header("Canales de Eventos")]
    [SerializeField] private SceneLoadEventChannelSO sceneLoadChannel;
    [SerializeField] private SceneChannelSO activatePreloadedSceneChannel;

    [Header("Animaci�n")]
    [Tooltip("El componente gr�fico que parpadear�. Si se deja vac�o, buscar� o a�adir� un CanvasGroup en este objeto.")]
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
        // 1) Guardar el �ltimo nivel para el bot�n Reintentar
        if (lastPlayedLevel != null)
        {
            lastPlayedLevel.lastLevel = sceneToLoad;
        }
        else
        {
            Debug.LogWarning("[LevelButtonController] 'lastPlayedLevel' no asignado. El bot�n Retry no sabr� a qu� nivel volver.");
        }

        // 2) Asegurar tiempo normal (por si vienes de una pausa/pantalla)
        Time.timeScale = 1f;

        // 3) Animaci�n blink del bot�n (opcional)
        if (buttonCanvasGroup != null)
        {
            _blinkTween = buttonCanvasGroup
                .DOFade(blinkFadeValue, blinkDuration)
                .SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // 4) PRE-CARGA as�ncrona con tu sistema
        sceneLoadChannel.RaiseEvent(sceneToLoad, UnityEngine.SceneManagement.LoadSceneMode.Single, true);

        // 5) Peque�a espera en TIEMPO REAL (funciona aunque hayas pausado)
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
