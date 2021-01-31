using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonarBeam : MonoBehaviour
{
    public GameObject sonarSegment;

    public float radarWidth;
    public int radarSegmentsPerPulse;
    public int nthSegmentPings;
    public float timeBetweenPulses;

    private bool isFiring;

    float timeSincePulse = 0f;

    // Start is called before the first frame update
    void Start()
    {
        isFiring = false;
    }

    // Update is called once per frame
    void Update()
    {
        timeSincePulse += Time.fixedDeltaTime;

        isFiring = Input.GetMouseButton(1);

        if (isFiring)
        {
            if (timeSincePulse >= timeBetweenPulses)
            {
                CreatePulse();
                timeSincePulse = 0;
            }
        }
    }

    void StartFiring()
    {

    }
    void StopFiring()
    {

    }

    void CreatePulse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 clickPosition = ray.GetPoint(Math.Abs(Camera.main.transform.position.z));

        Vector2 pulseDirection = clickPosition - transform.position;
        float pulseAngle = Vector2.Angle(new Vector2(1, 0), pulseDirection);
        pulseAngle *= pulseDirection.y > 0 ? 1 : -1;

        for (int i=0; i<radarSegmentsPerPulse; i++)
        {
            float segmentAngle = pulseAngle - (radarWidth/2) + (i * radarWidth / radarSegmentsPerPulse);

            GameObject segment = Instantiate(sonarSegment, transform.position, Quaternion.Euler(0, 0, segmentAngle-90));
            segment.transform.parent = transform.parent;
            segment.GetComponent<SonarSegment>().Initialize(i % nthSegmentPings == 0, gameObject);
        }
    }
}
