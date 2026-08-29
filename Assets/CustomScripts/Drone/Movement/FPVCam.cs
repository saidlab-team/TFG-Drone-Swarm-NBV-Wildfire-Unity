using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System;

public class FPVCam : MonoBehaviour
{

    public float sensX;
    public float sensY;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform orientation;

    float xRotation;
    float yRotation;

    public bool MainDrone;

    float mouseX;
    float mouseY;

    public Quaternion targetRotation;

    [Header("Escaneo Automático")]
    public float scanSpeed = 2f;
    public float scanAngle = 30f; // Amplitud del vaivén
    public float tiltWithMovement = 5f; // Inclinación según velocidad
    public float autoTime;

    [Header("Configuración VR")]
    public bool usarVR = false; // Elige desde el inspector si juegas en VR o PC

    public bool modoMonitoreo = false;
    public bool modoFijo = false;

    public Quaternion posToLook;

    public float suavizado = 100f; // Cuanto más bajo, más pesado/suave se siente
    private float xRotationActual;
    private float yRotationActual;


    public EventTickExplorationRate eventTickExplorationRate;
    private int explorationRateCounter = 0;
    private HashSet<string> objetosYaVistos = new HashSet<string>();
    public metricsScript metricsManager;

    public bool ppalComputaMetricas = false;

    void ResetExplorationRateCounter() {
        explorationRateCounter = 0;
    }
    void OnEnable() {
        EventTickExplorationRate.OnTickExplorationRate += saveExplorationRateData;
    }
    void OnDisable() {
        EventTickExplorationRate.OnTickExplorationRate -= saveExplorationRateData;
    }
    private void saveExplorationRateData() {
        if (ppalComputaMetricas) {
            metricsManager.saveMetricRate(explorationRateCounter, "ppalDrone");
        }
        ResetExplorationRateCounter();
    }

    void Start()
    {
        if (!usarVR)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        modoFijo = true;
        metricsManager = FindObjectOfType<metricsScript>();
    }

    public void setLookAtRot(Quaternion rot)
    {
        posToLook = rot;
    }
    public void setModoMonitoreo(bool value)
    {
        modoMonitoreo = value;
        modoFijo = !value;
    }
    public void setModoFijo(bool value)
    {
        modoFijo = value;
        modoMonitoreo = !value;
    }
    [SerializeField] private float tiempoMinimoObservacion = 1.0f; // Tiempo mínimo en segundos para dar por "confirmado" el fuego
    private Dictionary<string, float> tiempoObservacionCeldas = new Dictionary<string, float>();

    public void HandleDetection(Collider other)
    {
        if (!ppalComputaMetricas) return;

        bool isFire = other.CompareTag("Fire");
        bool isBurned = other.CompareTag("Burned");

        if (!isFire && !isBurned) return;

        int indiceX = Mathf.FloorToInt((other.transform.position.x + 300f) / 20f);
        int indiceZ = Mathf.FloorToInt((other.transform.position.z + 300f) / 20f);
        string claveCelda = $"{indiceX}_{indiceZ}";

        // 1. Acumulamos tiempo mientras el collider permanezca dentro
        if (!tiempoObservacionCeldas.ContainsKey(claveCelda))
        {
            tiempoObservacionCeldas[claveCelda] = 0f;
        }

        tiempoObservacionCeldas[claveCelda] += Time.deltaTime;

        // 2. Si no ha alcanzado el tiempo mínimo, no contabilizamos aún
        if (tiempoObservacionCeldas[claveCelda] < tiempoMinimoObservacion)
        {
            return;
        }

        // 3. Procesamos la métrica una vez superado el tiempo
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
    }

    public void HandleDetectionExit(Collider other)
    {
        if (!ppalComputaMetricas) return;

        bool isFire = other.CompareTag("Fire");
        bool isBurned = other.CompareTag("Burned");

        if (!isFire && !isBurned) return;

        int indiceX = Mathf.FloorToInt((other.transform.position.x + 300f) / 20f);
        int indiceZ = Mathf.FloorToInt((other.transform.position.z + 300f) / 20f);
        string claveCelda = $"{indiceX}_{indiceZ}";

        // Reiniciamos el tiempo de observación al salir del collider
        if (tiempoObservacionCeldas.ContainsKey(claveCelda))
        {
            tiempoObservacionCeldas.Remove(claveCelda);
        }
    }



    // rotar en X e Y, Ignorar Z 
    // Update is called once per frame
    void Update()
    {   
        // En VR el timeScale importa, pero Cursor.visible no debería capar el script
        if (Time.timeScale == 0f || (!usarVR && Cursor.visible))
        {
            return; 
        }

        if (MainDrone)
        {
            if (usarVR)
            {
                // MÁGIA VR: Si usamos VR, el XR Origin rota la cámara de forma nativa. 
                // No escribimos nada en transform.rotation para no romper el tracking físico.
                return;
            }

            // --- MODO PC TRADICIONAL (Solo ejecuta si usarVR == false) ---
            float mouseX = Mouse.current.delta.x.ReadValue() * sensX * Time.deltaTime;
            float mouseY = Mouse.current.delta.y.ReadValue() * sensY * Time.deltaTime;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            xRotationActual = Mathf.Lerp(xRotationActual, xRotation, Time.deltaTime * suavizado);
            yRotationActual = Mathf.Lerp(yRotationActual, yRotation, Time.deltaTime * suavizado);

            transform.rotation = Quaternion.Euler(xRotationActual, yRotationActual, 0);
        }
        else
        {
            // --- MODO DRONES AUTOMÁTICOS (Acompañantes) ---
            // Queda exactamente igual, ya que ellos no usan cascos de VR individuales
            if (modoMonitoreo)
            {
                Quaternion targetRotation = Quaternion.LookRotation(posToLook * Vector3.forward * -1f);
                float degreesPerSecond = 20f; 
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, degreesPerSecond * Time.deltaTime);
            }
            else if (modoFijo)
            {
                xRotation = Mathf.Lerp(xRotation, 45f, Time.deltaTime);
                transform.rotation = orientation.rotation * Quaternion.Euler(xRotation, 0, 0);
            }
        }

    }
}
