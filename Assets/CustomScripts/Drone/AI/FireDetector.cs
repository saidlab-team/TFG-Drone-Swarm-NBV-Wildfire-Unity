using UnityEngine;

public class FrustumCollider : MonoBehaviour
{
    private MeshCollider meshCollider;
    private Mesh frustumMesh;
    public Camera cam;
    public NPCWanderer wanderer;
    public metricsScript metricsManager;
    private int lastIndexX;
    private int lastIndexY;
    public LayerMask terrenoLayer;
    public bool isMainDrone = false;
    public FPVCam fPVCam;

    public float cooldownCelda = 3.0f;
    private float[,] proximoTiempoPermitido = new float[30, 30];
    public bool ppalComputaMetricas = false;

    void Awake()
    {
        //cam = GetComponent<Camera>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.convex = true;
        meshCollider.isTrigger = true;
        meshCollider.providesContacts = true;
        
    }
    void OnDrawGizmos()
    { 
        // Establecemos el color del Gizmo
        Gizmos.color = Color.yellow;

        // Obtenemos la matriz de la cámara para que el Gizmo se mueva y rote con ella<
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        // Dibujamos el frustum (campo de visión)
        // El 1.0f final es para la escala, normalmente se deja en 1
        Gizmos.DrawFrustum(Vector3.zero, cam.fieldOfView, cam.farClipPlane, cam.nearClipPlane, cam.aspect);
    }
    void Start()
    {
        metricsManager = FindObjectOfType<metricsScript>();
    }
    void Update()
    {
        // Esto genera una malla que sigue exactamente lo que ve la cámara
        meshCollider.sharedMesh = GenerateFrustumMesh();
        
        Transform camTransform = cam.transform;
        RaycastHit hit;

        if (metricsManager == null || metricsManager.gridVecesVisto == null) return;

        // Lanzamos raycast desde el centro de la cámara
        if (Physics.Raycast(camTransform.position, camTransform.forward, out hit, 200f, terrenoLayer))
        {
            int indiceX = (int)(hit.point.x + 300) / 20;
            int indiceZ = (int)(hit.point.z + 300) / 20; 

            // 1. Validamos que los índices estén dentro del rango de la rejilla (0 a 29)
            if (indiceX >= 0 && indiceX < 30 && indiceZ >= 0 && indiceZ < 30)
            {
                // 2. EL COOLDOWN: ¿El tiempo actual del juego ya superó el tiempo de espera de esta celda?
                if (Time.time >= proximoTiempoPermitido[indiceX, indiceZ])
                {
                    // Registramos la visita sumando directamente
                    if (ppalComputaMetricas) {
                        metricsManager.addVecesVisto(indiceX, indiceZ);
                    }
                    else
                    {
                        if (wanderer != null) {
                            metricsManager.addVecesVisto(indiceX, indiceZ);
                        }
                    }
                    
                    Debug.Log($"[Métricas] Celda ({indiceX}, {indiceZ}) registrada de forma segura. Visitas totales: {metricsManager.gridVecesVisto[indiceX, indiceZ]}");

                    // 3. Bloqueamos esta celda específica añadiendo el cooldown al tiempo actual
                    proximoTiempoPermitido[indiceX, indiceZ] = Time.time + cooldownCelda;
                }
            }
        }

    }

    Mesh GenerateFrustumMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[8];

        // 1. Obtener las 4 esquinas del plano CERCANO (Near)
        // CalculateFrustumCorners devuelve los puntos en espacio local de la cámara
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, vertices);

        // 2. Obtener las 4 esquinas del plano LEJANO (Far)
        // Necesitamos un array temporal porque la función sobreescribe
        Vector3[] farCorners = new Vector3[4];
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, farCorners);

        // Copiamos los puntos lejanos al array principal (índices 4 al 7)
        for (int i = 0; i < 4; i++)
        {
            vertices[i + 4] = farCorners[i];
        }

        mesh.vertices = vertices;

        // 3. Definir los triángulos (6 caras x 2 triángulos x 3 vértices = 36 índices)
        // El orden de los vértices importa para que las caras miren hacia afuera
        mesh.triangles = new int[]
        {
            // Cara Cercana (Near)
            0, 1, 2, 0, 2, 3,
            // Cara Lejana (Far)
            6, 5, 4, 7, 6, 4,
            // Cara Izquierda
            4, 5, 1, 4, 1, 0,
            // Cara Derecha
            3, 2, 6, 3, 6, 7,
            // Cara Superior
            1, 5, 6, 1, 6, 2,
            // Cara Inferior
            4, 0, 3, 4, 3, 7
        };

        mesh.RecalculateNormals(); // Para que la luz y las físicas detecten bien la dirección
        mesh.RecalculateBounds();

        return mesh;
    }
    Vector3 hitPoint;

    void OnTriggerEnter(Collider other)
    {
        if (!isMainDrone) {
            wanderer.HandleDetection(other);
        }
        
    }



    void OnTriggerStay(Collider other)
    {
        if (isMainDrone) {
            fPVCam.HandleDetection(other);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (isMainDrone) {
            fPVCam.HandleDetectionExit(other);
        }
    }
}