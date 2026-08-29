using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AreaDetector : MonoBehaviour
{
    public LayerMask terrenoLayer;
    public LayerMask fuegoLayer;
    private LineRenderer lineRenderer;
    public Camera cam;

    void Start()
    {
        
        lineRenderer = GetComponent<LineRenderer>();
        
        // Configuración básica de la línea
        lineRenderer.positionCount = 5; // 4 esquinas + 1 para cerrar el ciclo
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        DibujarPerimetro();
        //Debug.Log(ContarObjetosEnCamara(fuegoLayer));

        
    }

    void DibujarPerimetro()
    {
        // Las 4 esquinas del viewport (0,0 es abajo-izq, 1,1 es arriba-der)
        Vector3[] esquinas = new Vector3[] {
            new Vector3(0, 0, cam.nearClipPlane),
            new Vector3(1, 0, cam.nearClipPlane),
            new Vector3(1, 1, cam.nearClipPlane),
            new Vector3(0, 1, cam.nearClipPlane)
        };

        for (int i = 0; i < 4; i++)
        {
            // Creamos un rayo que sale de la cámara hacia la esquina del FOV
            Ray ray = cam.ViewportPointToRay(esquinas[i]);
            RaycastHit hit;

            // Lanzamos el rayo. Ajusta el '1000' según la altura de tu dron
            if (Physics.Raycast(ray, out hit, 1000f, terrenoLayer))
            {
                lineRenderer.SetPosition(i, hit.point + Vector3.up * 0.1f); // Un poco elevado para evitar Z-fighting
            }
            else
            {
                // Si el rayo no toca suelo, proyectamos a una distancia fija
                lineRenderer.SetPosition(i, ray.GetPoint(50f));
            }
        }

        // El quinto punto es igual al primero para cerrar el cuadrado
        lineRenderer.SetPosition(4, lineRenderer.GetPosition(0));
    }
    public int ContarObjetosEnCamara(LayerMask layerObjetos)
    {
        // 1. Calculamos los 6 planos del frustum de la cámara
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        
        // 2. Buscamos todos los colliders en un radio amplio alrededor del dron
        // (Ajusta el 100f según el rango de tu cámara)
        Collider[] collidersCercanos = Physics.OverlapSphere(transform.position, 200f, layerObjetos);
        
        int contador = 0;

        foreach (Collider col in collidersCercanos)
        {
            //Debug.DrawRay(col.bounds.min, Vector3.up * col.bounds.size.y, Color.blue);
            // 3. Verificamos si los límites del objeto están dentro de los planos
            if (GeometryUtility.TestPlanesAABB(planes, col.bounds))
            {
                contador++;
                // Opcional: Dibujar una línea para confirmar que lo detectó
                //Debug.DrawLine(transform.position, col.transform.position, Color.red);
            }
        }

        return contador;
    }
    /*
    public struct CandidatoNBV {
    public Vector3 posicion;
    public Quaternion rotacion;
    public int puntuacion;
}

public CandidatoNBV EvaluarPosicion(Vector3 posSimulada, Quaternion rotSimulada, LayerMask layerObjetos) {
    // Calculamos la matriz de la cámara como si estuviera en la posición simulada
    Matrix4x4 matrix = Matrix4x4.TRS(posSimulada, rotSimulada, Vector3.one);
    
    // Obtenemos los planos del frustum para esa matriz
    // cam.projectionMatrix es fija, lo que cambia es la vista
    Plane[] planosSimulados = GeometryUtility.CalculateFrustumPlanes(cam.projectionMatrix * matrix.inverse);

    int objetosVistos = 0;
    Collider[] posibles = Physics.OverlapSphere(posSimulada, cam.farClipPlane, layerObjetos);

    foreach (var col in posibles) {
        if (GeometryUtility.TestPlanesAABB(planosSimulados, col.bounds)) {
            objetosVistos++;
        }
    }

    return new CandidatoNBV {
        posicion = posSimulada,
        rotacion = rotSimulada,
        puntuacion = objetosVistos
    };
}

------------------------------------

void PlanearSiguienteMovimiento() {
    CandidatoNBV mejorOpcion = new CandidatoNBV { puntuacion = -1 };
    
    // Simulamos 4 posiciones (Ej: Norte, Sur, Este, Oeste a 10 metros)
    Vector3[] direcciones = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
    
    foreach (Vector3 dir in direcciones) {
        Vector3 puntoASimular = transform.position + (dir * 10f);
        // Mirando hacia el centro o hacia donde apunta el dron
        Quaternion rotASimular = Quaternion.LookRotation(dir); 

        CandidatoNBV resultado = EvaluarPosicion(puntoASimular, rotASimular, miLayerMask);

        if (resultado.puntuacion > mejorOpcion.puntuacion) {
            mejorOpcion = resultado;
        }
    }

    Debug.Log($"La mejor posición es {mejorOpcion.posicion} con {mejorOpcion.puntuacion} objetos.");
    // Aquí mandas a tu Rigidbody hacia mejorOpcion.posicion
}


    */
}