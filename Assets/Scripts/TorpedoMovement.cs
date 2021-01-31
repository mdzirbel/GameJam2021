using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoMovement : MonoBehaviour
{
    public float speed;

    private bool isInitialized = false;
    private GameObject torpedoDestination;

    public void Initialize(GameObject destination)
    {
        isInitialized = true;
        torpedoDestination = destination;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, torpedoDestination.transform.position, speed * Time.fixedDeltaTime);
        transform.up = torpedoDestination.transform.position - transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit");
    }
}
