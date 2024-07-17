using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseMovement : MonoBehaviour
{

    private CharacterController controller;
    private Animator _animator;
    private BoxCollider _playerCollider;

    private string currentState;
    
    // ANIMATOR STATES

    private const string STAND_WALK = "Armature|WALK_FORWARD_UNARMED";
    private const string STAND_IDLE = "Armature|IDLE_UNARMED";
    private const string CROUCH_WALK = "CRAWL_FORWARD_UNARMED";
    private const string CROUCH_IDLE = "CROUCH_IDLE_UNARMED";
    private const string STAND_JUMP = "Armature|JUMP_START_UNARMED";
    private const string RUN = "RUN";
    private const string SLIDE = "Armature|SLIDE";
    
    public Camera playerCamera;

    [Header("X Movement")]
    [Range(0f,5f)]public float moveSpeed;
    [Range(1f,2f)]public float runSpeedMultiplier;
    [Range(0f,1f)]public float crouchSpeedMultiplier;
    
    [Header("Y Movement")]
    public float jumpForce;
    public float gravityScale;

    private float _coyoteTimer;
    private bool _midJump;
    private bool _previousIsGrounded;
    
    [Header("Others...")]
    public bool running;
    
    public bool _sliding;
    private float _slideCounter;
    public float slideTimeLength;
    private int _slideDirectionSign;
    private bool _playerLanded;

    private float _playerTakeOffSpeed;
    private bool _playerTakeOffLeft;
    private bool _playerTakeOffRight;
    private bool _playerTakeOffMomentumMaintained;

    private bool _previousRunning;
    private float _previousMovementSpeed;
    private float _runJumpSavedSpeed;

    public bool _crouchingGrounded;
    public bool _crouchingCeilingLocked;
    
    private Vector3 movementVector;

    [Header("UI Elements")] 
    public Image DoorEntryArrow;

    void Start()
    {
        movementVector = new Vector3(0, 0, 0);
        controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _playerCollider = this.GetComponent<BoxCollider>();
    }

    void Update()
    {
        
        // COMBAT-STATE MOVEMENT [LEFT/RIGHT/JUMP]
        float xDisplacement = 0f;

        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) 
        {
            xDisplacement = -(1f) * moveSpeed;
        }
        if (!Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) 
        {
            xDisplacement = moveSpeed;
        }

        movementVector.x = xDisplacement * moveSpeed;

        if (controller.isGrounded)
        {
            _midJump = false;
            
            _coyoteTimer = 0f;
            movementVector.y = 0.001f;
            
            if (Input.GetKeyDown(KeyCode.W) && (!_crouchingCeilingLocked))
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

        // CROUCHING
        if (Input.GetKey(KeyCode.S))
        {
            yDisplacement = gravityScale * Time.deltaTime * 2;

            if (controller.isGrounded)
            {
                _crouchingGrounded = true;
                movementVector.x *= crouchSpeedMultiplier;
                _crouchingCeilingLocked = false;
            }
            else
            {
                _crouchingGrounded = false;
                _crouchingCeilingLocked = false;
            }
            
            controller.height = 5.8125f;
            controller.radius = 2f;
            controller.center = new Vector3(0f, 2.9f, 0f);
            _playerCollider.size = new Vector3(4f, 5.8125f, 4f);
            _playerCollider.center = new Vector3(0f, 2.9f, 0f);
        }
        else
        {
            yDisplacement = gravityScale * Time.deltaTime;
            
            // CHECK IF PLAYER IS ALLOWED TO UNCROUCH WITH OVERHEAD RAYCAST
            LayerMask layerMaskAbove = LayerMask.GetMask("Wall");
            RaycastHit hitAbove;
            if (!Physics.Raycast(this.transform.position, transform.TransformDirection(Vector3.up), out hitAbove,
                    3.4875f,
                    layerMaskAbove))
            {

                controller.height = 9.3f;
                controller.radius = 2f;
                controller.center = new Vector3(0f, 4.632017f, 0f);
                _playerCollider.size = new Vector3(4f, 9.3f, 4f);
                _playerCollider.center = new Vector3(0f, 4.632017f, 0f);

                _crouchingGrounded = false;
                _crouchingCeilingLocked = false;
            }
            else
            {
                movementVector.x *= crouchSpeedMultiplier;
                _crouchingCeilingLocked = true;
                _crouchingGrounded = true;
            }
        }
        
        
        Vector3 mousePos = Input.mousePosition;
        
        // divide screen width/height by 2 to get the edge-to-origin length
        float origin_relative_mousex = mousePos.x - (Screen.width / 2f);
        float origin_relative_mousey = mousePos.y - (Screen.height / 2f);
        
        if (Input.GetKey(KeyCode.LeftShift) && !_crouchingGrounded && !_crouchingCeilingLocked && controller.isGrounded && 
            (((origin_relative_mousex < 0f) && (movementVector.x < 0f)) || ((origin_relative_mousex > 0f) && (movementVector.x > 0f))))
        {
            running = true;
            movementVector.x *= runSpeedMultiplier;
        }
        else
        {
            running = false;
        }

        if (_playerTakeOffMomentumMaintained)
        {
            if ((_playerTakeOffLeft == Input.GetKey(KeyCode.A)) && (_playerTakeOffRight == Input.GetKey(KeyCode.D)) && !controller.isGrounded)
            {
                _playerTakeOffMomentumMaintained = true;
                movementVector.x = _playerTakeOffSpeed;
            }
            else
            {
                _playerTakeOffMomentumMaintained = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("gerf");
        }
        
        // Debug.Log(controller.isGrounded + " " + _crouchingCeilingLocked + " " + Input.GetKeyDown(KeyCode.S) + " " + running);
        if ((controller.isGrounded && !_crouchingCeilingLocked && Input.GetKeyDown(KeyCode.S) && _previousRunning) || (_playerLanded && Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftShift)))
        {
            _sliding = true;
            _slideCounter = 0f;

            if (movementVector.x < 0f)
            {
                _slideDirectionSign = -1;
            }
            else
            {
                _slideDirectionSign = 1;
            }
        }

        if (_sliding)
        {
            // SLIDE CANCEL IF OPPOSITE DIRECTION, JUMP, FALL, OR CROUCH RELEASE
            if (_slideDirectionSign == -1f)
            {
                if ((movementVector.x > 0f) || !controller.isGrounded)
                {
                    _sliding = false;
                }
            }
            else
            {
                if ((movementVector.x < 0f) || !controller.isGrounded)
                {
                    _sliding = false;
                }            
            }
            
            if (_slideCounter < slideTimeLength)
            {
                running = false; // set running to false so that player can shoot all directions when sliding
                
                _slideCounter += Time.deltaTime;
                
                // movementVector.x = directionSign * (((runSpeed - crouchSpeed) * (timeScale)) + crouchSpeed)
                // ---------------> = sign * (((max - min) * (scale)) + min)
                
                // so that the sliding movement speed decreases to a clamped minimum, rather than 0
                // this prevents sliding from hindering flow of player movement
                movementVector.x = _slideDirectionSign *
                                   ((moveSpeed * moveSpeed * (runSpeedMultiplier - crouchSpeedMultiplier)) *
                                   (1 - (_slideCounter / slideTimeLength)) + (moveSpeed * moveSpeed * crouchSpeedMultiplier));
            }
            else
            {
                _sliding = false;   
            }

            // SLIDE CANCEL IF MOVEMENT HALTS (HITTING A WALL)
            if (controller.velocity.x == 0f)
            {
                _sliding = false;
            }

            if (controller.isGrounded)
            {
                controller.height = 3.03f;
                controller.center = new Vector3(0f, 2.12f, 0f);
                controller.radius = 2.15f;
                _playerCollider.size = new Vector3(4f, 4.3f, 8.5f);
                _playerCollider.center = new Vector3(0f, 2.12f, 0f);
            }
        }
        
        movementVector.y += yDisplacement;
        controller.Move(movementVector * Time.deltaTime);
        Vector3 finalPosition = new Vector3(transform.position.x, transform.position.y, 0f);
        transform.position = finalPosition; // anti-clipping safety measure
        
        // SETTING ANIMATOR STATES
        if (controller.isGrounded)
        {
            if (movementVector.x != 0)
            {
                if (_sliding)
                {
                    SetAnimationState(SLIDE);
                }
                else if (running)
                {
                    SetAnimationState(RUN);
                }
                else if (_crouchingGrounded)
                {
                    SetAnimationState(CROUCH_WALK);
                }
                else
                {
                    SetAnimationState(STAND_WALK);
                }
            }
            else
            {
                if (_crouchingGrounded)
                {
                    SetAnimationState(CROUCH_IDLE);
                }
                else
                {
                    SetAnimationState(STAND_IDLE);
                }
            }
        }
        else
        {
            SetAnimationState(STAND_JUMP);
        }
        
        // CAMERA MOVEMENT + AIM (mouse position)
        
        Vector3 newCamPos = new Vector3();

        newCamPos.x = (Mathf.Abs(origin_relative_mousex) < (Screen.width / 2f)) ? (origin_relative_mousex) : ((origin_relative_mousex < 0f) ? (-1f * Screen.width / 2f) : (Screen.width / 2f));
        newCamPos.y = (Mathf.Abs(origin_relative_mousey) < (Screen.height / 2f)) ? (origin_relative_mousey) : ((origin_relative_mousey < 0f) ? (-1f * Screen.height / 2f) : (Screen.height / 2f));
        
        // Flip player Scale.z if mouse is facing left side of screen

        if (origin_relative_mousex < 0)
        {
            this.transform.eulerAngles = new Vector3(0f, -90f, 0f);
        }
        else
        {
            this.transform.eulerAngles = new Vector3(0f, 90f, 0f);
        }
        
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
        if (Physics.Raycast(this.transform.position, new Vector3(0f, 0f, 1f), out hit, 5f,
                layerMask))
        {
            Collider interactableCollider = hit.collider;
            
            // DOOR INTERACTABLE
            if (interactableCollider.CompareTag("Door") && (controller.isGrounded))
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
                    warpedPosition.z = 0f; // KEEP PLAYER ON Z = 0 FOR PROPER PLATFORM ALIGNMENT
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

    private void LateUpdate()
    {
        if (!_previousIsGrounded && controller.isGrounded)
        {
            _playerLanded = true;
        }
        else
        {
            _playerLanded = false;
        }

        if (_previousIsGrounded && !controller.isGrounded)
        {
            _playerTakeOffSpeed = movementVector.x;
            _playerTakeOffMomentumMaintained = true;
            _playerTakeOffLeft = Input.GetKey(KeyCode.A);
            _playerTakeOffRight = Input.GetKey(KeyCode.D);
        }
        
        _previousIsGrounded = controller.isGrounded;
        _previousRunning = running;
        _previousMovementSpeed = movementVector.x;    
    }

    void SetAnimationState(string nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        _animator.CrossFade(nextState, 0.1f);
        currentState = nextState;
    }
}
