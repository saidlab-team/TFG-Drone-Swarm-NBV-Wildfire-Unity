using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CameraFollowScript : MonoBehaviour
{
    // La camara vibra cuando se mueve
    public Transform drone;

    [Header("Follow Mode")]
    public Vector3 followOffset;
    public float followSmooth = 0.2f;

    [Header("FPV Mode")]
    public Vector3 fpvOffset = new Vector3(0, 0.5f, 0.5f);

    private Vector3 velocity = Vector3.zero;
    public bool fpvMode;

    private float previousValue;

    public Quaternion rot = Quaternion.identity;

    [HideInInspector] public Quaternion targetRotation = Quaternion.identity;
    private Quaternion lastTargetRotation;
    
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        //drone.GetComponent<DroneMovementScript>().moveCameraInput;
    }
    void Update()
    {
        if (drone == null) return;
        
        
        if (drone.GetComponent<DroneMovementScript>().changeCameraInput == 1f && drone.GetComponent<DroneMovementScript>().changeCameraInput != previousValue)
        {
            Debug.Log(drone.GetComponent<DroneMovementScript>().changeCameraInput );
            fpvMode = !fpvMode;
            
        }
        lastTargetRotation = rot;
        previousValue = drone.GetComponent<DroneMovementScript>().changeCameraInput;

        //Debug.Log(drone.GetComponent<DroneMovementScript>().moveCameraInput);
    }

    void LateUpdate()
    {
        if (fpvMode)
        {
            // FPV con suavizado
            Vector3 targetPos = drone.TransformPoint(fpvOffset);
            transform.position = Vector3.Lerp(transform.position, targetPos, 10f * Time.deltaTime);
            Quaternion targetRot;
            if (rot != Quaternion.identity && rot != lastTargetRotation)
            {
               targetRot = drone.rotation * rot;
            }
            else
            {
                targetRot = drone.rotation * Quaternion.Euler(20f, 0f, 0f);
            }
            

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);

            //drone.Find("animated_drone_with_camera_free").gameObject.SetActive(false);
        }
        else
        {
            // Follow externo
            Vector3 targetPos = drone.TransformPoint(followOffset);


            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, followSmooth);
            transform.position = targetPos;

            transform.position = Vector3.Lerp(transform.position, targetPos, 10f * Time.deltaTime);

            transform.LookAt(drone);

            //drone.Find("animated_drone_with_camera_free").gameObject.SetActive(true);

        }
    }
    
}
