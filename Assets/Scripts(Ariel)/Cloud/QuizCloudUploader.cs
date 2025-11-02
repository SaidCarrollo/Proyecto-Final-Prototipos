// QuizCloudUploader.cs (versión corregida)
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;

public static class QuizCloudUploader
{
    public static async Task SubirResultadosAsync(ResultadosDelQuizSO so)
    {
        Debug.Log("[QuizCloudUploader] Inicio de subida...");

        try
        {
            // 1. Servicios
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                Debug.Log("[QuizCloudUploader] Inicializando UnityServices...");
                await UnityServices.InitializeAsync();
                Debug.Log("[QuizCloudUploader] UnityServices inicializado.");
            }

            // 2. Auth anónima
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("[QuizCloudUploader] Firmando anónimo...");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("[QuizCloudUploader] Sign in OK. PlayerID: " + AuthenticationService.Instance.PlayerId);
            }
            else
            {
                Debug.Log("[QuizCloudUploader] Ya estaba firmado. PlayerID: " + AuthenticationService.Instance.PlayerId);
            }

            // 3. Preparar datos
            string json = so.GetJsonActual();
            string momento = so.esPostGame ? "postgame" : "pregame";

            // SANEAR nombreNivel para que cumpla reglas de Cloud Save (solo A-Z a-z 0-9 - _)
            string safeLevel = SlugifyForCloudSave(so.nombreNivel);

            // armar key final
            string rawKey = $"quiz_{momento}_{safeLevel}_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}";

            // asegurar max 255
            if (rawKey.Length > 255)
                rawKey = rawKey.Substring(0, 255);

            var data = new Dictionary<string, object> {
                { rawKey, json }
            };

            Debug.Log($"[QuizCloudUploader] Subiendo a Cloud Save con key: {rawKey} (len={rawKey.Length})...");

            await CloudSaveService.Instance.Data.ForceSaveAsync(data);

            Debug.Log("[QuizCloudUploader] Subida COMPLETA. Key: " + rawKey);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError("[QuizCloudUploader] Error de autenticación: " + ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError("[QuizCloudUploader] Error de request UGS: " + ex);
        }
        catch (Exception ex)
        {
            Debug.LogError("[QuizCloudUploader] Error inesperado: " + ex);
        }
    }

    // ↓↓↓ Helper para limpiar nombres de nivel
    static string SlugifyForCloudSave(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "noname";

        // 1. quitar tildes y normalizar
        string normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        string noAccents = sb.ToString().Normalize(NormalizationForm.FormC);

        // 2. reemplazar espacios por _
        noAccents = noAccents.Replace(' ', '_');

        // 3. dejar solo A-Z a-z 0-9 _ -
        sb.Clear();
        foreach (char c in noAccents)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
            else if (c == '_' || c == '-')
            {
                sb.Append(c);
            }
            else
            {
                // cualquier otro símbolo lo pasamos a _
                sb.Append('_');
            }
        }

        string result = sb.ToString();
        // 4. por si queda vacío
        if (string.IsNullOrEmpty(result))
            result = "noname";

        return result;
    }
}
