using UnityEngine;
using UnityEngine.AI; 

[RequireComponent(typeof(NavMeshAgent))]
public class NPCMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("El punto (GameObject vacío) al que se dirigirá el NPC tras ser salvado.")]
    [SerializeField] private Transform destinationPoint;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.enabled = false;
    }

    public void MoveToDestination()
    {
        if (destinationPoint == null)
        {
            Debug.LogError($"El NPC '{gameObject.name}' no tiene un 'destinationPoint' asignado en su script NPCMovement.", this);
            return;
        }
        agent.enabled = true;
        agent.SetDestination(destinationPoint.position);
        Debug.Log($"<color=lime>MOVIMIENTO:</color> El NPC '{gameObject.name}' se dirige a '{destinationPoint.name}'.");
    }
}