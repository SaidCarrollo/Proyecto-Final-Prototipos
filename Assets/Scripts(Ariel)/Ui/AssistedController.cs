using UnityEngine;
using UnityEngine.AI; // Necesario para NavMesh
using UnityEngine.UI;
using TMPro;

public class AssistedController : AssistedModeBase
{
    [Header("Referencias Player")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private NavMeshAgent agent; // REFERENCIA NUEVA
    [SerializeField] private FirstPersonController fpsController;

    [Header("Configuración Inicial")]
    public WaypointNode nodoInicial;

    [Header("UI Botones")]
    [SerializeField] private Button botonIzquierda;
    [SerializeField] private TextMeshProUGUI textoIzquierda;
    [SerializeField] private Button botonDerecha;
    [SerializeField] private TextMeshProUGUI textoDerecha;

    // Estado interno
    private WaypointNode nodoActual;
    private bool estaViajando = false;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        // Por defecto, el Agente debe estar apagado para no molestar al FPS
        if (agent != null) agent.enabled = false;

        botonIzquierda.onClick.AddListener(IrIzquierda);
        botonDerecha.onClick.AddListener(IrDerecha);
    }

    void Update()
    {
        // Chequeamos si el agente ha llegado a su destino
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

        // --- CAMBIO DE CEREBRO ---
        // 1. Apagamos la física del Rigidbody para que el NavMesh mande
        rb.isKinematic = true;

        // 2. Encendemos el Agente
        agent.enabled = true;

        // 3. Teletransportar lógico (sincronizar agente con posición actual)
        // Esto evita que el agente crea que está en otro lado
        agent.Warp(transform.position);

        // Inicialización de Nodos
        if (nodoActual == null && nodoInicial != null)
        {
            // Si empezamos de cero, vamos directo al nodo inicial
            TeletransportarA(nodoInicial);
        }
        else
        {
            ActualizarBotones();
        }
    }

    // Función para salir del modo asistido (por si acaso)
    public void DesactivarModoAsistido()
    {
        agent.enabled = false;
        rb.isKinematic = false; // Devolvemos el control a la física
        // El FpsController se reactiva desde el GameManager normalmente
    }

    private void TeletransportarA(WaypointNode target)
    {
        nodoActual = target;
        agent.Warp(target.transform.position); // Warp es el teleport del NavMesh
        ActualizarBotones();
    }

    // --- LÓGICA DE MOVIMIENTO ---

    public void IrIzquierda()
    {
        if (estaViajando || nodoActual.nodoIzquierda == null) return;
        MoverA(nodoActual.nodoIzquierda);
    }

    public void IrDerecha()
    {
        if (estaViajando || nodoActual.nodoDerecha == null) return;
        MoverA(nodoActual.nodoDerecha);
    }

    private void MoverA(WaypointNode destino)
    {
        estaViajando = true;

        // Ocultar UI
        botonIzquierda.gameObject.SetActive(false);
        botonDerecha.gameObject.SetActive(false);

        // Mover el agente
        agent.SetDestination(destino.transform.position);

        // Guardamos la referencia para cuando lleguemos
        nodoActual = destino;
    }

    private void LlegarAlDestino()
    {
        estaViajando = false;
        ActualizarBotones();
    }

    // --- LÓGICA DE UI (Igual que antes) ---
    private void ActualizarBotones()
    {
        if (nodoActual == null) return;

        if (nodoActual.nodoIzquierda != null)
        {
            botonIzquierda.gameObject.SetActive(true);
            textoIzquierda.text = nodoActual.nodoIzquierda.nombreDelLugar;
        }
        else
        {
            botonIzquierda.gameObject.SetActive(false);
        }

        if (nodoActual.nodoDerecha != null)
        {
            botonDerecha.gameObject.SetActive(true);
            textoDerecha.text = nodoActual.nodoDerecha.nombreDelLugar;
        }
        else
        {
            botonDerecha.gameObject.SetActive(false);
        }
    }
}