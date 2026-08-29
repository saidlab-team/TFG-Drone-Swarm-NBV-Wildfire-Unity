using UnityEngine;
using System.Collections.Concurrent;

public class FireManager : MonoBehaviour 
{
    // Diccionario concurrente: Seguro para múltiples drones
    private ConcurrentDictionary<Vector3, int> assignedFires = new ConcurrentDictionary<Vector3, int>();

    public bool IsFireAssigned(Vector3 firePos) {
        return assignedFires.ContainsKey(firePos);
    }

    public bool TryAssignFire(Vector3 firePos, int droneID) {
        // TryAdd intenta añadirlo; si ya existe, devuelve false
        bool asignado = assignedFires.TryAdd(firePos, droneID);
        if (asignado) {
            Debug.Log($"Fuego en {firePos} asignado al Dron ID: {droneID}");
        }else {
            Debug.Log($"Fuego en {firePos} ya asignado a otro dron.");
        }
        return asignado;
    }

    public void UnassignFire(Vector3 firePos) {
        int removedID;
        assignedFires.TryRemove(firePos, out removedID);
    }
}