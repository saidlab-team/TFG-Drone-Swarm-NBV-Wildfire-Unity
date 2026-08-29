using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using CesiumForUnity;
using Unity.Mathematics;

// TODO dejarlos colocados en la demos de la defensa???
public class VegetationPlacement : MonoBehaviour
{

    // Crea un arbol en una localizacion aleatoria dentro de un area
    // modificarlo para que creee muchos 
    //public float numberOfTrees;
    //public List<GameObject> treeTypes;
    public List<VegetationType> vegetationTypes;

    private Vector2 spawnArea = new Vector2(300f, 300f); 
    //public Transform parentObject; // Objeto padre donde estaran los arboles

    // diccionario celda (x,z), lista de gameobjects??
    public Dictionary<string, List<GameObject>> treesPlaced;

    [System.Serializable]
    public class VegetationType
    {
        public string name;
        public List<GameObject> treeTypes;
        public int count;
        public Transform parentObject;
        [HideInInspector] public bool colocados_todos = false;
        [HideInInspector] public int colocados = 0;
    }


    void Start()
    {
        treesPlaced = new Dictionary<string, List<GameObject>>();
        foreach (var type in vegetationTypes)
        {
            StartCoroutine(createTreesOnArea(type, type.count));
            //StartCoroutine(createTreesOnArea(type, type.count / 2));
        }
    }



// +300 / 10

    int batchSize = 10;
    IEnumerator  createTreesOnArea(VegetationType typ, int count)
    {
        Physics.autoSimulation = false;
        for (int i = 0; i < count; i++)
        {
            // Elige un arbol aleatorio de entre las posibilidades
            GameObject randomTree = typ.treeTypes[UnityEngine.Random.Range(0, typ.treeTypes.Count)];

            Vector3 position = new Vector3(
                UnityEngine.Random.Range(-spawnArea.x, spawnArea.x),
                100,
                UnityEngine.Random.Range(-spawnArea.y, spawnArea.y)
            );
            
            Quaternion randomizeRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            GameObject instance = Instantiate(randomTree, position, randomizeRotation, typ.parentObject);
            instance.isStatic = true;
            //instance.SetActive(false);

            instance.name = $"{typ.name}_{i:D4}";
            typ.colocados = typ.colocados + 1;


            int indiceX = (int) (position.x + 300) / 20;
            int indiceZ = (int) (position.z + 300) / 20; 
            string key = $"{indiceX},{indiceZ}";
            if (!treesPlaced.ContainsKey(key))
            {
                treesPlaced[key] = new List<GameObject>();
            }
            treesPlaced[key].Add(instance);


            if (i % batchSize == 0)
            {
                
                Debug.Log($"Colocados en y=100 {i}/{count} arboles...");
                yield return null;
            }

        } 
        Physics.autoSimulation = true;
        typ.colocados_todos = true;
    }

}



