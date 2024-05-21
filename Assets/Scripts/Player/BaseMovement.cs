using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMovement : MonoBehaviour
{

    private CharacterController controller;
    public Camera playerCamera;

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

        // MOUSE MOVEMENT [alter camera view]
        Vector3 mousePos = Input.mousePosition;

        float origin_relative_mousex = mousePos.x - (Screen.width / 2);
        float origin_relative_mousey = mousePos.y - (Screen.height / 2);
        
        Vector3 newCamPos = new Vector3();
        newCamPos.x = origin_relative_mousex / (Screen.width / 3);
        newCamPos.y = origin_relative_mousey / (Screen.height / 3) + 2f;
        newCamPos.z = -14f;

        playerCamera.transform.position = transform.position + newCamPos;
    }
}
