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

        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) 
        {
            xDisplacement = -1f;
        }
        if (!Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) 
        {
            xDisplacement = 1f;
        }

        movementVector = new Vector3(xDisplacement * moveSpeed, movementVector.y, 0);

        float localGravityScale = gravityScale;
        if (controller.isGrounded) 
        {
            movementVector.y = 0.001f;
            if (Input.GetKeyDown(KeyCode.W)) 
            {
                movementVector.y = jumpForce;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.S))
            {
                localGravityScale *= 3f;
            }
        }

        movementVector.y = movementVector.y + (Physics.gravity.y * localGravityScale);
        controller.Move(movementVector * Time.deltaTime);

        // CAMERA MOVEMENT + AIM (mouse position)
        Vector3 mousePos = Input.mousePosition;

        // divide screen width/height by 2 to get the edge-to-origin length
        float origin_relative_mousex = mousePos.x - (Screen.width / 2f);
        float origin_relative_mousey = mousePos.y - (Screen.height / 2f);
        
        Vector3 newCamPos = new Vector3();

        newCamPos.x = (Mathf.Abs(origin_relative_mousex) < (Screen.width / 2f)) ? (origin_relative_mousex) : ((origin_relative_mousex < 0f) ? (-1f * Screen.width / 2f) : (Screen.width / 2f));
        newCamPos.y = (Mathf.Abs(origin_relative_mousey) < (Screen.height / 2f)) ? (origin_relative_mousey) : ((origin_relative_mousey < 0f) ? (-1f * Screen.height / 2f) : (Screen.height / 2f));
        
        // THE FOLLOWING ARE MAGIC NUMBERS FOR NOW, MAKE THEM CHANGEABLE VARIABLES LATER ON FOR CUTSCENES?
        // 130f is a cam scale factor, 
        // 3f is a cam y-shift factor,
        // -18f is a cam zoom factor

        newCamPos.x /= 130f;
        newCamPos.y /= 130f;
        newCamPos.y += 3f;
        newCamPos.z = -16f;

        playerCamera.transform.position = transform.position + newCamPos;
    }
}
