using UnityEngine;

[CreateAssetMenu(
    fileName = "NextSceneHolder",
    menuName = "Scene Management/Next Scene Holder")]
public class NextSceneSO : ScriptableObject
{
    [Tooltip("Escena marcada como 'la próxima a la que deberíamos ir'")]
    public SceneDefinitionSO nextScene;
}
