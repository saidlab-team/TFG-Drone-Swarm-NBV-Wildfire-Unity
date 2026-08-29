using UnityEngine;
using UnityEngine.InputSystem;

public class UserOrders : MonoBehaviour
{
    private InputSystem_Actions m_actions;
    public LayerMask terrenoLayer;

    void Awake()
    {
        m_actions = new InputSystem_Actions();
        m_actions.Enable(); 
    }

    void OnEnable()
    {
        m_actions.Drone.Give_Orders.performed += OnGiveOrdersPerformed;
    }

    void OnDisable()
    {
        m_actions.Drone.Give_Orders.performed -= OnGiveOrdersPerformed;
    }

    private void OnGiveOrdersPerformed(InputAction.CallbackContext ctx)
    {
        Transform camTransform = Camera.main.transform;
        RaycastHit hit;

        // Lanzamos raycast desde el centro de la cámara
        if (Physics.Raycast(camTransform.position, camTransform.forward, out hit, 200f, terrenoLayer))
        {
            Debug.Log("Orden de usuario: Fuego reportado en " + hit.point);
            // Llamada estática a la radio global
            EventFireDetect.GritarFuego(hit.point);
            
            Debug.DrawRay(camTransform.position, camTransform.forward * hit.distance, Color.green, 2f);
        }
    }
}