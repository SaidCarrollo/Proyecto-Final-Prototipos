using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    [Header("Datos del Lugar")]
    public string nombreDelLugar; // Ej: "Cocina", "Pasillo Central"

    [Header("Conexiones (Lista Doble)")]
    [Tooltip("Arrastra aquí el nodo que queda a la IZQUIERDA de este.")]
    public WaypointNode nodoIzquierda;

    [Tooltip("Arrastra aquí el nodo que queda a la DERECHA de este.")]
    public WaypointNode nodoDerecha;

    // Dibujo visual para que veas las conexiones en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f);

        if (nodoIzquierda != null)
        {
            Gizmos.color = Color.red; // Rojo hacia la izquierda
            Gizmos.DrawLine(transform.position, nodoIzquierda.transform.position);
        }

        if (nodoDerecha != null)
        {
            Gizmos.color = Color.green; // Verde hacia la derecha
            Gizmos.DrawLine(transform.position, nodoDerecha.transform.position);
        }
    }
}