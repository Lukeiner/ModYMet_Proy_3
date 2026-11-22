using UnityEngine;
using UnityEngine.AI;

public static class GameStats
{
    public static int RobosExitosos;
    public static int Capturas;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        RobosExitosos = 0;
        Capturas = 0;
    }
}

public class LadronEstado : MonoBehaviour
{
    public bool Escondido;
    public bool Robando;
    public bool Capturado;

    public float TiempoReaparicion = 3f;
    [Tooltip("Segundos en que no lo pueden capturar al aparecer o reaparecer.")]
    public float TiempoProteccion = 2f;

    private float m_ProtegidoHasta;

    private Behaviour m_Grafo;
    private NavMeshAgent m_Agente;
    private SpriteRenderer m_Sprite;
    private Color m_ColorOriginal = Color.white;

    private void Awake()
    {
        m_Grafo = GetComponent("BehaviorGraphAgent") as Behaviour;
        m_Agente = GetComponent<NavMeshAgent>();
        m_Sprite = GetComponentInChildren<SpriteRenderer>();
        if (m_Sprite != null) m_ColorOriginal = m_Sprite.color;
        // Protegido desde el primer frame (el policia puede actualizar antes que nuestro Start).
        m_ProtegidoHasta = Time.time + TiempoProteccion;
    }

    private void Start()
    {
        // Arranca en la entrada (PuntoInicio) y no en el medio junto al policia.
        IrAPuntoInicio();
        m_ProtegidoHasta = Time.time + TiempoProteccion;
    }

    private void IrAPuntoInicio()
    {
        GameObject inicio = GameObject.Find("PuntoInicio");
        if (inicio == null) return;
        if (m_Agente != null && m_Agente.isOnNavMesh) m_Agente.Warp(inicio.transform.position);
        else transform.position = inicio.transform.position;
    }

    private void Update()
    {
        // Mientras esta escondido se ve mas oscuro, para que se note en la demo.
        if (m_Sprite == null || Capturado) return;
        m_Sprite.color = Escondido ? m_ColorOriginal * new Color(0.45f, 0.45f, 0.45f, 1f) : m_ColorOriginal;
    }

    public void Capturar()
    {
        if (Capturado || Time.time < m_ProtegidoHasta) return;

        Capturado = true;
        Escondido = false;
        Robando = false;
        GameStats.Capturas++;
        Debug.Log("Ladron capturado! Capturas: " + GameStats.Capturas);

        if (m_Grafo != null) m_Grafo.enabled = false;
        if (m_Agente != null && m_Agente.isOnNavMesh) { m_Agente.ResetPath(); m_Agente.isStopped = true; }
        if (m_Sprite != null) m_Sprite.color = new Color(m_ColorOriginal.r, m_ColorOriginal.g, m_ColorOriginal.b, 0.4f);

        Invoke(nameof(Reaparecer), TiempoReaparicion);
    }

    private void Reaparecer()
    {
        IrAPuntoInicio();
        m_ProtegidoHasta = Time.time + TiempoProteccion;

        if (m_Agente != null && m_Agente.isOnNavMesh) m_Agente.isStopped = false;
        if (m_Sprite != null) m_Sprite.color = m_ColorOriginal;

        Capturado = false;

        // Si lo atraparon con el producto, lo pierde.
        if (m_Grafo != null)
        {
            var agente = GetComponent<Unity.Behavior.BehaviorGraphAgent>();
            if (agente != null) agente.SetVariableValue("TieneProducto", false);
            m_Grafo.enabled = true;
            if (agente != null) agente.Restart();
        }
    }
}
