using UnityEngine;

public class AbrirEnlace : MonoBehaviour
{
    [Tooltip("La URL completa que se abrirá (ej: https://www.google.com)")]
    [SerializeField] private string url = "";

    // Este es el método PÚBLICO que llamaremos desde el evento OnClick del botón.
    public void AbrirLink()
    {
        // Primero, comprobamos que el campo de la URL no esté vacío.
        if (!string.IsNullOrEmpty(url))
        {
            // Application.OpenURL abre la URL en el navegador por defecto del usuario.
            Application.OpenURL(url);
        }
        else
        {
            // Si está vacío, mostramos una advertencia en la consola para ayudar a depurar.
            Debug.LogWarning("La URL no ha sido asignada en el Inspector para el botón: " + gameObject.name);
        }
    }
}