using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Mover a objetivo 2D",
    story: "[Agent] se mueve hacia [Target]",
    category: "Action/Ladron",
    id: "a1f3c9d27b6e4f0a8c15d3e7b9024a61")]
public partial class MoverAObjetivo2DAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Velocidad = new BlackboardVariable<float>(3f);
    [SerializeReference] public BlackboardVariable<float> DistanciaParada = new BlackboardVariable<float>(0.15f);

    private NavMeshAgent m_Agente;

    protected override Status OnStart()
    {
        if (Agent.Value == null || Target.Value == null) return Status.Failure;

        m_Agente = Agent.Value.GetComponent<NavMeshAgent>();
        if (m_Agente != null && m_Agente.isOnNavMesh)
        {
            m_Agente.isStopped = false;
            m_Agente.speed = Velocidad.Value;
            m_Agente.stoppingDistance = DistanciaParada.Value;
            m_Agente.SetDestination(Target.Value.transform.position);
        }
        else
        {
            m_Agente = null;
        }
        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (m_Agente != null && m_Agente.isOnNavMesh) m_Agente.ResetPath();
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value == null || Target.Value == null) return Status.Failure;

        if (m_Agente != null)
        {
            if (m_Agente.pathPending) return Status.Running;
            if (m_Agente.pathStatus == NavMeshPathStatus.PathInvalid) return Status.Failure;
            if (m_Agente.remainingDistance <= DistanciaParada.Value) return Status.Success;
            return Status.Running;
        }

        Transform t = Agent.Value.transform;
        Vector2 pos = t.position;
        Vector2 destino = Target.Value.transform.position;

        if (Vector2.Distance(pos, destino) <= DistanciaParada.Value)
            return Status.Success;

        Vector2 nueva = Vector2.MoveTowards(pos, destino, Velocidad.Value * Time.deltaTime);
        t.position = new Vector3(nueva.x, nueva.y, t.position.z);
        return Status.Running;
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Elegir punto aleatorio",
    story: "Elegir un punto al azar de [Lista] y guardarlo en [Destino]",
    category: "Action/Ladron",
    id: "b7d2e5a10c3f48b9961e2a4d8f5c7b03")]
public partial class ElegirPuntoAleatorioAction : Action
{
    [SerializeReference] public BlackboardVariable<List<GameObject>> Lista;
    [SerializeReference] public BlackboardVariable<GameObject> Destino;

    protected override Status OnStart()
    {
        if (Lista.Value == null || Lista.Value.Count == 0) return Status.Failure;
        Destino.Value = Lista.Value[UnityEngine.Random.Range(0, Lista.Value.Count)];
        return Status.Success;
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Elegir punto mas cercano",
    story: "[Agent] elige el punto mas cercano de [Lista] y lo guarda en [Destino]",
    category: "Action/Ladron",
    id: "c4e8a1f6d9b2473e85a0c3d7f1b92e54")]
public partial class ElegirPuntoMasCercanoAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<List<GameObject>> Lista;
    [SerializeReference] public BlackboardVariable<GameObject> Destino;

    protected override Status OnStart()
    {
        if (Agent.Value == null || Lista.Value == null || Lista.Value.Count == 0)
            return Status.Failure;

        GameObject mejor = null;
        float mejorDist = float.MaxValue;
        Vector2 pos = Agent.Value.transform.position;

        foreach (GameObject punto in Lista.Value)
        {
            if (punto == null) continue;
            float d = Vector2.Distance(pos, punto.transform.position);
            if (d < mejorDist)
            {
                mejorDist = d;
                mejor = punto;
            }
        }

        if (mejor == null) return Status.Failure;
        Destino.Value = mejor;
        return Status.Success;
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Buscar producto disponible",
    story: "[Agent] busca un producto disponible y lo guarda en [ProductoObjetivo]",
    category: "Action/Ladron",
    id: "d9a3b7c5e1f04268b4d6a8c2e0f7135a")]
public partial class BuscarProductoDisponibleAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> ProductoObjetivo;

    protected override Status OnStart()
    {
        if (Agent.Value == null) return Status.Failure;

        Producto[] productos = UnityEngine.Object.FindObjectsByType<Producto>(FindObjectsSortMode.None);
        Producto mejor = null;
        float mejorDist = float.MaxValue;
        Vector2 pos = Agent.Value.transform.position;

        foreach (Producto p in productos)
        {
            if (!p.Disponible) continue;
            float d = Vector2.Distance(pos, p.transform.position);
            if (d < mejorDist)
            {
                mejorDist = d;
                mejor = p;
            }
        }

        if (mejor == null) return Status.Failure;
        ProductoObjetivo.Value = mejor.gameObject;
        return Status.Success;
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Robar producto",
    story: "[Agent] roba [ProductoObjetivo] durante [Duracion] s y marca [TieneProducto]",
    category: "Action/Ladron",
    id: "e2c6f0a4b8d14975a3e7c1b5d9f82460")]
