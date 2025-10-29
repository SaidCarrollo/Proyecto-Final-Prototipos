using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class RetryLevelButton : MonoBehaviour
{
    [Header("Comportamiento")]
    [Tooltip("Si está activo, recarga esta escena (thisLevel) con el sistema asíncrono. Úsalo en el menú de Pausa.")]
    [SerializeField] private bool reloadCurrentScene = false;

    [Tooltip("La SceneDefinition de ESTA escena (la actual). Recomendado asignarla si usarás reloadCurrentScene en Pausa.")]
    [SerializeField] private SceneDefinitionSO thisLevel;

    [Header("Memoria de último nivel (para Win/Lose)")]
    [SerializeField] private LastPlayedLevelSO lastPlayedLevel;

    [Header("Sistema de escenas (tus canales)")]
    [SerializeField] private SceneLoadEventChannelSO sceneLoadChannel;
    [SerializeField] private SceneChannelSO activatePreloadedSceneChannel;

    [Header("UI (opcional)")]
    [SerializeField] private Button button;

    private void Awake()
    {
        if (button != null) button.onClick.AddListener(OnRetryClicked);
    }

    public void OnRetryClicked()
    {
        // Normaliza timeScale (por si estabas pausado)
        Time.timeScale = 1f;

        // Determinar target según el modo
        SceneDefinitionSO target = null;

        if (reloadCurrentScene)
        {
            // Prioriza thisLevel (asignado en cada escena de gameplay)
            if (thisLevel != null)
            {
                target = thisLevel;
            }
            else if (lastPlayedLevel != null && lastPlayedLevel.lastLevel != null)
            {
                // Fallback: si no asignaste thisLevel, usa el último jugado
                target = lastPlayedLevel.lastLevel;
                Debug.LogWarning("[RetryLevelButton] reloadCurrentScene activo, pero 'thisLevel' no asignado. Usando lastPlayedLevel como fallback.");
            }
        }
        else
        {
            // Modo Win/Lose clásico: volver al último nivel jugado
            if (lastPlayedLevel != null && lastPlayedLevel.lastLevel != null)
            {
                target = lastPlayedLevel.lastLevel;
            }
            else if (thisLevel != null)
            {
                // Fallback: por si quieres que Win/Lose también pueda recargar la actual si no hubo registro previo
                target = thisLevel;
                Debug.LogWarning("[RetryLevelButton] No hay lastPlayedLevel registrado. Usando 'thisLevel' como fallback.");
            }
        }

        if (target == null)
        {
            Debug.LogWarning("[RetryLevelButton] No hay SceneDefinition objetivo (thisLevel/lastPlayedLevel). Deshabilitando botón.");
            if (button != null) button.interactable = false;
            return;
        }

        StartCoroutine(RetrySequence(target));
    }

    private IEnumerator RetrySequence(SceneDefinitionSO target)
    {
        // PRELOAD (Single) con tu sistema
        sceneLoadChannel.RaiseEvent(target, LoadSceneMode.Single, true);
        yield return new WaitForSecondsRealtime(0.1f);
        // ACTIVATE (dispara fade + allowSceneActivation)
        activatePreloadedSceneChannel.RaiseEvent();
    }
}
