using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

// Tasa de Exploracion: superficie quemada/fuego (investigada) por unidad de tiempo. --> La cantiad de fueegos nuevos (o superficie) que se van descubriendo por unidad de tiempo
//                      Rapidez con la que el sistema descubre nuevas áreas del incendio

 // % de cobertura: Area del incendio / Area total del mapa - podemos hacer algo como una matriz para ver cuantas veces pasa el dron por cada celda
 //                 Proporción del área total (del frente o del incendio) que ha sido monitoreada por los drones                    

 // Ganancia de informacion: En teoria esto es la puntuacion del NBV
 //                            Cantidad de fuegos nuevos (no mapeados) que se monitorizan a cada paso del algoritmo.

 // Tiempo completitud: Tiempo que tarda en completar la tarea si hay n celdas, tiempo que tarda en monitorear fuego en n/2 o n/4
 //                     Tiempo que tarda el algoritmo en cubrir un porcentaje determinado del incendio con un número variable de drones 




public class metricsScript : MonoBehaviour
{
    struct metricExploracion {
        public int rate;
        public string droneID;
    }

    public float periodoExploracion = 20.0f; // 20 segundos entre cada evento de exploración
    private float cronometro = 0f; 
    List<metricExploracion> metricasExploracion = new List<metricExploracion>(); 
    public EventTickExplorationRate eventTickExplorationRate;

    public FireSpreadAlgScriptArr fireSpreadAlgScriptArr;
    
    public int [,] gridVecesVisto = new int[30,30]; // cobertura
    public bool [,] gridFuegos = new bool[30,30]; // fuegos


    
    
    public List<int> gananciaInformacion = new List<int>();
    private HashSet<string> celdasUnicasMonitoreadasGlobal = new HashSet<string>();
    private HashSet<string> celdasUnicasMonitoreadasFuegoGlobal = new HashSet<string>();


    int celdasQuemadas = 0;
    int celdasTotales = 600;
    bool inicioIncendio = false;
    private float tiempoTranscurrido = 0f;
    int celdasQuemadasMonitoreadas = 0;



    private bool exportado50Quemadas = false;
    private bool exportado50Monitoreadas = false;

    public float tiempoLimiteSimulacion = 180f; // Límite en segundos (ej. 180s = 3 minutos)
    private bool metricasExportadas = false;


    string carpetaDestino;
    string timestamp;
    string ruta;
    public string idExperimento = "EXP_001";
    public string modo = "manual"; // "manual" o "automatico"
    public int numeroDrones = 2; // 2 4 6
    public String propagacion = "rapida"; // "rapida" o "lenta"


    public void addFuego(int x, int z) {
        gridFuegos[x, z] = true;
    }

    public void addVecesVisto(int x, int z) {
        gridVecesVisto[x, z] += 1;
    }

    public void addCeldaMonitoreada(int x, int z) {
        celdasQuemadasMonitoreadas++;
        Debug.Log($"[Métricas] Celda monitoreada: ({x}, {z}). Total de celdas quemadas monitoreadas: {celdasQuemadasMonitoreadas}");
    }

    public void inicioIncendioActivado() {
        inicioIncendio = true;
    }

    public void addCeldaQuemada() {
        celdasQuemadas++;
    }


    public void addGananciaInformacion(int ganancia) {
        gananciaInformacion.Add(ganancia);
    }
    void Start() {
        carpetaDestino = Application.dataPath + "/Metricas_Exportadas/" + idExperimento;
        timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        ruta = $"{carpetaDestino}/{timestamp}_metricas_{idExperimento}";
    }

    public bool addCeldaMonitoreadaGlobal(int x, int z) {
        return celdasUnicasMonitoreadasGlobal.Add($"{x},{z}");
    }

    public bool addCeldaFuegoGlobal(int x, int z) {
        return celdasUnicasMonitoreadasFuegoGlobal.Add($"{x},{z}");
    }

