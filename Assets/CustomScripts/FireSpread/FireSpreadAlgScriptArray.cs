using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;


public class FireSpreadAlgScriptArr : MonoBehaviour
{
    public class NodoCelda
    {
        private int indiceX; // %10
        private int indiceZ; // %10
        private int estado; // 0- normal, 1- Precalentado, 2- Combustion, 3- Consumido // REVISAR SI ES NECESARIO
        private int probabilidadEncender;
        private int vecesVisto;

        public NodoCelda(int indiceX, int indiceZ, int estado)
        {
            this.indiceX = indiceX;
            this.indiceZ = indiceZ;
            this.estado = estado;
            this.probabilidadEncender = 30; // revisar
            this.vecesVisto = 0;
        }
        
        public int getEstado()
        {
            return estado;
        }
        public void setEstado(int estado)
        {
            this.estado = estado;
        }
        public int getIndiceX()
        {
            return indiceX;
        }
        public int getIndiceZ()
        {
            return indiceZ;
        }
        public int getProbabilidadEncender()
        {
            return probabilidadEncender;
        }
        public void setProbabilidadEncender(int probabilidadEncender)
        {
            this.probabilidadEncender = probabilidadEncender;
        }
        public int getVecesVisto()
        {
            return vecesVisto;
        }
        public void setVecesVisto(int vecesVisto)
        {
            this.vecesVisto = vecesVisto;
        }
        
        
    }
    
    private Vector2 spawnArea = new Vector2(300f, 300f); // Area disponible (600 * 600)
    

    int tamanoTerreno = 600; // El area es de 600 * 600
    int tamanoCelda = 20; // cada celda es de 10 * 10
    public int tamanoGrid ; // = 60 posiciones
    int numeroFocos = 90; // = 20?

    LinkedList<NodoCelda> celdasAProcesar;
    LinkedList<NodoCelda> celdasProximoPaso;
    Dictionary<string, NodoCelda> celdasEnRevison; // 
    private Dictionary<string, List<GameObject>> treesPlaced;
    private Dictionary<string, GameObject> preheatPlaced;

    [HideInInspector]
    public bool enableUpdate = false;
    bool setupCall = false;
    public Slider progressbar;

    public GameObject fuego;
    public Transform parentObject;
    public Transform uu;
    // Debe esperar a que terminen de ponerse los arboles.
    // Una vez terminen, elegira un punto al azar y empezará el fuego
    // Creara una corrutina para empezar el algoritmo de propagacion. target: O(n2)

    public Transform VegetationPlacement;

    public GameObject BurnedProjector;
    public Transform burnedParent;
    public Transform preheatParent;
    public GameObject preheatPrefab;

    public NodoCelda[,] gridCompleta;
    public metricsScript metricsScript;
    
    public LayerMask terrenoLayer;

    
    void Update()
    {
        // Espera a que termine la progressbar (ver value de slider)
        if (progressbar.value >= 1)
        {
            
            enableUpdate = true;
        }
        if (!enableUpdate) return;

        if (!setupCall)
        {
            SetupAlgo();
            metricsScript.inicioIncendioActivado();
        }
        
    }

    void SetupAlgo()
    {
        if (!setupCall)
        {
            setupCall = true;
        }

        
        // Setup de la grid
        tamanoGrid = tamanoTerreno / tamanoCelda;
        //terrenoGrid = new NodoCelda[tamanoGrid, tamanoGrid]; // Grid 2D de 60 * 60 donde cada celda equivale a un area de 10 * 10 
        celdasAProcesar = new LinkedList<NodoCelda>();

        gridCompleta = new NodoCelda[tamanoGrid,tamanoGrid]; // celdas son NULL

        //InitGrid();
        celdasEnRevison = new Dictionary<string, NodoCelda>();

        // Elige un punto cualquiera del mapa
        Vector3 position = new Vector3(
            UnityEngine.Random.Range(-spawnArea.x, spawnArea.x),
            1000, // Alatura prdeterminada. hacer raycast al suelo
            UnityEngine.Random.Range(-spawnArea.y, spawnArea.y)
        );  
        // Transforma el punto en indices (x,z) para obtener una celda aleatoria
        int celdaIndX = (int) (position.x + 300) / tamanoCelda; // + 300 para evitar posiciones negativas
        int celdaIndZ = (int) (position.z + 300) / tamanoCelda; // para tener rangos de (0 , 600) en vez de (-300, 300)

        NodoCelda newNode = new NodoCelda(celdaIndX,celdaIndZ,1);
        newNode.setProbabilidadEncender(100);
        
        
        celdasAProcesar.AddLast(newNode);
        treesPlaced = VegetationPlacement.GetComponent<VegetationPlacement>().treesPlaced;
        preheatPlaced = new Dictionary<string, GameObject>();
        

        StartCoroutine(HandleFire());
    }
    
