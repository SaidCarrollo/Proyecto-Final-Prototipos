using UnityEngine;

public class ObjectProjection : MonoBehaviour
{
    public GameObject projectionPrefab; // Arrastra aquí tu prefab de proyección
    private GameObject projectionInstance; // La instancia de la proyección en la escena
    public float maxDistance = 100f; // Distancia máxima del raycast

    void Start()
    {
        // Creamos la instancia de la proyección cuando el objeto es levantado
        projectionInstance = Instantiate(projectionPrefab);
    }

    void Update()
    {
        // Lanzamos un raycast desde la posición del objeto hacia abajo
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxDistance))
        {
            // Si el rayo choca con algo, activamos y movemos la proyección
            projectionInstance.SetActive(true);
            projectionInstance.transform.position = hit.point; // Posicionamos la proyección en el punto de impacto

            // Opcional: Alinear la rotación con la superficie
            projectionInstance.transform.rotation = Quaternion.LookRotation(transform.forward, hit.normal);
        }
        else
        {
            // Si no choca con nada, desactivamos la proyección
            projectionInstance.SetActive(false);
        }
    }

    // Llama a este método cuando el objeto es soltado para destruir la proyección
    public void OnObjectDropped()
    {
        Destroy(projectionInstance);
    }

    // Si el objeto principal se destruye, también lo hace la proyección
    void OnDestroy()
    {
        if (projectionInstance != null)
        {
            Destroy(projectionInstance);
        }
    }
}