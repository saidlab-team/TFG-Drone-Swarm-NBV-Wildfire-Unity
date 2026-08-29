using UnityEngine;
using System.Collections.Generic;

public class NPCSettings : MonoBehaviour
{
    private int numberOfAreas;
    private int numberOfDrones;
    private int numberOfDronesPerArea;
    private int numberOfCameraHolders; 
    

    public GameObject NPCprefab;
    public Transform dronesParent;

    public FireManager fireManagerReference;
    public MapDroneManager mapDroneManagerReference;
    public DroneCameraManager droneCameraManager; 
    public DroneOrientationManager droneOrientationManager;

    public void SetupCreatNPCs()
    {
        List<Vector2> posiciones2D = mapDroneManagerReference.ObtenerPosicionesPara3D();
        numberOfDrones = posiciones2D.Count;
        CreateNPCs(posiciones2D);

    }

    public void CreateNPCs(List<Vector2> posiciones2D)
    {   
        numberOfCameraHolders = numberOfDrones; // Asumimos que cada dron tiene un camera holder
        int i = 0;
        foreach (Vector2 pos in posiciones2D)
        {
            GameObject drone = Instantiate(NPCprefab, Vector3.zero, Quaternion.identity,dronesParent);
            drone.name = $"Drone_{i + 1}";
            drone.GetComponentInChildren<NPCWanderer>().fireManager = fireManagerReference;
            drone.transform.GetChild(1).GetComponent<Area>().transform.position = new Vector3(i * 10, 0, 0); 


            float centroX = pos.x; 
            float centroZ = pos.y;

            Vector3 posicionArea = new Vector3(centroX, 100, centroZ); // la y?

            Vector3 origin = posicionArea + Vector3.up; // Origen del rayo
            Vector3 direction = Vector3.down;
            if (Physics.Raycast(origin, direction, out RaycastHit hit, 200f))
            {
                drone.transform.GetChild(1).GetComponent<Area>().transform.position = hit.point;
                drone.transform.GetChild(1).GetComponent<Area>().radius = mapDroneManagerReference.sizeArea;
            }
            i++;
            droneCameraManager.AñadirCamaraAlHUD(drone.GetComponentInChildren<Camera>()); 
            droneOrientationManager.AñadirorientacionAlHUD(drone.transform.GetChild(2)); 
        } 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
