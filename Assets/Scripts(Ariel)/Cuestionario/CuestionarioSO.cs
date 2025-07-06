using UnityEngine;

[CreateAssetMenu(fileName = "Nuevo Cuestionario", menuName = "Cuestionario/Crea un nuevo cuestionario")]
public class CuestionarioSO : ScriptableObject
{
    public string nombreDelCuestionario;
    public Pregunta[] preguntas;
}