using UnityEngine;
using UnityEngine.UI;

public class GenericEnableDisableObject : MonoBehaviour
{
    // Esta funci�n no cambia. Activar siempre es instant�neo para que OnEnable se dispare.
    public void SetGameObjectActive(GameObject targetObject)
    {
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }

    // �Esta es la funci�n modificada y mejorada!
    public void SetGameObjectInactive(GameObject targetObject)
    {
        if (targetObject == null) return;

        // Intentamos obtener el script de animaci�n del objeto.
        PanelAnimado panelAnimado = targetObject.GetComponent<PanelAnimado>();

        // Comprobamos si el script existe en el objeto.
        if (panelAnimado != null)
        {
            // Si existe, le pedimos que inicie su secuencia de cierre.
            // El propio script se encargar� de hacer SetActive(false) al final.
            panelAnimado.CerrarPanel();
        }
        else
        {
            // Si no tiene el script de animaci�n, lo desactivamos de forma abrupta como antes.
            // As� mantenemos la compatibilidad con objetos que no necesitan animaci�n.
            targetObject.SetActive(false);
        }
    }
}