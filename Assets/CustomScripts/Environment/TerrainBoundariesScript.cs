using UnityEngine;

public class TerrainBoundariesScript : MonoBehaviour
{
    // Crea una "caja invisible" que limita el movimiento del dron
    public Transform drone; //Asignar en el editor
    private Vector3 dronePosition;

    // Caracteristicas de la pared
    private float wallHeight = 100f;
    private float wallWidth = 100f;
    //private float wallHeight;
    private float wallDistance = 300f; // Area de 600*600 metrods
    // La coordenada y es la altura

    void Start()
    {
        GameObject.Find("Cesium World Terrain").SetActive(true);
        dronePosition = drone.position;
        CreateWall(new Vector3(dronePosition.x + wallDistance, dronePosition.y, dronePosition.z), "InvisibleWallNorth", new Vector3(0f, 0f, 90f)); // Pared norte
        CreateWall(new Vector3(dronePosition.x - wallDistance, dronePosition.y, dronePosition.z), "InvisibleWallSouth", new Vector3(0f, 0f, 90f)); // Pared sur

        CreateWall(new Vector3(dronePosition.x , dronePosition.y, dronePosition.z + wallDistance), "InvisibleWallEast",new Vector3(90f, 0f, 0f)); // Pared este
        CreateWall(new Vector3(dronePosition.x , dronePosition.y, dronePosition.z - wallDistance), "InvisibleWallWest",new Vector3(90f, 0f, 0f)); // Pared oeste

    }
    void CreateWall(Vector3 pos, string wallName, Vector3 rot)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Plane);
        wall.transform.position = pos;
        wall.name = wallName;
        wall.transform.rotation = Quaternion.Euler(rot); // Rotacion 90 grados para que sea vertical
        wall.transform.localScale = new Vector3(wallWidth, 1, wallHeight); // Escalado de la pared teniendo en cuenta que la original mide 10 * 10
        // Invisible pero con colision
        Destroy(wall.GetComponent<MeshRenderer>());
    }

}
