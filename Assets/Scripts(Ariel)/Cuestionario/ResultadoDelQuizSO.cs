using System;
using System.IO;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Resultados del Quiz", menuName = "Cuestionario/Resultados del Quiz")]
public class ResultadosDelQuizSO : ScriptableObject
{
    public System.Collections.Generic.List<ResultadoPregunta> resultados = new();

    [Header("Identidad de este cuestionario")]
    public string nombreNivel = "Fuego en la cocina";
    public bool esPostGame = false;
    public string prefijo = "Respuestas usuario";

    [Serializable] public class ResultadosWrapper { public System.Collections.Generic.List<ResultadoPregunta> resultados; }

    public void LimpiarResultados()
    {
        resultados.Clear();
        Debug.Log($"[ResultadosDelQuizSO] Resultados limpiados para {nombreNivel} ({(esPostGame ? "POST" : "PRE")}).");
    }

    public void GuardarResultados()
    {
        var wrapper = new ResultadosWrapper { resultados = this.resultados };
        string json = JsonUtility.ToJson(wrapper, true);
        string ruta = GetRutaGuardadoUnico();
        File.WriteAllText(ruta, json);
        Debug.Log($"[ResultadosDelQuizSO] Guardado local OK en: {ruta}");
    }

    public void CargarResultadosDesdeArchivo(string rutaAbsoluta)
    {
        if (!File.Exists(rutaAbsoluta))
        {
            Debug.LogWarning("[ResultadosDelQuizSO] No existe archivo en: " + rutaAbsoluta);
            return;
        }

        string json = File.ReadAllText(rutaAbsoluta);
        var wrapper = JsonUtility.FromJson<ResultadosWrapper>(json);
        resultados = wrapper.resultados;
        Debug.Log($"[ResultadosDelQuizSO] Cargado desde archivo: {rutaAbsoluta}. Preguntas: {resultados.Count}");
    }

    string GetRutaGuardadoUnico()
    {
        string momento = esPostGame ? "postgame" : "pregame";
        string baseName = $"{prefijo} {momento} {nombreNivel}";
        string key = $"RUN_COUNTER::{baseName}";
        int run = PlayerPrefs.GetInt(key, 0) + 1;
        PlayerPrefs.SetInt(key, run);
        PlayerPrefs.Save();

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"{baseName}_{run}_{timestamp}.json";
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    string GetUltimoArchivo()
    {
        string momento = esPostGame ? "postgame" : "pregame";
        string baseName = $"{prefijo} {momento} {nombreNivel}_";
        string dir = Application.persistentDataPath;

        if (!Directory.Exists(dir)) return null;

        var files = Directory.GetFiles(dir, "*.json")
            .Where(p => Path.GetFileName(p).StartsWith(baseName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => File.GetLastWriteTimeUtc(p))
            .ToArray();

        return files.FirstOrDefault();
    }

    // NUEVO
    public string GetJsonActual()
    {
        var wrapper = new ResultadosWrapper { resultados = this.resultados };
        string json = JsonUtility.ToJson(wrapper, true);
        Debug.Log($"[ResultadosDelQuizSO] Json generado en memoria. Largo: {json.Length} chars. Nivel: {nombreNivel} ({(esPostGame ? "POST" : "PRE")})");
        return json;
    }
}
