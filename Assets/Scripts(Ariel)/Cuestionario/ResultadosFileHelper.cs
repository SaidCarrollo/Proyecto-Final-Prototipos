// ResultadosFileHelper.cs
using System;
using System.IO;
using System.Linq;

public static class ResultadosFileHelper
{
    public static string GetUltimoArchivo(string prefijo, string nombreNivel, bool esPostGame)
    {
        string momento = esPostGame ? "postgame" : "pregame";
        string baseName = $"{prefijo} {momento} {nombreNivel}_";
        string dir = UnityEngine.Application.persistentDataPath;

        if (!Directory.Exists(dir)) return null;

        var files = Directory.GetFiles(dir, "*.json")
            .Where(p => Path.GetFileName(p).StartsWith(baseName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => File.GetLastWriteTimeUtc(p))
            .ToArray();

        return files.FirstOrDefault();
    }
}
