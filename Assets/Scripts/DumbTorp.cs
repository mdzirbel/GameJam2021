using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DumbTorp : MonoBehaviour
{
    public float speed;
    void Start()
    {
        
    }
    void FixedUpdate()
    {
        transform.position += transform.up * Time.fixedDeltaTime * speed;
        //transform.position = Vector3.MoveTowards(transform.position, torpedoDestination.transform.position, speed * Time.fixedDeltaTime);
        //stransform.up = torpedoDestination.transform.position - transform.position;
    }
}
