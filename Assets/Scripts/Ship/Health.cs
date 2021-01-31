using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    private float health = 100;
    public ProgressBar Pb;
    public GameObject shipExplosionExample;
    private AudioSource hitSound;
    // Start is called before the first frame update
    void Start()
    {
        Pb = GameObject.Find("HealthBar").GetComponent<ProgressBar>();
        health = 100;
        Pb.BarValue = health;
        hitSound = GetComponent<AudioSource>();
    }

    public void DecrementHealth(float am, GameObject ship)
    {
        hitSound.Play();

        health -= am;
        Pb.BarValue = health; 
        if (health <= 0)
        {
            //Debug.Log(ship.transform.position);
            var explosion = Instantiate(shipExplosionExample, ship.transform.position, Quaternion.identity);
            ship.SetActive(false);
            Networking.Instance.SendShipExplosion(ship.transform.position);
            Networking.Instance.SendDied();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
