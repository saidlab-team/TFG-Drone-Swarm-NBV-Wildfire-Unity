using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class NpcMovement : MonoBehaviour
{
    public Transform drone;
    // bool travelling;
    DroneMovementScript movementScript;
    private Coroutine waitArrivalCoroutine;

    


    bool readyToMove = true; // false -> no se mueve, true -> sí se mueve


    void Awake()
    {
        movementScript = GetComponent<DroneMovementScript>();
    }


    void OnDrawGizmosSelected(){
        if (movementScript != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(movementScript.targetPosition, 2);
        }
        
    }

    public void StopDestination()
    {
        // cuando detecta un fuego, tiene que ir a esa posicion y apuntar con la camara del dron (camara + collider?)
        if (!readyToMove)
        {
            StopCoroutine(waitArrivalCoroutine);
            movementScript.targetPosition = drone.transform.position; // hace que se pare?
            readyToMove = true;
            //travelling = false;
        }
       
    }

    public void SetDestination (Vector3 destination){
        if (readyToMove)
        {   
            //Debug.Log($"DESTINO ESTABLECIDO --> {destination}");
            readyToMove = false;
            movementScript.targetPosition = destination;
            waitArrivalCoroutine = StartCoroutine(waitForArrival());
        }
    }

    

    public void SetDestinationGivenLast(Vector3 lastPosition)
    {
        
        if (readyToMove)
        {
            readyToMove = false;
            float rango= 20f;
            Vector3 nuevaPosicion;
            int intentos = 0;
            do
            {
                float randomX = Random.Range(-rango, rango);
                float randomZ = Random.Range(-rango, rango);
                nuevaPosicion = new Vector3(
                    transform.position.x + randomX,
                    transform.position.y,
                    transform.position.z + randomZ
                );
                intentos++;
            } while (Vector3.Distance(nuevaPosicion, lastPosition) < 10f && intentos < 10);   
            movementScript.targetPosition = nuevaPosicion;
            Debug.Log($"Nueva location {nuevaPosicion}");
            waitArrivalCoroutine = StartCoroutine(waitForArrival());
        }
    }
    


    public bool IsDroneReadyToMove()
    {
        return readyToMove;
    }

    IEnumerator waitForArrival()
    {
        yield return new WaitUntil(() => movementScript.hasArrived);
        // reset para la próxima vez
        Debug.Log("Llegada a destino");
        yield return new WaitForSeconds(5f); // Cooldoewn
        readyToMove = true;
        movementScript.hasArrived = false; 


    }

    

}
