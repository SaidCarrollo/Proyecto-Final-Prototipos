using UnityEngine;
using UnityEngine.AI;

public class AssistedControllerLvl2 : AssistedModeBase
{
    [Header("Referencias Player")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private FirstPersonController fpsController;
    [SerializeField] private Rigidbody rb;

    [Header("Navegación 3D")]
    public AssistedNode3D nodoInicial;
    private AssistedNode3D nodoActual;

    private bool estaViajando = false;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (agent != null) agent.enabled = false;

        // Buscar todos los nodos del nivel y decirles quién es el jefe
        AssistedNode3D[] todosLosNodos = FindObjectsByType<AssistedNode3D>(FindObjectsSortMode.None);
        foreach (var nodo in todosLosNodos)
        {
            nodo.Configurar(this);
        }
    }

    void Update()
    {
        // Chequear llegada
        if (estaViajando && agent.enabled && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    LlegarAlDestino();
                }
            }
        }
    }

    public override void ActivarModoAsistido()
    {
        if (fpsController != null) fpsController.SetAssistedMode(true);

        rb.isKinematic = true;
        agent.enabled = true;
        agent.Warp(transform.position);

        if (nodoActual == null && nodoInicial != null)
        {
            ViajarANodo(nodoInicial, true); // Viaje instantáneo (teleport) al inicio
        }
        else
        {
            ActualizarNodosVisibles();
        }
    }

    public void ViajarANodo(AssistedNode3D destino, bool teleport = false)
    {
        if (estaViajando) return;

        // 1. Apagamos TODOS los botones visuales para limpiar la pantalla durante el viaje
        ApagarTodosLosNodosVecinos();

        if (teleport)
        {
            agent.Warp(destino.puntoDeDestino != null ? destino.puntoDeDestino.position : destino.transform.position);
            nodoActual = destino;
            ActualizarNodosVisibles();
        }
        else
        {
            estaViajando = true;
            agent.SetDestination(destino.puntoDeDestino != null ? destino.puntoDeDestino.position : destino.transform.position);
            nodoActual = destino; // Lo guardamos como futuro actual
        }
    }

    private void LlegarAlDestino()
    {
        estaViajando = false;
        ActualizarNodosVisibles();
    }

    private void ActualizarNodosVisibles()
    {
        if (nodoActual == null) return;

        // Cuando llegamos a un lugar, ENCENDEMOS los botones de sus vecinos
        foreach (var vecino in nodoActual.nodosVecinos)
        {
            if (vecino != null)
            {
                vecino.EncenderNodo();
            }
        }
    }

    private void ApagarTodosLosNodosVecinos()
    {
        if (nodoActual != null)
        {
            foreach (var vecino in nodoActual.nodosVecinos)
            {
                if (vecino != null) vecino.ApagarNodo();
            }
        }
    }
}