using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseMovement : MonoBehaviour
{

    private CharacterController controller;
    public Camera playerCamera;

    public float moveSpeed;
    public float jumpForce;
    public float gravityScale;

    private float _coyoteTimer;
    private bool _midJump;
    
    private Vector3 movementVector;

    [Header("UI Elements")] 
    public Image DoorEntryArrow;

    void Start()
    {
        movementVector = new Vector3(0, 0, 0);
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

        movementVector.x = xDisplacement * moveSpeed;

        if (controller.isGrounded)
        {
            _midJump = false;
            
            _coyoteTimer = 0f;
            movementVector.y = 0.001f;
            
            if (Input.GetKeyDown(KeyCode.W))
            {
                _midJump = true;
                movementVector.y = Mathf.Sqrt(jumpForce * -2f * gravityScale);
            }
        }
        else if ((_coyoteTimer < 0.3f) && (!_midJump))
        {
            _coyoteTimer += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.W)) 
            {
                movementVector.y = Mathf.Sqrt(jumpForce * -2f * gravityScale);
            }        
        }

        float yDisplacement;
        
        if (Input.GetKey(KeyCode.S))
        {
            yDisplacement = gravityScale * Time.deltaTime * 2;
        }
        else
        {
            yDisplacement = gravityScale * Time.deltaTime;
        }
        
        movementVector.y += yDisplacement;
        
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
        
        // PLAYER INTERACTABLE DETECTION VIA RAYCAST
        int layerMask = 255;
        // layerMask = ~layerMask;
        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, transform.TransformDirection(Vector3.forward), out hit, 5f,
                layerMask))
        {
            Collider interactableCollider = hit.collider;
            // DOOR INTERACTABLE
            if (interactableCollider.CompareTag("Door"))
            {
                Vector3 arrowPlacement3D = playerCamera.WorldToScreenPoint(interactableCollider.transform.position);
                DoorEntryArrow.rectTransform.anchoredPosition = new Vector2(arrowPlacement3D.x - (Screen.width / 2f),
                    arrowPlacement3D.y - (Screen.height / 2f));
                
                DoorTransport currentDoor = interactableCollider.GetComponent<DoorTransport>();
                
                // Interactable Input for Door to Enter
                if ((Input.GetKeyDown(KeyCode.F)) && (!currentDoor.doorLocked))
                {
                    controller.enabled = false;
                    
                    DoorTransport nextDoor = currentDoor.pairedDoor;
                    Transform nextDoorTransform = nextDoor.gameObject.transform;
                    Vector3 warpedPosition = nextDoorTransform.position;
                    warpedPosition.y -= (nextDoorTransform.localScale.y / 2f);
                    warpedPosition.y += (this.transform.localScale.y / 2f);
                    controller.transform.position = warpedPosition;

                    controller.enabled = true;
                }
            }
            else
            {
                // Hide Door Entry UI
                DoorEntryArrow.rectTransform.anchoredPosition = new Vector2((-2f) * Screen.width, (-2f) * Screen.height);
            }
        }
    }
}
