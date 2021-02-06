using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowShip : MonoBehaviour
{
    public GameObject ship;
    public float unlockedSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(ship.activeSelf)
        {
            Vector3 cameraPos = ship.transform.position;
            cameraPos.z = transform.position.z;
            transform.position = cameraPos;
        }
        else
        {
            Vector3 cameraPos = transform.position;
            if (Input.GetKey("w"))
            {
                cameraPos.y += unlockedSpeed * Time.deltaTime;
            }
            else if (Input.GetKey("s"))
            {
                cameraPos.y -= unlockedSpeed * Time.deltaTime;
            }
            if (Input.GetKey("a"))
            {
                cameraPos.x -= unlockedSpeed * Time.deltaTime;
            }
            else if (Input.GetKey("d"))
            {
                cameraPos.x += unlockedSpeed * Time.deltaTime;
            }
            transform.position = cameraPos;
        }
    }
}
