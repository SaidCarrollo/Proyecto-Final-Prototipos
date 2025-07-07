// ResultadoPregunta.cs
using System.Collections.Generic;
[System.Serializable]
public class ResultadoPregunta
{
    public string textoPregunta;
    public List<string> textosRespuestas;
    public List<bool> respuestasCorrectas;
    public List<string> justificaciones;

    public int indiceRespuestaMarcada;

    public ResultadoPregunta(Pregunta pregunta, int indice)
    {
        textoPregunta = pregunta.textoPregunta;
        textosRespuestas = new List<string>();
        respuestasCorrectas = new List<bool>();
        justificaciones = new List<string>();

        foreach (var r in pregunta.respuestas)
        {
            textosRespuestas.Add(r.textoRespuesta);
            respuestasCorrectas.Add(r.esCorrecta);
            justificaciones.Add(r.justificacion);
        }

        indiceRespuestaMarcada = indice;
    }
}
