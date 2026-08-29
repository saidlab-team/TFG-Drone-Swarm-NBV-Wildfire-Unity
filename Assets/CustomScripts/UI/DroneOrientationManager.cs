using UnityEngine;

public class DroneOrientationManager : MonoBehaviour
{
    public Transform contenedorOrrientUI; // El panel con el Layout Group
    public GameObject prefabDroneOrientation;     
    public Camera dron3D;

    public void AñadirorientacionAlHUD(Transform dronacompanante)
    {
        GameObject nuevaUI = Instantiate(prefabDroneOrientation.gameObject, contenedorOrrientUI);
        nuevaUI.GetComponent<OrientationIndicatorScript>().dron3D = dron3D;
        nuevaUI.GetComponent<OrientationIndicatorScript>().dronAcompanante = dronacompanante;

    }
}
