using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;


[RequireComponent(typeof(BehaviorGraphAgent))]
public class LadronConfigurador : MonoBehaviour
{
    public string NombreGuardia = "Policia";

    private void Start()
    {
        BehaviorGraphAgent agente = GetComponent<BehaviorGraphAgent>();

        Asignar(agente, "Guardia", GameObject.Find(NombreGuardia));
        Asignar(agente, "Salida", GameObject.Find("Salida"));
        Asignar(agente, "PuntoInicio", GameObject.Find("PuntoInicio"));
        AsignarLista(agente, "Puntos", Hijos("Ladron_Puntos"));
        AsignarLista(agente, "Escondites", Hijos("Escondites"));
    }

    private static List<GameObject> Hijos(string nombrePadre)
    {
        List<GameObject> lista = new List<GameObject>();
        GameObject padre = GameObject.Find(nombrePadre);
        if (padre == null)
        {
            Debug.LogWarning("LadronConfigurador: no encontre '" + nombrePadre + "'");
            return lista;
        }
        foreach (Transform t in padre.transform) lista.Add(t.gameObject);
        return lista;
    }

    private static void Asignar(BehaviorGraphAgent agente, string variable, GameObject valor)
    {
        if (valor == null)
        {
            Debug.LogWarning("LadronConfigurador: no encontre el objeto para '" + variable + "'");
            return;
        }
        if (!agente.SetVariableValue(variable, valor))
            Debug.LogWarning("LadronConfigurador: el Blackboard no tiene la variable '" + variable + "' (GameObject)");
    }

    private static void AsignarLista(BehaviorGraphAgent agente, string variable, List<GameObject> valor)
    {
        if (!agente.SetVariableValue(variable, valor))
            Debug.LogWarning("LadronConfigurador: el Blackboard no tiene la variable '" + variable + "' (List<GameObject>)");
    }
}
