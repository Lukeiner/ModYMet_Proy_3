using UnityEngine;
using Unity.Behavior;

public class PoliceSensor : MonoBehaviour
{
    public Transform ladron;
    public float rangoVision = 4f;
    public float rangoDetencion = 0.8f;

    private BehaviorGraphAgent agent;
    void Start()
    {
        agent = GetComponent<BehaviorGraphAgent>();
    }
    void Update()
    {
        if (ladron == null) return;

        float distancia = Vector2.Distance(transform.position, ladron.position);

        // Actualiza los booleanos del Blackboard automáticamente
        agent.BlackboardReference.SetVariableValue("VeAlLadron", distancia <= rangoVision);
        agent.BlackboardReference.SetVariableValue("PuedeDetener", distancia <= rangoDetencion);
    }

}
