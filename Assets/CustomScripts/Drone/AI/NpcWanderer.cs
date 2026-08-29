using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class NPCWanderer : NPCComponent
{
    public Area area;
    bool fireDetected;
    //bool burnedDetected;
    Vector3 firePosition;
    public LayerMask fuegoLayer;

    public static event Action<Vector3> OnFuegoDetectado;
    public HashSet<Vector3> detectedColliderPositions = new HashSet<Vector3>();
    public EventFireDetect eventFireDetect;
    public EventTickExplorationRate eventTickExplorationRate;
    private int explorationRateCounter = 0;


    private metricsScript metricsManager;
    int paso = 0;

    //public HashSet<Vector3> detectedBurnedColliderPositions = new HashSet<Vector3>();
    void ResetExplorationRateCounter() {
        explorationRateCounter = 0;
    }
    void OnEnable()
    {
        EventFireDetect.OnFuegoDetectado += EscucharAlarma;
        EventTickExplorationRate.OnTickExplorationRate += saveExplorationRateData;
        EventFireDetect.OnPedirAsistencia += EscucharPeticionAyuda;
        EventFireDetect.OnRespuestaAsistencia += RecibirRespuestaAyuda;
    }

    void OnDisable()
    {
        EventFireDetect.OnFuegoDetectado -= EscucharAlarma;
        EventTickExplorationRate.OnTickExplorationRate -= saveExplorationRateData;
        EventFireDetect.OnPedirAsistencia -= EscucharPeticionAyuda;
        EventFireDetect.OnRespuestaAsistencia -= RecibirRespuestaAyuda;
    }

    private void saveExplorationRateData() {
        metricsManager.saveMetricRate(explorationRateCounter, transform.parent.name);
        ResetExplorationRateCounter();
    }
    private void VerFuego(Vector3 posicionFuego)
    {
        // Si hay alguien escuchando la radio, gritamos la posición
        Debug.Log("Avisando compañeros sobre el fuego en " + posicionFuego);
        EventFireDetect.GritarFuego(posicionFuego);
    }
    bool ayudaPedida = false;
    private void EscucharAlarma(Vector3 posicionFuego)
    {
        if (fireDetected) return; // Si ya hemos detectado un fuego, no cambiamos de objetivo por otro fuego que nos digan
        // Opcional: Para que el dron que gritó no reaccione a su propio grito
        if (Vector3.Distance(transform.position, posicionFuego) < 5f) return; 

        Debug.Log(gameObject.name + " escuchó la alarma. Cambiando rumbo.");
        // Código para ir hacia 'posicionFuego'
        StopDestination();
        area.MoveAreaCenter(posicionFuego);
        Vector3 tempPos = new Vector3(posicionFuego.x, transform.position.y, posicionFuego.z);
        SetDestination(tempPos);
        
    }
    private void EscucharPeticionAyuda(int idSolicitante)
    {
        // 1. Asegurarnos de que no nos estamos respondiendo a nosotros mismos
        // 2. Comprobar que actualmente sí estamos viendo un fuego
        if (this.gameObject.GetInstanceID() != idSolicitante && fireDetected)
        {
            Debug.Log(gameObject.name + " responde con coordenadas al dron ID: " + idSolicitante);
            EventFireDetect.ResponderAsistencia(idSolicitante, hitPoint);
        }
    }

    private void RecibirRespuestaAyuda(int idDestinatario, Vector3 posicionFuego)
    {
        // Solo hacemos caso si el mensaje va dirigido a nuestra ID concreta
        if (this.gameObject.GetInstanceID() == idDestinatario)
        {
            hitPoint = posicionFuego;
            Debug.Log(gameObject.name + " recibió ayuda de un compañero. Cambiando rumbo hacia: " + posicionFuego);
        }
    }

    void Start()
    {
        metricsManager = FindObjectOfType<metricsScript>();
        eventTickExplorationRate = FindObjectOfType<EventTickExplorationRate>();
        SetRandomDestination();
        fireDetected = false;
    }

    void Update()
    {
        if (!IsDroneReadyToMove()) return;
        
        if (fireDetected)
        {
            Debug.Log("NBV Activate");
            npc.camera.GetComponent<FPVCam>().setModoMonitoreo(true);
            VerFuego(hitPoint);
            HandleFireNBV();
            
        }/*
        else if (burnedDetected) {
            Debug.Log("Posicion dependiendo de otra");
            SetDestinationGivenLast(transform.position);
            npc.camera.GetComponent<FPVCam>().setModoFijo(true);
            burnedDetected = false;
        }*/
        else
        {
            Debug.Log("Random Location");
            SetRandomDestination();
            npc.camera.GetComponent<FPVCam>().setModoFijo(true);

        }

        
    }

    private HashSet<Collider> objetosYaVistos = new HashSet<Collider>();

    Vector3 hitPoint;
    bool enableDetection = true;
    public void HandleDetection(Collider other)
    {
        // Se llama cuando colisiona con un objeto fuego, solo 1 vez
        // Debug.Log("Collididng");
        // Debug.Log(other.tag);
        bool isFire = other.CompareTag("Fire");
        bool isBurned = other.CompareTag("Burned");

        if (!isFire && !isBurned) return;

        int indiceX = Mathf.FloorToInt((other.transform.position.x + 300f) / 20f);
        int indiceZ = Mathf.FloorToInt((other.transform.position.z + 300f) / 20f);

        if (metricsManager.addCeldaMonitoreadaGlobal(indiceX, indiceZ))
        {
            explorationRateCounter++;
        }

        if (isFire)
        {
            if (metricsManager.addCeldaFuegoGlobal(indiceX, indiceZ))
            {
                metricsManager.addCeldaMonitoreada(indiceX, indiceZ); 
            }
        }

        if (!enableDetection)
        {
            ActualizarHitPoint(other);
            return; // No estamos en modo de detección, ignoramos colisiones
        }

        if (other.CompareTag("Fire"))
        {
            if (!fireDetected)
            {
                // .Add devuelve true si lo añade, o false si ya existía
                if (!detectedColliderPositions.Add(other.transform.position)) 
                {
                    return; // Ya hemos detectado este fuego antes, no hacemos nada
                } 
                
                // va a una posicion cercana y empieza el NBV
                Debug.Log($"Ha detectado un fuego en la posicion: {other.transform.position}. Parando Navegacion");
                StopDestination();
                // Vector3 tempPos = new Vector3 (other.transform.position.x, transform.position.y, other.transform.position.z);
                // SetDestinationGivenLast(tempPos);
                hitPoint = other.transform.position;

                //burnedDetected = false;
                fireDetected= true;

                enableDetection = false;
                //SetRotation(hitPoint);
                detectedColliderPositions.Add(other.transform.position);
            }
            //Debug.Log(other.transform.position);
        }
        else if (other.CompareTag("Burned"))
        {
            if (!detectedColliderPositions.Add(other.transform.position)) 
            {
                return; // Ya hemos detectado este fuego antes, no hacemos nada
            } 
            //detectedColliderPositions.Add(other.transform.position);
            //burnedDetected = true;
            //StopDestination();
            area.MoveAreaCenter(other.transform.position);
            //SetDestinationGivenLast(transform.position);
        }
    }

    public void ActualizarHitPoint(Collider other) {
        if (other.CompareTag("Fire"))
        {
            hitPoint = other.transform.position;
        }
    }

//// Esta comprobación es instantánea O(1)
/// return fuegosRegistrados.Contains(posicion);
        
    void HandleFireNBV ()
    {
        //npc.camera.GetComponent<FPVCam>().setModoMonitoreo(true);
        Debug.Log("Handle Fire");
        CandidatoNBV mejorPos = PlanearSiguienteMovimiento();

        Matrix4x4 matrix = Matrix4x4.TRS(mejorPos.posicion, mejorPos.rotacion, Vector3.one);
        DibujarFrustumSimulado(matrix, npc.camera.projectionMatrix, mejorPos.posicion);

        SetDestination(mejorPos.posicion);
        SetRotation(mejorPos.rotacion);
        area.MoveAreaCenter(mejorPos.posicion);
        metricsManager.addGananciaInformacion(mejorPos.puntuacion);
        paso++;
    }


    // NBV

    public struct CandidatoNBV {
        public Vector3 posicion;
        public Quaternion rotacion;
        public int puntuacion;
    }

    public CandidatoNBV EvaluarPosicion(Vector3 posSimulada, Quaternion rotSimulada, LayerMask layerObjetos) {
        // Calculamos la matriz de la cámara como si estuviera en la posición simulada
        //Vector3 truePos = new Vector3(posSimulada.x, transform.position.y, posSimulada.z);
        Matrix4x4 matrix = Matrix4x4.TRS(posSimulada, rotSimulada, Vector3.one);
        
        //DibujarFrustumSimulado(matrix, npc.camera.projectionMatrix, posSimulada);
        // Obtenemos los planos del frustum para esa matriz
        // cam.projectionMatrix es fija, lo que cambia es la vista
        Plane[] planosSimulados = GeometryUtility.CalculateFrustumPlanes(npc.camera.projectionMatrix * matrix.inverse);

        int objetosVistos = 0;
        Collider[] posibles = Physics.OverlapSphere(posSimulada, npc.camera.farClipPlane, layerObjetos);

        Debug.DrawRay(posSimulada, rotSimulada * Vector3.forward * 5f, Color.red, 2.0f);
        int fuego= 0;
        int burned= 0;
        int preheat = 0;
        int puntuacion = 0;
        int penalizacion = 0;

        foreach (var col in posibles) {
            // if (detectedBurnedColliderPositions.Contains(col.transform.position)) 
            // {
            //     continue; // Ya hemos detectado este fuego o quemado antes, no lo contamos
            // }

            if (fireManager.IsFireAssigned(col.transform.position)) {
                continue; 
            }

        
            if (GeometryUtility.TestPlanesAABB(planosSimulados, col.bounds)) {
                if (col.CompareTag("Fire")) fuego++;
                else if (col.CompareTag("Burned")) burned++;
                else if (col.CompareTag("Preheat")) preheat++; 
                
                int indiceX = (int)(col.transform.position.x + 300) / 20;
                int indiceZ = (int)(col.transform.position.z + 300) / 20; 

                int vecesVisto = metricsManager.gridVecesVisto[indiceX, indiceZ];
                penalizacion += vecesVisto * 20;   
                //detectedBurnedColliderPositions.Add(col.transform.position);
                //objetosVistos++;
                


                Debug.Log($"Objeto visto en posición simulada: {col.gameObject.name} en {col.transform.position}, {col.gameObject.tag}");
            }
            
            // Minimizar la cantidad de burned que hay? porque no podemos detectar los preheat, 
        }

        puntuacion = (200*fuego)+ (100*preheat) - (20 * burned) - penalizacion; 

        
        return new CandidatoNBV {
            posicion = posSimulada,
            rotacion = rotSimulada,
            puntuacion = puntuacion
        };
    }
    
    public FireManager fireManager;
    int contadorSinDetectar = 0;
    public CandidatoNBV PlanearSiguienteMovimiento() {
        CandidatoNBV mejorOpcion = new CandidatoNBV { puntuacion = int.MinValue };
        

        //Vector3[] direcciones = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        Vector3[] direcciones = ObtenerCandidatosAlrededorDe(40f,20);

        
        foreach (Vector3 dir in direcciones) {
            //Vector3 puntoASimular = transform.position + dir;
            // Debug.Log($"La mejor posición es {puntoASimular}");
            // Mirando hacia el centro o hacia donde apunta el dron
            Quaternion rotASimular = Quaternion.LookRotation(dir - hitPoint);

            // Debug.DrawRay(dir, (hitPoint - dir) * 5f, Color.red, 2.0f);


            CandidatoNBV resultado = EvaluarPosicion(dir, rotASimular, fuegoLayer);

            if (resultado.puntuacion > mejorOpcion.puntuacion) {
                mejorOpcion = resultado;
                
            }
        }

        Debug.Log($"La mejor posición es {mejorOpcion.posicion} con {mejorOpcion.puntuacion} objetos. Rotación: {mejorOpcion.rotacion}, dron: {this.transform.parent.name}");
        Debug.DrawRay(mejorOpcion.posicion, mejorOpcion.rotacion * Vector3.forward * 5f, Color.green, 2.0f);

        if (mejorOpcion.puntuacion > 0) {
            // Re-calculamos el frustum solo para la posición y rotación ganadoras
            Matrix4x4 matrixGanadora = Matrix4x4.TRS(mejorOpcion.posicion, mejorOpcion.rotacion, Vector3.one);
            Plane[] planosGanadores = GeometryUtility.CalculateFrustumPlanes(npc.camera.projectionMatrix * matrixGanadora.inverse);
            Collider[] objetosGanadores = Physics.OverlapSphere(mejorOpcion.posicion, npc.camera.farClipPlane, fuegoLayer);
            bool hit_actualizado = false;
            foreach (var col in objetosGanadores) {
                if (GeometryUtility.TestPlanesAABB(planosGanadores, col.bounds)) {
                    if (col.CompareTag("Fire")) {
                        // Actualizamos el hitPoint con un fuego real que estamos VIENDO
                        hitPoint = col.transform.position; 
                        hit_actualizado = true;
                        contadorSinDetectar = 0;
                    }

                    // 1. Guardar en la memoria local de este dron
                    //detectedBurnedColliderPositions.Add(col.transform.position);

                    // 2. Asignar en el FireManager global para que los demás drones no vengan
                    fireManager.TryAssignFire(col.transform.position, this.gameObject.GetInstanceID());
                    //Debug.Log($"Objeto guardado en memoria y asignado globalmente:{this.gameObject.GetInstanceID()} --> {col.gameObject.name} en {col.transform.position}, {col.gameObject.tag}");
                }
            }
            if (!hit_actualizado) {
                contadorSinDetectar++;
                if (contadorSinDetectar > 5) {
                    EventFireDetect.SolicitarAyuda(this.gameObject.GetInstanceID());
                    contadorSinDetectar = 0; // Reiniciamos el contador para no pedir ayuda constantemente
                }
            }
        }
        
        return mejorOpcion;

    }

    public Vector3[] ObtenerCandidatosAlrededorDe(float radius, int cantidad) {
        Vector3[] candidatos = new Vector3[cantidad];
        Vector3 randomPoint;
        
        for (int i = 0; i < cantidad; i++) {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;

            // FORZAMOS que la Y sea positiva respecto al fuego (siempre por encima)
            // Usamos Mathf.Abs para que el desplazamiento vertical sea siempre hacia arriba
            float desplazamientY = Mathf.Abs(randomDirection.y) + 15f; // +2f para que no esté a ras de suelo

            candidatos[i] = new Vector3(hitPoint.x + randomDirection.x, hitPoint.y + desplazamientY, hitPoint.z + randomDirection.z);
            //Debug.Log($"Posicion de los candidatos: {candidatos[i]}");
        }
        
        return candidatos;
    }




    bool IsDroneReadyToMove()
    {
        //return npc.agent.remainingDistance <= npc.agent.stoppingDistance;
        return npc.movement.IsDroneReadyToMove();
    }
    void SetRandomDestination()
    {
        //npc.agent.SetDestination(area.GetRandomPoint());
        npc.movement.SetDestination(area.GetRandomPoint());
    }
    void SetDestination(Vector3 firePos)
    {
        npc.movement.SetDestination(firePos);
    }
    void SetRotation (Quaternion rot)
    {
        //npc.camera.transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
        //npc.camera.GetComponent<CameraFollowScript>().rot = rot;    
        npc.camera.GetComponent<FPVCam>().setLookAtRot(rot);  
    }
    void StopDestination()
    {
        npc.movement.StopDestination();
    }

    void SetDestinationGivenLast(Vector3 lasPos)
    {
        npc.movement.SetDestinationGivenLast(lasPos);
    }


    void DibujarFrustumSimulado(Matrix4x4 viewMatrix, Matrix4x4 projMatrix, Vector3 truePos)
    {
        Matrix4x4 clipToWorld = (projMatrix * viewMatrix.inverse).inverse;
        Vector3[] esquinas = {
            clipToWorld.MultiplyPoint(new Vector3(-1, -1, 1)), // Inferior Izquierda (Far)
            clipToWorld.MultiplyPoint(new Vector3(1, -1, 1)),  // Inferior Derecha (Far)
            clipToWorld.MultiplyPoint(new Vector3(1, 1, 1)),   // Superior Derecha (Far)
            clipToWorld.MultiplyPoint(new Vector3(-1, 1, 1))   // Superior Izquierda (Far)
        };

        // Dibujar el rectángulo del fondo (Far Plane)
        Debug.DrawLine(esquinas[0], esquinas[1], Color.yellow, 2f);
        Debug.DrawLine(esquinas[1], esquinas[2], Color.yellow, 2f);
        Debug.DrawLine(esquinas[2], esquinas[3], Color.yellow, 2f);
        Debug.DrawLine(esquinas[3], esquinas[0], Color.yellow, 2f);

        // Dibujar líneas desde el origen (truePos) a las esquinas
        for (int i = 0; i < 4; i++) {
            Debug.DrawLine(truePos, esquinas[i], Color.yellow, 2f);
        }
    }


}
