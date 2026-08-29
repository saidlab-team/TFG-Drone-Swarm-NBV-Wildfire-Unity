using UnityEngine;
using System;

public class EventTickExplorationRate : MonoBehaviour
{
    // El canal de radio estático donde todos escuchan
    public static event Action OnTickExplorationRate;

    public static void tickExplorationRate() {
        // El ?. asegura que si nadie escucha, no de error
        OnTickExplorationRate?.Invoke();
    }
}