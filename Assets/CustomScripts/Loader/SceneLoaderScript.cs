using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CesiumForUnity;
using System.Collections;



public class SceneLoader : MonoBehaviour
{
    // Componentes de la UI y el spawner
    public Slider progressBar;
    public Image backgroundImage;
    public Transform vegetationSpawner;


    private bool loadCompete = false;
    
    // a
    private Transform bushChild;
    private Transform treeChild;
    private Cesium3DTileset tileset;
    private VegetationPlacement vegetationPlacement;


    private float progress = 0f;
    
    private float adjustedCount = 0f;
    private float previousProgress = 0f;


    private float countBush;
    private float countTree;

    // Image fadeout
    private bool fadingOut = false;
    public float fadeSpeed = 1f;


    
    void OnEnable()
    {
        tileset = FindObjectOfType<Cesium3DTileset>();   
        treeChild = vegetationSpawner.Find("TreeHeight");
        bushChild = vegetationSpawner.Find("BushHeight");
        vegetationPlacement = vegetationSpawner.GetComponent<VegetationPlacement>();
        progressBar.value = progress; // 100% es 1f (si esta en enteros, dividir entre 100)
        countBush = (float) bushChild.GetComponent<AdjustVegetationHeight>().count; // 2000
        countTree = (float) treeChild.GetComponent<AdjustVegetationHeight>().count; // 7000
    }
    void Start()
    {
        // Enable del terreno
        tileset.gameObject.SetActive(true);
        // Disable lo demas
        vegetationSpawner.gameObject.SetActive(false);
        treeChild.gameObject.SetActive(false);
        bushChild.gameObject.SetActive(false);
        
    }

    void Update()
    {
        if (!loadCompete)
        { 
            backgroundImage.gameObject.SetActive(true);
            //loadTrees();
            if (!loadTerrain()) return; 
            vegetationSpawner.gameObject.SetActive(true);
            if (!loadPlacedTrees()) return;
            treeChild.gameObject.SetActive(true);
            bushChild.gameObject.SetActive(true);
            if (!loadAdjustedTrees()) return;
        }

        if (loadCompete && !fadingOut)
        {
            // desactivar los scripts??
            fadingOut = true;
            
        }
    }
    void LateUpdate()
    {
        if (loadCompete) return;
        
        progress = (mapProgress * (1f / 3f) +
            placementProgress * (1f / 3f) +
            adjustedProgress * (1f / 3f)) / 100;

        if (progress > previousProgress)
        {
            previousProgress = progress;
            progressBar.value = progress;
        }
        
        if (progress == 1f)
        {
            loadCompete = true;
            progressBar.gameObject.SetActive(false);
        }
    }



    float mapProgress= 0f;
    bool loadTerrain()
    {
        // 1/3
        mapProgress =  (float) tileset.ComputeLoadProgress();
        if (mapProgress == 100f) return true;
        return false;
    }

    float placementProgress= 0f;
    bool loadPlacedTrees()
    {
         // 1/3
        float placedCount = 0f;
        foreach (var type in vegetationPlacement.vegetationTypes) // Total 2000 + 7000
        {
            placedCount = placedCount + (float)type.colocados;
        }
        //Debug.Log($"Placed count {placedCount}");
        placementProgress = placedCount  / (countBush + countTree) * 100f;
        if (placementProgress >= 100f)
        {
            placementProgress = 100f;
            return true;
        }
        return false;
    }
    float adjustedProgress = 0f;
    float bushChildProgress;
    float treeChildProgress;

    bool loadAdjustedTrees()
    {
        bushChildProgress = 0f;
        if (bushChild != null)
        {
            bushChildProgress = (float)bushChild.GetComponent<AdjustVegetationHeight>().processed;
        }
        treeChildProgress = 0f;
        if (treeChild != null)
        {
            treeChildProgress = (float)treeChild.GetComponent<AdjustVegetationHeight>().processed;
        }
        adjustedProgress = (bushChildProgress + treeChildProgress) / (countBush + countTree) * 100f;


        if (adjustedProgress >= 100f)
        {
            adjustedProgress = 100f;
            return true;
        }
        return false;
    }
    
    /*
    void loadTrees()
    {
        // 1/3
        float mapProgress =  (float) tileset.ComputeLoadProgress();

        // 1/3
        float placedCount = 0f;
        foreach (var type in vegetationPlacement.vegetationTypes) // Total 2000 + 7000
        {
            placedCount = placedCount + (float)type.colocados;
        }
        //Debug.Log($"Placed count {placedCount}");
        float placementProgress = placedCount  / (countBush + countTree) * 100f;
        if (placementProgress > 100f)
        {
            placementProgress = 100f;
        }

        // 1/3
        float bushChildProgress = 0f;
        if (bushChild != null)
        {
            bushChildProgress = (float)bushChild.GetComponent<AdjustVegetationHeight>().processed;
        }
        float treeChildProgress = 0f;
        if (treeChild != null)
        {
            treeChildProgress = (float)treeChild.GetComponent<AdjustVegetationHeight>().processed;
        }
        float adjustedProgress = (bushChildProgress + treeChildProgress) / (countBush + countTree) * 100f;


        if (adjustedProgress > 100f)
        {
            adjustedProgress = 100f;
        }

        //Debug.Log($"Map prog {mapProgress}");
        //Debug.Log($"Placement prog {placementProgress}");
        //Debug.Log($"adjust prog {adjustedProgress}");
        

        progress = (mapProgress * (1f / 3f) +
            placementProgress * (1f / 3f) +
            adjustedProgress * (1f / 3f)) / 100;

        //Debug.Log($"progress {progress}");

        if (progress > previousProgress)
        {
            previousProgress = progress;
            progressBar.value = progress;
        }
        if (progress == 1f)
        {
            loadCompete = true;
            progressBar.gameObject.SetActive(false);
        }
        
    }*/
    
}
