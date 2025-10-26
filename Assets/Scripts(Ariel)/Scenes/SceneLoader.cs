using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneLoader : MonoBehaviour
{
    [Header("Canales de Eventos")]
    [SerializeField] private SceneLoadEventChannelSO sceneLoadChannel;
    [SerializeField] private SceneChannelSO activatePreloadedSceneChannel;

    [Header("Transición")]
    [Tooltip("El CanvasGroup del panel negro que hará el fade")]
    [SerializeField] private CanvasGroup transitionCanvasGroup;
    [SerializeField] private float transitionDuration = 0.5f;

    private AsyncOperation _preloadedSceneOperation;
    private bool _isPreloading = false;
    private bool _isActivating = false;

    private void OnEnable()
    {
        if (sceneLoadChannel != null)
            sceneLoadChannel.OnSceneRequested += HandleSceneLoadRequest;

        if (activatePreloadedSceneChannel != null)
            activatePreloadedSceneChannel.OnEventRaised += ActivatePreloadedScene;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (sceneLoadChannel != null)
            sceneLoadChannel.OnSceneRequested -= HandleSceneLoadRequest;

        if (activatePreloadedSceneChannel != null)
            activatePreloadedSceneChannel.OnEventRaised -= ActivatePreloadedScene;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void HandleSceneLoadRequest(SceneDefinitionSO sceneToLoad, LoadSceneMode loadMode, bool isAsync)
    {
        if (isAsync)
        {
            if (_isPreloading)
            {
                Debug.LogWarning("Ya hay una escena pre-cargando.");
                return;
            }
            StartCoroutine(PreloadScene(sceneToLoad.scenePath, loadMode));
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad.scenePath, loadMode);
        }
    }

    private IEnumerator PreloadScene(string scenePath, LoadSceneMode loadMode)
    {
        _isPreloading = true;
        _preloadedSceneOperation = SceneManager.LoadSceneAsync(scenePath, loadMode);
        _preloadedSceneOperation.allowSceneActivation = false;

        while (_preloadedSceneOperation.progress < 0.9f)
            yield return null;

        Debug.Log($"Escena {scenePath} pre-cargada y lista para activar.");
    }

    private void ActivatePreloadedScene()
    {
        if (_preloadedSceneOperation == null || _isActivating)
        {
            Debug.LogWarning("No hay escena pre-cargada o ya se está activando.");
            return;
        }

        _isActivating = true;

        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.interactable = true;      // ? para bloquear clics durante el fade
            transitionCanvasGroup.blocksRaycasts = true;
            transitionCanvasGroup.DOKill();
            transitionCanvasGroup.DOFade(1f, transitionDuration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    // (opcional) Normaliza por si venías pausado DESDE el menú
                    Time.timeScale = 1f;                    // ? NUEVO failsafe
                    _preloadedSceneOperation.allowSceneActivation = true;
                });
        }
        else
        {
            Time.timeScale = 1f;                            // ? NUEVO failsafe
            _preloadedSceneOperation.allowSceneActivation = true;
        }
    }

    // Llamado cuando CUALQUIER escena termina de cargarse
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // === NORMALIZACIÓN CRÍTICA ===
        Time.timeScale = 1f;                                // ? NUEVO: evita “leaks” de pausa/slowmo

        _isPreloading = false;
        _isActivating = false;

        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.DOKill();
            transitionCanvasGroup.DOFade(0f, transitionDuration).SetUpdate(true);
            transitionCanvasGroup.interactable = false;
            transitionCanvasGroup.blocksRaycasts = false;
        }
    }
}
