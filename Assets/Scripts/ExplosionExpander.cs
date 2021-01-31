using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ExplosionExpander : MonoBehaviour
{
    public float expansionSpeed;
    public float maxSize;
    public float expansionRadius = 0;
    public float damageAtCenter;
    public float currentDamage = 0;
    public bool doesDamage = true;
    private GameObject OurShip;
    private Health shipHealth;
    private bool enabled = false;
    // Start is called before the first frame update
    void Start()
    {
        OurShip = GameObject.Find("OurShip");
        currentDamage = damageAtCenter;
        if (OurShip!=null)
        {
            enabled = true;
            shipHealth = OurShip.GetComponent<Health>();
        }
        else
        {
            Debug.Log("Tried to do explosion damage but we are not alive so no point");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (enabled)
        {
            CircleCollider2D myCollider = transform.GetComponent<CircleCollider2D>();
            float maybeSize = expansionRadius + (expansionSpeed) * Time.fixedDeltaTime;
            if (maybeSize <= maxSize)
            {
                expansionRadius = maybeSize;
            }
            else
            {
                expansionRadius = maxSize;
                doesDamage = false;
            }
            myCollider.radius = expansionRadius;

            float percDone = (maxSize - expansionRadius) / maxSize;
            currentDamage = damageAtCenter * percDone;
            if (currentDamage < .1)
            {
                myCollider.enabled = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == OurShip.name)
        {
            shipHealth.DecrementHealth(currentDamage, OurShip);
            Debug.Log(currentDamage + " damage to us!");
        }
    }
}
