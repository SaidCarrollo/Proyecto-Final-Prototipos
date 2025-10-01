using UnityEngine;
using UnityEngine.UI;

public class ScrollFondo : MonoBehaviour
{
    // Arrastra aqu� tu componente RawImage desde el Inspector
    [SerializeField] private RawImage imagenFondo;

    // Controla la velocidad de movimiento en los ejes X e Y
    [SerializeField] private float velocidadX = 0.05f;
    [SerializeField] private float velocidadY = 0f;

    void Update()
    {
        // Si no hay imagen asignada, no hagas nada.
        if (imagenFondo == null) return;

        // Obtenemos el rect�ngulo UV actual de la imagen.
        Rect rectActual = imagenFondo.uvRect;

        // Calculamos el nuevo desplazamiento.
        // Multiplicamos por Time.deltaTime para que el movimiento sea fluido
        // e independiente de los fotogramas por segundo (FPS).
        float offsetX = rectActual.x + velocidadX * Time.deltaTime;
        float offsetY = rectActual.y + velocidadY * Time.deltaTime;

        // Creamos un nuevo Rect con la nueva posici�n y el mismo tama�o.
        // El movimiento se logra al modificar la posici�n (el primer par�metro).
        imagenFondo.uvRect = new Rect(offsetX, offsetY, rectActual.width, rectActual.height);
    }
}