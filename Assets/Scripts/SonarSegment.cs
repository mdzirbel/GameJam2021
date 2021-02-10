using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonarSegment : MonoBehaviour
{
    public float speed;
    public float timeoutTime;
    public GameObject ping;

    private float existedTime = 0;
    private GameObject parent;
    private bool pingsOnHit;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Initialize(bool givesPing, GameObject parentObject)
    {
        pingsOnHit = givesPing;
        parent = parentObject;

        //if (!givesPing)
        //{
        //    Destroy(gameObject.GetComponent<CapsuleCollider2D>());
        //}
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += transform.up * speed * Time.fixedDeltaTime;
        existedTime += Time.fixedDeltaTime;
        if (existedTime > timeoutTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.name.Contains("Torp"))
        {
            return;
        }
        if (other.gameObject != parent)
        {
            if (pingsOnHit && !other.gameObject.name.Contains("Wall"))
            {
                Networking.Instance.SendPing(transform.position);
                GameObject pingInstance = Instantiate(ping, transform.position, Quaternion.identity);
            }
            pingsOnHit = false;
            Destroy(gameObject);
        }
    }
}
