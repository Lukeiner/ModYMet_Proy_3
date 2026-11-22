using UnityEngine;

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

    public void Capturar()
    {
        if (Capturado) return;

        Capturado = true;
        Escondido = false;
        Robando = false;
        GameStats.Capturas++;
        Debug.Log("Ladron capturado!");

        Behaviour grafo = GetComponent("BehaviorGraphAgent") as Behaviour;
        if (grafo != null) grafo.enabled = false;
    }
}
