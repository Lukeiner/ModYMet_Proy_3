using UnityEngine;
using Unity.Behavior;
using UnityEngine.AI;

public class PoliceSensor : MonoBehaviour
{
    public Transform ladron;
    public float rangoVision = 4f;
    public float rangoDetencion = 0.8f;

    private BehaviorGraphAgent agent;
    private LadronEstado estadoLadron;
    private bool loEstabaViendo = false;
    private NavMeshAgent navAgent;
    private float velocidadInspector;

    void Start()
    {
        agent = GetComponent<BehaviorGraphAgent>();
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent != null) velocidadInspector = navAgent.speed;
        if (ladron != null) estadoLadron = ladron.GetComponent<LadronEstado>();
    }

    void Update()
    {
        if (ladron == null) return;

        float distancia = Vector2.Distance(transform.position, ladron.position);

        // Si el ladron esta escondido o ya fue capturado, el policia no lo ve.
        bool escondido = estadoLadron != null && (estadoLadron.Escondido || estadoLadron.Capturado);
        bool veAhora = distancia <= rangoVision && !escondido;

        // Si lo estaba viendo y recien lo perdio de vista, guarda la pista
        if (loEstabaViendo && !veAhora)
        {
            agent.BlackboardReference.SetVariableValue("UltimaPosicionVista", ladron.position);
            agent.BlackboardReference.SetVariableValue("TienePista", true);
        }

        loEstabaViendo = veAhora;

        bool puedeDetener = veAhora && distancia <= rangoDetencion;
        agent.BlackboardReference.SetVariableValue("VeAlLadron", veAhora);
        agent.BlackboardReference.SetVariableValue("PuedeDetener", puedeDetener);

        // Al alcanzarlo, lo captura (detiene el arbol del ladron).
        if (puedeDetener && estadoLadron != null) estadoLadron.Capturar();
    }

    void LateUpdate()
    {
        if (navAgent != null) navAgent.speed = velocidadInspector;
    }
}
