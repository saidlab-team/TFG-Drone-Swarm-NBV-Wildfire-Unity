using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DroneCameraManager : MonoBehaviour
{
    [Header("UI Setup")]
    public Transform contenedorCamarasUI; // El panel con el Layout Group
    public GameObject prefabRawImage;     // Tu prefab del RawImage

    [Header("Calidad de las cámaras")]
    public int resolucionX = 256; 
    public int resolucionY = 256;

    // Guardamos referencias para limpiar la memoria si se destruyen los drones
    private List<RenderTexture> texturasActivas = new List<RenderTexture>();

    // Llama a esta función cada vez que instancies un dron auxiliar
    public void AñadirCamaraAlHUD(Camera camaraDronAuxiliar)
    {
        Debug.Log("Añadiendo cámara al HUD: " + camaraDronAuxiliar.name);
        // 1. Crear un RenderTexture en tiempo real
        // Formato 16 es la profundidad de color (suficiente para cámaras pequeñas)
        RenderTexture nuevaTextura = new RenderTexture(resolucionX, resolucionY, 16);
        nuevaTextura.Create();
        texturasActivas.Add(nuevaTextura);

        // 2. Asignar la textura a la cámara del dron para que "grabe" ahí
        camaraDronAuxiliar.targetTexture = nuevaTextura;

        // 3. Instanciar el prefab de UI (RawImage) en el contenedor
        GameObject nuevaUI = Instantiate(prefabRawImage, contenedorCamarasUI);
        
        // 4. Conectar el RawImage a la textura
        RawImage rawImage = nuevaUI.GetComponent<RawImage>();
        rawImage.texture = nuevaTextura;
    }

    // Las RenderTextures se quedan en la memoria VRAM (Tarjeta Gráfica).
    // Si reinicias el nivel o eliminas drones, DEBES destruirlas.
    void OnDestroy()
    {
        foreach (RenderTexture rt in texturasActivas)
        {
            if (rt != null)
            {
                rt.Release(); // Libera la memoria
            }
        }
    }
}