// ResultadoPregunta.cs
[System.Serializable]
public class ResultadoPregunta
{
    public Pregunta preguntaOriginal;
    public int indiceRespuestaMarcada; // Guardamos el �ndice (0, 1, 2, o 3) de la respuesta que eligi� el jugador

    public ResultadoPregunta(Pregunta pregunta, int indice)
    {
        preguntaOriginal = pregunta;
        indiceRespuestaMarcada = indice;
    }
}