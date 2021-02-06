using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoMovement : MonoBehaviour
{
    public float speed;
    public GameObject explosion;
    public GameObject exampleSonar;
    public static byte torpNum = 0;
    private bool isInitialized = false;
    private GameObject torpedoDestination;
    private GameObject parent;
    private byte myTorpNum;

    public void Initialize(GameObject destination, GameObject parentObject)
    {
        isInitialized = true;
        torpedoDestination = destination;
        parent = parentObject;
        transform.position = Vector3.MoveTowards(transform.position, torpedoDestination.transform.position, speed * Time.fixedDeltaTime);
        transform.up = torpedoDestination.transform.position - transform.position;
        myTorpNum = torpNum++;
        Networking.Instance.CreateTorp(myTorpNum, transform.position.x, transform.position.y, transform.eulerAngles.z);
    }

    void FixedUpdate()
    {
        transform.position += transform.up * Time.fixedDeltaTime * speed;
        //transform.position = Vector3.MoveTowards(transform.position, torpedoDestination.transform.position, speed * Time.fixedDeltaTime);
        //transform.up = torpedoDestination.transform.position - transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isSonar = other.gameObject.name.Contains("Sonar");
        if (isSonar)
        {
            return;
        }
        if (other.gameObject != parent)
        {
            Networking.Instance.SendExplosion(transform.position);
            Networking.Instance.DestroyTorp(myTorpNum);
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
            Destroy(torpedoDestination);
        }
    }
}
