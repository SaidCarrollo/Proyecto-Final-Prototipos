// ResultadosDelQuizSO.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resultados del Quiz", menuName = "Cuestionario/Resultados del Quiz")]
public class ResultadosDelQuizSO : ScriptableObject
{
    public List<ResultadoPregunta> resultados = new List<ResultadoPregunta>();

    public void LimpiarResultados()
    {
        resultados.Clear();
    }
}