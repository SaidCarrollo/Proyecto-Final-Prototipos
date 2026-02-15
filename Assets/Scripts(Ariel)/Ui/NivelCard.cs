using UnityEngine;
using UnityEngine.UI;

public class NivelCard : MonoBehaviour
{
    [Header("Configuración")]
    public string nombreDelNivel;

    [Header("UI")]
    public Toggle toggleJuegoLibre;

    private const string SUFIJO_PREF = "_AssistMode";
    private string _claveGuardado; // Variable para guardar la key y no recalcularla

    void Start()
    {
        if (string.IsNullOrEmpty(nombreDelNivel))
        {
            Debug.LogError($"[NivelCard] Falta nombre en: {gameObject.name}");
            return;
        }

        // 1. Calculamos la clave UNA sola vez al inicio
        _claveGuardado = nombreDelNivel + SUFIJO_PREF;

        // 2. Leemos
        int estadoGuardado = PlayerPrefs.GetInt(_claveGuardado, 0);

        // 3. Actualizamos UI sin disparar evento
        toggleJuegoLibre.SetIsOnWithoutNotify(estadoGuardado == 1);

        // 4. Suscribimos evento
        toggleJuegoLibre.onValueChanged.AddListener(GuardarPreferencia);
    }

    private void GuardarPreferencia(bool estaActivo)
    {
        // Ya usamos la variable privada, no hace falta volver a sumar strings
        int valor = estaActivo ? 1 : 0;
        PlayerPrefs.SetInt(_claveGuardado, valor);
        PlayerPrefs.Save();

        Debug.Log($"Preferencia guardada: {_claveGuardado} = {valor}");
    }

    void OnDestroy()
    {
        if (toggleJuegoLibre != null)
            toggleJuegoLibre.onValueChanged.RemoveListener(GuardarPreferencia);
    }
}