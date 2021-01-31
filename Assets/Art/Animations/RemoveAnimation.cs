using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveAnimation : MonoBehaviour
{
    public float TimeUntilRemove;

    // Update is called once per frame
    void FixedUpdate()
    {
        TimeUntilRemove -= Time.fixedDeltaTime;
        if (TimeUntilRemove <= 0)
        {
            Destroy(gameObject);
        }
    }
}
