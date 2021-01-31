using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D shipRigidBody;

    public float verticalAcceleration;
    public float turningStrength;

    private void Start()
    {
        shipRigidBody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // apply forward input
        float inlineThrusters = Input.GetAxis("Vertical") * verticalAcceleration;

        // apply turn input
        float turningThrusters = -1 * Input.GetAxis("Horizontal") * turningStrength;

        shipRigidBody.AddForce(transform.up * inlineThrusters);
        shipRigidBody.AddTorque(turningThrusters);
    }
}