    IEnumerator HandleFire()
    {
        //  Normal --> Precalentado
        //      Cuando tiene una celda en combustion cerca
        //  Precalentado --> Combustion 
        //      Prob fija (40%) + 5% (por cada celda contigua (max 8) en combustion) + 10% (Temperatura ambiente) + 10% (Diferencia de altura)
        //  Combustion --> Consumido
        //      Despues de x tiempo
        yield return new WaitForSeconds(3); // Espera 3 segundos antes de empezar el fuego

        while (celdasAProcesar.Count > 0)
        {
            celdasProximoPaso = new LinkedList<NodoCelda>();
            
            foreach (NodoCelda node in celdasAProcesar)
            {
                ProcessCell(node);

            }
            celdasAProcesar.Clear();
            celdasAProcesar = celdasProximoPaso;
            // celdasEnRevison.Clear();
            
            //Debug.Log($"---------------");
            yield return new WaitForSeconds(5);
        }

        //Debug.Log("Ending");
    }
    void ProcessCell(NodoCelda node)
    {
        string newKey = $"{node.getIndiceX()},{node.getIndiceZ()}";

        

        if (node.getEstado() == 1)
        {
            // 1- Precalentado
            // probabilidad de encenderse, si no se enciende, guardarlo otra vez para la siguiente fase con una probailidad mayor
            // Debug.Log("Preheat");
            float rand = UnityEngine.Random.Range(0, 100);
            if (rand < node.getProbabilidadEncender())
            {
                // se enciende
                if (parentObject.childCount <= numeroFocos)
                {
                    SetFire(node.getIndiceX(), node.getIndiceZ());
                }
                else
                {
                    celdasProximoPaso.AddLast(node);
                }
                
            }
            else
            {
                // No se enciende
                node.setProbabilidadEncender(node.getProbabilidadEncender() + 5);
                celdasProximoPaso.AddLast(node);
            }

        }else if (node.getEstado() == 2)
        {
            // 2- Combustion
            // co rutina para apagar el fuego luego de x tiempo -> cambiar textura de suelo y arboles
            // poner las celdas adyacentes en estado de precalentado
            //Debug.Log("On fire");
            SetNeighborsPreHeat(node.getIndiceX() - 1, node.getIndiceZ() - 1);
            SetNeighborsPreHeat(node.getIndiceX(), node.getIndiceZ() - 1);
            SetNeighborsPreHeat(node.getIndiceX() + 1, node.getIndiceZ() - 1);
            
            SetNeighborsPreHeat(node.getIndiceX() - 1, node.getIndiceZ());
            SetNeighborsPreHeat(node.getIndiceX() + 1, node.getIndiceZ());

            SetNeighborsPreHeat(node.getIndiceX() - 1, node.getIndiceZ() + 1);
            SetNeighborsPreHeat(node.getIndiceX(), node.getIndiceZ() + 1);
            SetNeighborsPreHeat(node.getIndiceX() + 1, node.getIndiceZ() + 1);
        }
    }
    bool first = true;
    void SetFire(int x, int z) // z con raycast
    {
        string newKey = $"{x},{z}";
        if (!first)
        {
            if (preheatPlaced.ContainsKey(newKey))
            {
                GameObject preheatproj = preheatPlaced[$"{x},{z}"];
                if (preheatproj != null)
                {
                    GameObject.Destroy(preheatproj);
                }
                preheatPlaced.Remove($"{x},{z}");
            }
        }else
        {
            first = false;
        }
        

        // pone una celda en combustion
        NodoCelda newNode = new NodoCelda(x,z,2);
        celdasProximoPaso.AddLast(newNode);
        
        // efectos visuales
        Vector3 position = new Vector3(
                (x*20)-300 + 10,
                100,
                (z*20)-300 + 10 
            );


        Vector3 origin = position + Vector3.up; // Origen del rayo 
        Vector3 direction = Vector3.down;
        float maxDistance = 200f; // REVISAR

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, terrenoLayer))
        {
            position = hit.point; 
        }

        
        if (celdasEnRevison.ContainsKey(newKey)){
            return;
        }else{
            celdasEnRevison.Add(newKey, newNode);    
        }

        Quaternion randomizeRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
        GameObject instance = Instantiate(fuego, position, randomizeRotation, parentObject);
        instance.tag = "Fire";
        //Debug.Log($"objeto instanciado en {position}");
        //Color color= new Color(0, 0, 0, 1);
        //uu.GetComponent<yy7>().PaintCell(newNode.getIndiceX(),newNode.getIndiceZ(), color);
        //Debug.Log($"FUEGO EN indice :{x}, {z} // posicion {position.x}, {position.z}");


        gridCompleta [x,z] = newNode;
        // Calcular pesos
        metricsScript.addCeldaQuemada();
        metricsScript.addFuego(x, z);
        StartCoroutine(burnTrees(newKey));

    }
    IEnumerator burnTrees(string key)
    {
        yield return new WaitForSeconds(18);

        if (!treesPlaced.ContainsKey(key))
        {
            yield break;
        }
        else
        {
            float highestree = 0;

            string[] parts = key.Split(',');
            int x = int.Parse(parts[0]);
            int z = int.Parse(parts[1]);

            foreach (var tree in treesPlaced[key])
            {
                if (tree != null)
                {
                    if (Mathf.Abs(tree.transform.position.y) > Mathf.Abs(highestree))
                    {
                        highestree = tree.transform.position.y;
                    }
                    GameObject.Destroy(tree);
                }
                    
            }
            treesPlaced[key].Clear();
            
            Vector3 position = new Vector3(
                (x*20)-300 + 10,
                highestree + 2,
                (z*20)-300 + 10
            );
            GameObject proj = Instantiate(BurnedProjector, position, Quaternion.identity, burnedParent);
            proj.tag = "Burned";
            gridCompleta [x,z].setEstado(3); 

        }

        
    }


    void SetNeighborsPreHeat(int nx, int nz)
    {

        // pone una celda en estado de preheat
        if (nx < 0 || nx >= tamanoGrid || nz < 0 || nz >= tamanoGrid)
            return;

        
        if (gridCompleta[nx, nz] != null && gridCompleta[nx, nz].getEstado() > 0)
        {
            return; 
        }

        if (!treesPlaced.ContainsKey($"{nx},{nz}"))
        {
            return;
        }
        float highestree = 0;

        foreach (var tree in treesPlaced[$"{nx},{nz}"])
        {
            if (tree != null)
            {
                if (Mathf.Abs(tree.transform.position.y) > Mathf.Abs(highestree))
                {
                    highestree = tree.transform.position.y;
                }
            } 
        }

        
        

        NodoCelda neighNode = new NodoCelda(nx,nz,1);
        celdasProximoPaso.AddLast(neighNode);
        gridCompleta [nx,nz] = neighNode;
        Vector3 position = new Vector3(
                        (nx*20)-300 + 10,
                        highestree + 2,
                        (nz*20)-300 + 10
                    );

        
        if (!preheatPlaced.ContainsKey($"{nx},{nz}"))
        {
            GameObject proj = Instantiate(preheatPrefab, position, Quaternion.identity, preheatParent);
            preheatPlaced.Add($"{nx},{nz}", proj);
        }
        
    }
    


}
