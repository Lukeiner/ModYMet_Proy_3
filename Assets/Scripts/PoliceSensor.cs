using UnityEngine;
using Unity.Behavior;

public class PoliceSensor : MonoBehaviour
{
    public Transform ladron;
    public float rangoVision = 4f;
    public float rangoDetencion = 0.8f;

    private BehaviorGraphAgent agent;
    private bool loEstabaViendo = false;
    void Start()
    {
        agent = GetComponent<BehaviorGraphAgent>();
    }
    void Update()
    {
        if (ladron == null) return;

        float distancia = Vector2.Distance(transform.position, ladron.position);
        bool veAhora = distancia <= rangoVision;

        // Si lo estaba viendo y recién lo perdió de vista, guarda la pista
        if (loEstabaViendo && !veAhora)
        {
            agent.BlackboardReference.SetVariableValue("UltimaPosicionVista", ladron.position);
            agent.BlackboardReference.SetVariableValue("TienePista", true);
        }

        loEstabaViendo = veAhora;

        agent.BlackboardReference.SetVariableValue("VeAlLadron", veAhora);
        agent.BlackboardReference.SetVariableValue("PuedeDetener", distancia <= rangoDetencion);
    }

}
