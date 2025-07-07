// ResultadosDelQuizSO.cs
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(fileName = "Resultados del Quiz", menuName = "Cuestionario/Resultados del Quiz")]
public class ResultadosDelQuizSO : ScriptableObject
{
    public List<ResultadoPregunta> resultados = new List<ResultadoPregunta>();

    [System.Serializable]
    public class ResultadosWrapper
    {
        public List<ResultadoPregunta> resultados;
    }

    public void LimpiarResultados()
    {
        resultados.Clear();
    }

    public void GuardarResultados()
    {
        ResultadosWrapper wrapper = new ResultadosWrapper { resultados = this.resultados };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(GetRutaGuardado(), json);
    }

    public void CargarResultados()
    {
        string ruta = GetRutaGuardado();
        if (File.Exists(ruta))
        {
            string json = File.ReadAllText(ruta);
            ResultadosWrapper wrapper = JsonUtility.FromJson<ResultadosWrapper>(json);
            resultados = wrapper.resultados;
        }
        else
        {
            Debug.LogWarning("Archivo de resultados no encontrado.");
        }
    }

    string GetRutaGuardado()
    {
        return Path.Combine(Application.persistentDataPath, "resultadosQuiz.json");
    }
}
