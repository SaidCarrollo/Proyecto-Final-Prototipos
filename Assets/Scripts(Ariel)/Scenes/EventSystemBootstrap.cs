using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

[DefaultExecutionOrder(-10000)]
public class EventSystemBootstrap : MonoBehaviour
{
    [Header("Opcional")]
    [SerializeField] private bool makePersistent = false; // Por defecto: false (uno por escena)

    private void Awake()
    {
        EnsureEventSystem();

        // UI utilizable al entrar en Win/Lose o Pausa
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

#if ENABLE_INPUT_SYSTEM
        // (Opcional pero recomendado) Forzar Dynamic Update global por código.
        // También puedes configurarlo en Project Settings → Input System Package.
        try
        {
            if (InputSystem.settings != null &&
                InputSystem.settings.updateMode != InputSettings.UpdateMode.ProcessEventsInDynamicUpdate)
            {
                InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
            }
        }
        catch { /* versiones antiguas o settings nulos */ }
#endif
    }

    private void EnsureEventSystem()
    {
        var es = EventSystem.current;
        if (es == null)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));

#if ENABLE_INPUT_SYSTEM
            var module = go.AddComponent<InputSystemUIInputModule>();
            AssignDefaultUIActionsIfNeeded(module);
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            if (makePersistent) DontDestroyOnLoad(go);
        }
        else
        {
#if ENABLE_INPUT_SYSTEM
            var module = es.GetComponent<InputSystemUIInputModule>();
            if (module == null) module = es.gameObject.AddComponent<InputSystemUIInputModule>();
            AssignDefaultUIActionsIfNeeded(module);
#else
            if (es.GetComponent<StandaloneInputModule>() == null)
                es.gameObject.AddComponent<StandaloneInputModule>();
#endif
        }

        // Evita selección "stale"
        EventSystem.current?.SetSelectedGameObject(null);
    }

#if ENABLE_INPUT_SYSTEM
    private void AssignDefaultUIActionsIfNeeded(InputSystemUIInputModule module)
    {
        if (module == null) return;

        bool looksEmpty = true;
        try
        {
            // Si el asset o las refs principales no están, lo consideramos "vacío"
            looksEmpty = module.actionsAsset == null;
            if (!looksEmpty)
            {
                // Campos habituales; si todos faltan, es sospechoso.
                looksEmpty =
                    module.point == null &&
                    module.move == null &&
                    module.submit == null &&
                    module.cancel == null;
            }
        }
        catch
        {
            // En versiones con distintas APIs, forzamos asignación
            looksEmpty = true;
        }

        if (looksEmpty)
        {
            try { module.AssignDefaultActions(); }
            catch
            {
                Debug.LogWarning("[EventSystemBootstrap] No se pudieron asignar acciones UI por defecto. " +
                                 "Asigna un Input Actions asset al InputSystemUIInputModule en el inspector.");
            }
        }
    }
#endif
}
