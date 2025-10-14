using UnityEngine;

public class ObjectProjection : MonoBehaviour
{
    public GameObject projectionPrefab; // Arrastra aqu� tu prefab de proyecci�n
    private GameObject projectionInstance; // La instancia de la proyecci�n en la escena
    public float maxDistance = 100f; // Distancia m�xima del raycast

    void Start()
    {
        // Creamos la instancia de la proyecci�n cuando el objeto es levantado
        projectionInstance = Instantiate(projectionPrefab);
    }

    void Update()
    {
        // Lanzamos un raycast desde la posici�n del objeto hacia abajo
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxDistance))
        {
            // Si el rayo choca con algo, activamos y movemos la proyecci�n
            projectionInstance.SetActive(true);
            projectionInstance.transform.position = hit.point; // Posicionamos la proyecci�n en el punto de impacto

            // Opcional: Alinear la rotaci�n con la superficie
            projectionInstance.transform.rotation = Quaternion.LookRotation(transform.forward, hit.normal);
        }
        else
        {
            // Si no choca con nada, desactivamos la proyecci�n
            projectionInstance.SetActive(false);
        }
    }

    // Llama a este m�todo cuando el objeto es soltado para destruir la proyecci�n
    public void OnObjectDropped()
    {
        Destroy(projectionInstance);
    }

    // Si el objeto principal se destruye, tambi�n lo hace la proyecci�n
    void OnDestroy()
    {
        if (projectionInstance != null)
        {
            Destroy(projectionInstance);
        }
    }
}