using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTorpedo : MonoBehaviour
{
    public GameObject torpedo;
    public GameObject torpedoDestination;

    private ShipState shipState;
    // Start is called before the first frame update
    void Start()
    {
        shipState = GetComponent<ShipState>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && shipState.canFireMissile)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 clickPosition = ray.GetPoint(Math.Abs(Camera.main.transform.position.z));
            clickPosition.z = 0;
            Fire(clickPosition);
            //Debug.Log(ray.GetPoint(Math.Abs(Camera.main.transform.position.z)));
        }
    }
    void Fire(Vector3 destination)
    {
        GameObject torpedoDestinationInstance = Instantiate(torpedoDestination, destination, Quaternion.identity);
        GameObject torpedoInstance = Instantiate(torpedo, transform.position, Quaternion.identity);
        torpedoInstance.GetComponent<TorpedoMovement>().Initialize(torpedoDestinationInstance);
    }
}
