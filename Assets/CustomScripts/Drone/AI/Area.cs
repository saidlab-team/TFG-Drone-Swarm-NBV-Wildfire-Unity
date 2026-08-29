using UnityEngine;


public class Area : MonoBehaviour
{
    public float radius = 20f;
    public float minDistance = 40f;
    public float minHeight = 50f;
    public LayerMask terrainLayer;
    private float groundHeight; 
    bool hitComfirm = false;


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    
    }
    void Update()
    {
        /*
        if (!hitComfirm){
            Vector3 origin = transform.position + Vector3.up; // Origen del rayo 
            Vector3 direction = Vector3.down;
            float maxDistance = 200f;
            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                    hitComfirm = true;
                    Debug.Log ("HIT TERRAIN CONFIRM");
                }
            }
        }*/
    }

    Vector3 lastPosition;
    public Vector3 GetRandomPoint()
    {

        int attempts = 0;
        int maxAttempts = 20;
        Vector3 randomPoint;
        do
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomPoint = transform.position + randomDirection;
            attempts++;
            if (randomPoint.y < minHeight)
            {
                randomPoint.y = minHeight;
            }else if (randomPoint.y > 200f) {
                randomPoint.y = 200f;
            }
            
        } while (Vector3.Distance(randomPoint, lastPosition) < minDistance && attempts < maxAttempts );
        return randomPoint;

    } 

    public void MoveAreaCenter(Vector3 newCenter)
    {
        transform.position = newCenter;
    }
    public void ChangeAreaRadius(float newRadius)
    {
        radius = newRadius;
    }


}

