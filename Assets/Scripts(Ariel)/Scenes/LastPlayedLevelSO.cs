// LastPlayedLevelSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "LastPlayedLevel", menuName = "Scene Management/Last Played Level")]
public class LastPlayedLevelSO : ScriptableObject
{
    [Tooltip("Se setea automáticamente cuando sales a jugar un nivel desde un botón.")]
    public SceneDefinitionSO lastLevel;
}