public partial class RobarProductoAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> ProductoObjetivo;
    [SerializeReference] public BlackboardVariable<float> Duracion = new BlackboardVariable<float>(2f);
    [SerializeReference] public BlackboardVariable<bool> TieneProducto;

    private float m_Timer;
    private LadronEstado m_Estado;
    private Producto m_Producto;

    protected override Status OnStart()
    {
        if (Agent.Value == null || ProductoObjetivo.Value == null) return Status.Failure;

        m_Producto = ProductoObjetivo.Value.GetComponent<Producto>();
        if (m_Producto == null || !m_Producto.Disponible) return Status.Failure;

        m_Estado = Agent.Value.GetComponent<LadronEstado>();
        if (m_Estado != null) m_Estado.Robando = true;

        m_Timer = 0f;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        m_Timer += Time.deltaTime;
        if (m_Timer < Duracion.Value) return Status.Running;

        m_Producto.Tomar();
        TieneProducto.Value = true;
        Debug.Log("Ladron: producto robado");
        return Status.Success;
    }

    protected override void OnEnd()
    {
        if (m_Estado != null) m_Estado.Robando = false;
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Esconderse hasta que sea seguro",
    story: "[Agent] se esconde hasta que [Guardia] este a mas de [DistanciaSegura]",
    category: "Action/Ladron",
    id: "f5b1d9c3a7e24086b2f4d8a0c6e31795")]
public partial class EsconderseHastaSeguroAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Guardia;
    [SerializeReference] public BlackboardVariable<float> DistanciaSegura = new BlackboardVariable<float>(5f);
    [SerializeReference] public BlackboardVariable<float> TiempoMinimo = new BlackboardVariable<float>(2f);

    private float m_Timer;
    private LadronEstado m_Estado;

    protected override Status OnStart()
    {
        if (Agent.Value == null || Guardia.Value == null) return Status.Failure;

        m_Estado = Agent.Value.GetComponent<LadronEstado>();
        if (m_Estado != null) m_Estado.Escondido = true;

        m_Timer = 0f;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        m_Timer += Time.deltaTime;

        float dist = Vector2.Distance(Agent.Value.transform.position, Guardia.Value.transform.position);
        if (m_Timer >= TiempoMinimo.Value && dist > DistanciaSegura.Value)
            return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (m_Estado != null) m_Estado.Escondido = false;
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Escapar con el botin",
    story: "[Agent] escapa con el botin, vuelve a [PuntoInicio] y limpia [TieneProducto]",
    category: "Action/Ladron",
    id: "0a7e3c9b5d1f4862a8c4e0b6d2f91375")]
public partial class EscaparConBotinAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> PuntoInicio;
    [SerializeReference] public BlackboardVariable<bool> TieneProducto;

    protected override Status OnStart()
    {
        if (Agent.Value == null) return Status.Failure;

        GameStats.RobosExitosos++;
        Debug.Log("Ladron escapo! Robos exitosos: " + GameStats.RobosExitosos);

        TieneProducto.Value = false;

        if (PuntoInicio.Value != null)
        {
            Vector3 p = PuntoInicio.Value.transform.position;
            Agent.Value.transform.position = new Vector3(p.x, p.y, Agent.Value.transform.position.z);
        }
        return Status.Success;
    }
}

[Serializable, GeneratePropertyBag]
[Condition(
    name: "Objetivo cerca",
    story: "[Agent] esta a menos de [Distancia] de [Objetivo]",
    category: "Conditions/Ladron",
    id: "1b8f4d0a6c2e4973b5d1f7a3e9c80246")]
public partial class ObjetivoCercaCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Distancia = new BlackboardVariable<float>(3f);
    [SerializeReference] public BlackboardVariable<GameObject> Objetivo;

    public override bool IsTrue()
    {
        if (Agent.Value == null || Objetivo.Value == null) return false;
        return Vector2.Distance(Agent.Value.transform.position, Objetivo.Value.transform.position) < Distancia.Value;
    }
}

[Serializable, GeneratePropertyBag]
[Condition(
    name: "Es verdadero",
    story: "[Valor] es verdadero",
    category: "Conditions/Ladron",
    id: "2c9a5e1b7d3f4084a6e2b8d4f0a13957")]
public partial class EsVerdaderoCondition : Condition
{
    [SerializeReference] public BlackboardVariable<bool> Valor;

    public override bool IsTrue()
    {
        return Valor.Value;
    }
}
