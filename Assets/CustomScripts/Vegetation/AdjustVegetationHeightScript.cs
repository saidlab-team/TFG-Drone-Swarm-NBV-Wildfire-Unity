using UnityEngine;
using CesiumForUnity;
using Unity.Mathematics;
using System.Collections;

public class AdjustVegetationHeight : MonoBehaviour 
{
    public Transform forest;     // GameObject padre que contiene todos los árboles
    private CesiumGeoreference georeference;
    private bool allAdjusted = false;
    public int count;
    [HideInInspector] public int processed = 0;
    
    public float rotationSpeed = 45f;
    public float fullRotation = 360f;
    Cesium3DTileset tileset;
    bool enableUpdate = false;
    bool tilesLoaded=false;
    public LayerMask terrenoLayer;
    

    public void Start()
    {
        tileset = FindObjectOfType<Cesium3DTileset>();
        GameObject.Find("Cesium World Terrain").SetActive(true);
    }

    public void Update()
    {
        if (!tilesLoaded)
        {
            //Debug.Log($" tileset progress{tileset.ComputeLoadProgress()}");

            if (tileset.ComputeLoadProgress() == 100f)
            {
                //Actualiza progressbar
                enableUpdate = true;
                tilesLoaded = true;
            }
        }
        

        if (!enableUpdate) return;

        if (processed != count)
        {
            adjustHeight();
        }
    }
    
    public void adjustHeight()
    {
        

        foreach (Transform tree in forest)
        {
            if (tree.position.y == 100)
            {   
                
                double3 pos = new double3(tree.position.x, tree.position.y, tree.position.z);
                Vector3 origin = tree.position + Vector3.up; // Origen del rayo 
                Vector3 direction = Vector3.down;
                float maxDistance = 200f; // REVISAR

                if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, terrenoLayer))
                {
                    tree.position = hit.point;
                    processed++;
                }

                //Debug.Log($"Arboles con altura ajustada: {processed}");
            }


        }
    }
    

}
