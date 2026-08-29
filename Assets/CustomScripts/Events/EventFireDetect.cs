using UnityEngine;
using System;

public class EventFireDetect : MonoBehaviour
{
    // El canal de radio estático donde todos escuchan
    public static event Action<Vector3> OnFuegoDetectado;


    public static event Action<int> OnPedirAsistencia;
    public static event Action<int, Vector3> OnRespuestaAsistencia;

    public static void GritarFuego(Vector3 posicion) {
        // El ?. asegura que si nadie escucha, no de error
        OnFuegoDetectado?.Invoke(posicion);
    }

    // El dron perdido usa esto enviando su ID
    public static void SolicitarAyuda(int idSolicitante)
    {
        if (OnPedirAsistencia != null) OnPedirAsistencia.Invoke(idSolicitante);
    }

    // El compañero usa esto para responder a esa ID concreta
    public static void ResponderAsistencia(int idDestinatario, Vector3 posicionFuego)
    {
        if (OnRespuestaAsistencia != null) OnRespuestaAsistencia.Invoke(idDestinatario, posicionFuego);
    }
}