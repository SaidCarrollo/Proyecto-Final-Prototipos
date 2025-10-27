// LastPlayedLevelSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "LastPlayedLevel", menuName = "Scene Management/Last Played Level")]
public class LastPlayedLevelSO : ScriptableObject
{
    [Tooltip("Se setea autom�ticamente cuando sales a jugar un nivel desde un bot�n.")]
    public SceneDefinitionSO lastLevel;
}
