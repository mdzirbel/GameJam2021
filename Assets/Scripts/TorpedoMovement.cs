﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoMovement : MonoBehaviour
{
    public float speed;
    public GameObject explosion;

    private bool isInitialized = false;
    private GameObject torpedoDestination;
    private GameObject parent;

    public void Initialize(GameObject destination, GameObject parentObject)
    {
        isInitialized = true;
        torpedoDestination = destination;
        parent = parentObject;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, torpedoDestination.transform.position, speed * Time.fixedDeltaTime);
        transform.up = torpedoDestination.transform.position - transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject != parent)
        {
            System.Random rnd = new System.Random();
            Quaternion rotation = Quaternion.Euler(0, 0, rnd.Next(360));
            Instantiate(explosion, transform.position, rotation);
            Destroy(gameObject);
            Destroy(torpedoDestination);
            Networking.Instance.SendExplosion(transform.position);
        }
    }
}
