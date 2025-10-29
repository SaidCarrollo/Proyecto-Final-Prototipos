using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    // Referencia al prefab de sistemas (puedes arrastrarlo en el inspector)
    [SerializeField] private GameObject systemsPrefab;
    // Referencia a la escena del Men� Principal
    [SerializeField] private SceneDefinitionSO mainMenuScene;
    [SerializeField] private SceneLoadEventChannelSO sceneLoadChannel;
    void Awake()
    {
        Application.targetFrameRate = 60; // o 120 si tu m�vil lo soporta
    }
    void Start()
    {
        // Instancia los sistemas
        Instantiate(systemsPrefab);

        // Una vez instanciados, espera un frame para asegurar que se suscriban a eventos
        // y luego carga el men� principal de forma s�ncrona y sin transici�n.
        // Usamos el mismo sistema de eventos para mantener la consistencia.
        sceneLoadChannel.RaiseEvent(mainMenuScene, UnityEngine.SceneManagement.LoadSceneMode.Single, false);
    }
}