using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMovement : MonoBehaviour
{

    private CharacterController controller;

    public float moveSpeed;
    public float jumpForce;
    public float gravityScale;

    Vector3 movementVector;


    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // COMBAT-STATE MOVEMENT [LEFT/RIGHT/JUMP]
        float xDisplacement = 0f;

        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) {
            xDisplacement = -1f;
        }
        if (!Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) {
            xDisplacement = 1f;
        }

        movementVector = new Vector3(xDisplacement * moveSpeed, movementVector.y, 0);

        if (controller.isGrounded) {
            if (Input.GetKeyDown(KeyCode.W)) {
                movementVector.y = jumpForce;
            }
        }

        movementVector.y = movementVector.y + (Physics.gravity.y * gravityScale);

        controller.Move(movementVector * Time.deltaTime);
    }
}
