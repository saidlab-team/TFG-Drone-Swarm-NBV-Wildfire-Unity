using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    //[HideInInspector]
    //public NavMeshAgent agent;
    public Camera camera;
    
    [HideInInspector]
    public Animator animator;

    [HideInInspector]
    public NpcMovement movement;

    //public float CurrentSpeed
    //{
    //    get {return agent.velocity.magnitude;}
    //}

    [HideInInspector]
    public Vector3 pos;

    

    private void Awake()
    {
        //agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        movement = GetComponent<NpcMovement>();
        pos = transform.position;
    }
    
}
