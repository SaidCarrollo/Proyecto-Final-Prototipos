using UnityEngine;
using UnityEngine.UI;

public class GenericEnableDisableObject : MonoBehaviour
{
    // Esta función no cambia. Activar siempre es instantáneo para que OnEnable se dispare.
    public void SetGameObjectActive(GameObject targetObject)
    {
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }

    // ¡Esta es la función modificada y mejorada!
    public void SetGameObjectInactive(GameObject targetObject)
    {
        if (targetObject == null) return;

        // Intentamos obtener el script de animación del objeto.
        PanelAnimado panelAnimado = targetObject.GetComponent<PanelAnimado>();

        // Comprobamos si el script existe en el objeto.
        if (panelAnimado != null)
        {
            // Si existe, le pedimos que inicie su secuencia de cierre.
            // El propio script se encargará de hacer SetActive(false) al final.
            panelAnimado.CerrarPanel();
        }
        else
        {
            // Si no tiene el script de animación, lo desactivamos de forma abrupta como antes.
            // Así mantenemos la compatibilidad con objetos que no necesitan animación.
            targetObject.SetActive(false);
        }
    }
}