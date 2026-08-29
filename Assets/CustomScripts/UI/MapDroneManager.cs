using UnityEngine;
using System.Collections.Generic;

public class MapDroneManager : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject prefabDronUI; // Arrastra aquí el Prefab del dron UI
    public Transform panelMapa;     // El panel de 600x600

    // Lista para tener control de los drones creados
    private List<GameObject> dronesEnMapa = new List<GameObject>();
    public float sizeArea;
    

    // Llama a esta función cuando el usuario cambie el slider de cantidad
    // o pulse un botón de "Generar Drones"

    public void GenerateNewDrones(int cantidad, float radioAccion)
    {
        // 1. Limpiamos el mapa por si ya había drones de antes
        foreach (GameObject dron in dronesEnMapa)
        {
            Destroy(dron);
        }
        dronesEnMapa.Clear();

        // 2. Creamos los nuevos drones
        for (int i = 0; i < cantidad; i++)
        {
            // Instanciamos como hijo del panel del mapa
            GameObject nuevoDron = Instantiate(prefabDronUI, panelMapa);
            
            // Los colocamos de forma un poco aleatoria en el centro para que no aparezcan 100% solapados
            RectTransform rect = nuevoDron.GetComponent<RectTransform>();
            float offsetX = Random.Range(-50f, 50f);
            float offsetY = Random.Range(-50f, 50f);
            rect.anchoredPosition = new Vector2(offsetX, offsetY);

            // 3. Ajustamos el tamaño del área (Escala 1:1 -> diámetro = radio * 2)
            Transform circuloArea = nuevoDron.transform.Find("Dron2D"); // Pon el nombre exacto de tu hijo
            if (circuloArea != null)
            {
                float diametro = radioAccion * 2f;
                circuloArea.GetComponent<RectTransform>().sizeDelta = new Vector2(diametro, diametro);
            }
            sizeArea = radioAccion;
            dronesEnMapa.Add(nuevoDron);
        }
    }
    public void GenerateExistingDrones(List<Vector2> posiciones, float radioAccion)
    {
        // 1. Limpiamos el mapa por si ya había drones de antes
        foreach (GameObject dron in dronesEnMapa)
        {
            Destroy(dron);
        }
        dronesEnMapa.Clear();

        // 2. Creamos los nuevos drones en las posiciones especificadas
        for (int i = 0; i < posiciones.Count; i++)
        {
            // Instanciamos como hijo del panel del mapa
            GameObject nuevoDron = Instantiate(prefabDronUI, panelMapa);

            // Colocamos el dron en la posición especificada
            RectTransform rect = nuevoDron.GetComponent<RectTransform>();
            rect.anchoredPosition = posiciones[i];

            // 3. Ajustamos el tamaño del área (Escala 1:1 -> diámetro = radio * 2)
            Transform circuloArea = nuevoDron.transform.Find("Dron2D"); // Pon el nombre exacto de tu hijo
            if (circuloArea != null)
            {
                float diametro = radioAccion * 2f;
                circuloArea.GetComponent<RectTransform>().sizeDelta = new Vector2(diametro, diametro);
            }
            sizeArea = radioAccion;
            dronesEnMapa.Add(nuevoDron);
            
        }
    }

    public void GenerarDrones(int cantidad, float radioAccion)
    {
        if (cantidad == dronesEnMapa.Count)
        {
            GenerateExistingDrones(ObtenerPosicionesPara3D(), radioAccion);
        }
        else
        {
            GenerateNewDrones(cantidad, radioAccion);
        }
    }
    public void LimpiarMapa()
    {
        foreach (GameObject dron in dronesEnMapa)
        {
            Destroy(dron);
        }
        dronesEnMapa.Clear();
    }
    

    // Llama a esta función cuando le des al botón "Empezar Simulación"
    // para obtener las posiciones y pasarlas a tu mundo 3D
    public List<Vector2> ObtenerPosicionesPara3D()
    {
        List<Vector2> posiciones = new List<Vector2>();
        foreach (GameObject dron in dronesEnMapa)
        {
            // Extraemos la posición local dentro del panel
            posiciones.Add(dron.GetComponent<RectTransform>().anchoredPosition);
        }
        return posiciones;
    }
}