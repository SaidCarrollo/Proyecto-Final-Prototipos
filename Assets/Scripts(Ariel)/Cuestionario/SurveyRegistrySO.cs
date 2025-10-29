// SurveyRegistrySO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Cuestionario/Survey Registry")]
public class SurveyRegistrySO : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public SceneDefinitionSO escenaNivel;
        public ResultadosDelQuizSO resultadosPre;
        public ResultadosDelQuizSO resultadosPost;
    }

    public Entry[] items;

    public ResultadosDelQuizSO GetResultados(SceneDefinitionSO escena, bool esPost)
    {
        foreach (var e in items)
            if (e.escenaNivel == escena) return esPost ? e.resultadosPost : e.resultadosPre;
        return null;
    }
}