    void Update() {
        if (inicioIncendio && !metricasExportadas) {
            tiempoTranscurrido += Time.deltaTime;
        }
        ExplorationRate();
        

        if (tiempoTranscurrido >= tiempoLimiteSimulacion && !metricasExportadas) {
        metricasExportadas = true;
        
        if (!Directory.Exists(carpetaDestino)) Directory.CreateDirectory(carpetaDestino);
        
        ruta = $"{carpetaDestino}/{timestamp}_metricas_{idExperimento}";

        ExportarTasaExploracion(ruta);
        ExportarMatrizCobertura(ruta);
        ExportarGananciaInformacion(ruta);
        ExportarTiempoCompletitud(ruta);
        
        Debug.Log($"[Métricas] Exportación FINALIZADA correctamente al alcanzar el tiempo límite ({tiempoLimiteSimulacion}s). Ruta: {ruta}");
        
    }

    }   

    private void CoverageRate() {

        Debug.Log($"");
    }
    private void ExplorationRate() {
        cronometro += Time.deltaTime;
        if (cronometro >= periodoExploracion)
        {
            cronometro -= periodoExploracion;
            InvocacionDeMetricas();
        }
    }
    private void InvocacionDeMetricas()
    {
        if (eventTickExplorationRate != null)
        {
            EventTickExplorationRate.tickExplorationRate();
            
            Debug.Log($"[Metricas] Evento ejecutado automáticamente cada {periodoExploracion}s");
            Debug.Log($"[Metricas] Total de métricas de exploración registradas: {metricasExploracion.Count}");
            Debug.Log($"[Metricas] Detalles de las métricas de exploración:");
            foreach (var metric in metricasExploracion)
            {
                Debug.Log($"[Metricas] Drone ID: {metric.droneID}, Tasa de Exploración: {metric.rate}");
            }
        }
    }



    public void saveMetricRate(int rate, String droneID) {
        metricasExploracion.Add(new metricExploracion { rate = rate, droneID = droneID });
    }
    

    // --- LÓGICA DE EXPORTACIÓN MODULAR ---

    // Métrica 1: Tasa de Exploración de los Drones
    private void ExportarTasaExploracion(string rutaBase)
    {
        string rutaFinal = rutaBase + "_TasaExploracion.csv";
        using (StreamWriter writer = new StreamWriter(rutaFinal))
        {
            writer.WriteLine("DroneID,Tasa");
            foreach (var metrica in metricasExploracion)
            {
                writer.WriteLine($"{metrica.droneID},{metrica.rate}");
            }
        }
    }

    // Métrica 3: Ganancia de Información (Puntuación del algoritmo)
    private void ExportarGananciaInformacion(string rutaBase)
    {
        string rutaFinal = rutaBase + "_GananciaInformacion.csv";
        using (StreamWriter writer = new StreamWriter(rutaFinal))
        {
            writer.WriteLine("PasoAlgoritmo,Ganancia");
            for (int i = 0; i < gananciaInformacion.Count; i++)
            {
                writer.WriteLine($"{i},{gananciaInformacion[i]}");
            }
        }
    }

    // Métrica 2: Porcentaje de Cobertura (Matriz espacial)
    private void ExportarMatrizCobertura(string rutaBase)
    {
        string rutaFinal = rutaBase + "_MatrizCobertura.csv";
        using (StreamWriter writer = new StreamWriter(rutaFinal))
        {
            for (int x = 0; x < 30; x++)
            {
                string fila = "";
                for (int z = 0; z < 30; z++)
                {
                    fila += gridVecesVisto[x, z].ToString();
                    if (z < 29) fila += ","; // Agrega la coma solo si no es la última columna
                }
                writer.WriteLine(fila);
            }
        }
    }

    // Métrica 4: Tiempos de Completitud
    private void ExportarTiempoCompletitud(string rutaBase)
    {
        string rutaFinal = rutaBase + "_TiempoCompletitud.csv";
        using (StreamWriter writer = new StreamWriter(rutaFinal))
        {
            writer.WriteLine("ExpID;Modo;NumDrones;Propagacion;TiempoTranscurrido;CeldasQuemadas;CeldasMonitoreadas");
            writer.WriteLine($"{idExperimento};{modo};{numeroDrones};{propagacion};{tiempoTranscurrido};{celdasQuemadas};{celdasQuemadasMonitoreadas}");
        }

    }

    

}  
