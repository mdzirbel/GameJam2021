using System.Collections;
using UnityEngine;

public class Zoom : MonoBehaviour
{
    public float zoomScale;
    public float minZoom;
    public float maxZoom;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Clamp(transform.position.z + Input.mouseScrollDelta.y * zoomScale, -maxZoom, - minZoom));
    }
}
