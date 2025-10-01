using UnityEngine;

public class AbrirEnlace : MonoBehaviour
{
    [Tooltip("La URL completa que se abrir� (ej: https://www.google.com)")]
    [SerializeField] private string url = "";

    // Este es el m�todo P�BLICO que llamaremos desde el evento OnClick del bot�n.
    public void AbrirLink()
    {
        // Primero, comprobamos que el campo de la URL no est� vac�o.
        if (!string.IsNullOrEmpty(url))
        {
            // Application.OpenURL abre la URL en el navegador por defecto del usuario.
            Application.OpenURL(url);
        }
        else
        {
            // Si est� vac�o, mostramos una advertencia en la consola para ayudar a depurar.
            Debug.LogWarning("La URL no ha sido asignada en el Inspector para el bot�n: " + gameObject.name);
        }
    }
}