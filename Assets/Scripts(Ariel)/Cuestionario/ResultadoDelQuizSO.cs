// ResultadosDelQuizSO.cs (extracto con cambios mínimos)
using System;
using System.IO;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Resultados del Quiz", menuName = "Cuestionario/Resultados del Quiz")]
public class ResultadosDelQuizSO : ScriptableObject
{
    public System.Collections.Generic.List<ResultadoPregunta> resultados = new();

    [Header("Identidad de este cuestionario")]
    public string nombreNivel = "Fuego en la cocina"; // setea en Inspector
    public bool esPostGame = false;                    // setea en Inspector (PRE=false, POST=true)
    public string prefijo = "Respuestas usuario";      // fijo o configurable

    [Serializable] public class ResultadosWrapper { public System.Collections.Generic.List<ResultadoPregunta> resultados; }

    public void LimpiarResultados() => resultados.Clear();

    public void GuardarResultados()
    {
        var wrapper = new ResultadosWrapper { resultados = this.resultados };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(GetRutaGuardadoUnico(), json);
    }

    public void CargarResultados()
    {
        string path = GetUltimoArchivo();
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Debug.LogWarning("Archivo de resultados no encontrado (último por patrón).");
            resultados = new System.Collections.Generic.List<ResultadoPregunta>();
            return;
        }

        string json = File.ReadAllText(path);
        var wrapper = JsonUtility.FromJson<ResultadosWrapper>(json);
        resultados = wrapper?.resultados ?? new System.Collections.Generic.List<ResultadoPregunta>();
    }
    public void CargarResultadosDesdeArchivo(string rutaAbsoluta)
    {
        if (!File.Exists(rutaAbsoluta)) { Debug.LogWarning("No existe: " + rutaAbsoluta); return; }
        string json = File.ReadAllText(rutaAbsoluta);
        var wrapper = JsonUtility.FromJson<ResultadosWrapper>(json);
        resultados = wrapper.resultados;
    }
    string GetRutaGuardado()
    {
        string momento = esPostGame ? "postgame" : "pregame";
        string baseName = $"{prefijo} {momento} {nombreNivel}";

        // contador por baseName (persistente en PlayerPrefs)
        string key = $"RUN_COUNTER::{baseName}";
        int run = PlayerPrefs.GetInt(key, 0) + 1;
        PlayerPrefs.SetInt(key, run);
        PlayerPrefs.Save();

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"{baseName}_{run}_{timestamp}.json";
        return Path.Combine(Application.persistentDataPath, fileName);
    }
    string GetRutaGuardadoUnico()
    {
        string momento = esPostGame ? "postgame" : "pregame";
        string baseName = $"{prefijo} {momento} {nombreNivel}";

        // contador por baseName
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
        string baseName = $"{prefijo} {momento} {nombreNivel}_"; // ojo el guion bajo antes del contador
        string dir = Application.persistentDataPath;

        if (!Directory.Exists(dir)) return null;

        var files = Directory.GetFiles(dir, "*.json")
            .Where(p => Path.GetFileName(p).StartsWith(baseName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => File.GetLastWriteTimeUtc(p))
            .ToArray();

        return files.FirstOrDefault();
    }
}
