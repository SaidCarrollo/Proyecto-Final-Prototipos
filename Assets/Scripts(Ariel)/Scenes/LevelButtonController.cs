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

    [Header("Próxima escena planificada")]
    [Tooltip("Asset global donde el GameManager dejó guardada la 'siguiente escena' que deberíamos ir.")]
    [SerializeField] private NextSceneSO plannedNextScene;

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
        StartCoroutine(LoadLevelSequence(sceneToLoad, shouldRememberAsLast: true));
    }

    private System.Collections.IEnumerator LoadLevelSequence(SceneDefinitionSO targetScene, bool shouldRememberAsLast)
    {
        // 1) Guardar el último nivel para Retry (solo si aplica)
        if (shouldRememberAsLast)
        {
            if (lastPlayedLevel != null)
            {
                lastPlayedLevel.lastLevel = targetScene;
            }
            else
            {
                Debug.LogWarning("[LevelButtonController] 'lastPlayedLevel' no asignado. El botón Retry no sabrá a qué nivel volver.");
            }
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

        // 4) PRE-CARGA asíncrona con tu sistema central de escenas
        sceneLoadChannel.RaiseEvent(
            targetScene,
            UnityEngine.SceneManagement.LoadSceneMode.Single,
            true
        );

        // 5) Pequeña espera en TIEMPO REAL (funciona aunque hayas pausado)
        yield return new WaitForSecondsRealtime(0.1f);

        // 6) ACTIVAR pre-cargada (dispara fade + allowSceneActivation)
        activatePreloadedSceneChannel.RaiseEvent();
    }
    public void OnLoadPlannedNextScene()
    {
        if (_isSelected) return;

        if (plannedNextScene == null || plannedNextScene.nextScene == null)
        {
            Debug.LogWarning("[LevelButtonController] No hay escena planificada en 'plannedNextScene'.");
            return;
        }

        _isSelected = true;
        // OJO: aquí NO queremos sobreescribir lastPlayedLevel con la plannedNextScene
        // a menos que sí lo necesites. Te doy ambas opciones:
        //
        // Opción A (conservar retry del último nivel jugado real): shouldRememberAsLast:false
        // Opción B (actualizar retry a la nueva escena): shouldRememberAsLast:true
        //
        // Lo más común para 'Continuar historia' es A, así el Retry en pantallas de Win
        // sigue apuntando al nivel anterior, no al siguiente.
        StartCoroutine(LoadLevelSequence(plannedNextScene.nextScene, shouldRememberAsLast: false));
    }
    private void OnDestroy()
    {
        if (_blinkTween != null && _blinkTween.IsActive())
            _blinkTween.Kill();
    }
}
