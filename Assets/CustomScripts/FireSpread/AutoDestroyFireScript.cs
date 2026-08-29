using UnityEngine;
using System.Collections;

public class AutoDestroyFireScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Delete());
    }
    IEnumerator Delete()
    {
        Destroy(gameObject, 23f);  
        yield return null;
    }

}
