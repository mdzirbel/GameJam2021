using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D shipRigidBody;

    public float verticalAcceleration;
    public float turningStrength;

    private void Start()
    {
        shipRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // apply forward input
        float inlineThrusters = Input.GetAxis("Vertical") * verticalAcceleration;

        // apply turn input
        float turningThrusters = -1 * Input.GetAxis("Horizontal") * turningStrength;

        shipRigidBody.AddForce(transform.up * inlineThrusters);
        shipRigidBody.AddTorque(turningThrusters);
    }

    //private void FixedUpdate()
    //{
    //    // apply velocity drag
    //    velocity = velocity * (1 - Time.deltaTime * velocityDrag);

    //    // clamp to maxSpeed
    //    velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

    //    // apply rotation drag
    //    zRotationVelocity = zRotationVelocity * (1 - Time.deltaTime * rotationDrag);

    //    // clamp to maxRotationSpeed
    //    zRotationVelocity = Mathf.Clamp(zRotationVelocity, -maxRotationSpeed, maxRotationSpeed);

    //    // update transform
    //    transform.position += velocity * Time.deltaTime;
    //    transform.Rotate(0, 0, zRotationVelocity * Time.deltaTime);
    //}
}