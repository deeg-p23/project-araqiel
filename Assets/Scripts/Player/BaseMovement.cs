using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseMovement : MonoBehaviour
{

    public CharacterController controller;
    private Animator _animator;
    public BoxCollider _playerCollider;

    private string currentState;
    
    // ANIMATOR STATES

    private const string STAND_WALK = "Armature|WALK_FORWARD_UNARMED";
    private const string STAND_IDLE = "Armature|IDLE_UNARMED";
    private const string CROUCH_WALK = "CRAWL_FORWARD_UNARMED";
    private const string CROUCH_IDLE = "CROUCH_IDLE_UNARMED";
    private const string STAND_JUMP = "Armature|JUMP_START_UNARMED";
    private const string RUN = "RUN";
    private const string SLIDE = "Armature|SLIDE";
    private const string HANG = "HANG";
    
    public Camera playerCamera;

    [Header("X Movement")]
    [Range(0f,5000f)]public float moveSpeed;
    [Range(0f,2f)]public float runSpeedMultiplier;
    [Range(0f, 1f)] public float crouchSpeedMultiplier;
    
    [Header("Y Movement")]
    [Range(0f,5000f)]public float jumpForce;
    [Range(-5000f,5000f)]public float gravityScale;
    [Range(0f,1f)]public float coyoteWindowMax;

    private float _previousYVel;
    private float _YVel;
    private bool _jumping;
    public bool jumpRising;

    private float _coyoteTimer;
    private bool _midJump;
    private bool _previousIsGrounded;
    
    private int _overlapSphereFrameCheck;
    public Vector3 ledgePoint;
    public bool ledgeGrabbed;
    public int ledgeHangDirection;
    public float ledgeGrabCooldown;
    public GameObject ledgeGrabber;
    
    [Header("Others...")]
    [Range(0f, 100f)]public float cameraShiftDamp;
    public bool running;

    public int mouseXDirection;

    public bool isGrounded;
    private ControllerColliderHit groundHit;
    private bool edgeCorrecting;
    
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


    private bool leftKey;
    private bool rightKey;
    private bool downKey;
    private bool jumpKey;
    private bool runKey;
    private bool interactKey;

    private bool leftKeyDown;
    private bool rightKeyDown;
    private bool downKeyDown;
    private bool jumpKeyDown;
    private bool runKeyDown;
    private bool interactKeyDown;

    private bool leftKeyUp;
    private bool rightKeyUp;
    private bool downKeyUp;
    private bool jumpKeyUp;
    private bool runKeyUp;
    private bool interactKeyUp;

    private Vector3 mousePos;

    private LayerMask layerMaskWall;

    void Start()
    {
        movementVector = new Vector3(0, 0, 0);
        controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        layerMaskWall = LayerMask.GetMask("Wall");
    }

    void Update()
    {
        // TEMP INPUT HANDLING
        // hold
        leftKey = Input.GetKey(KeyCode.A);
        rightKey = Input.GetKey(KeyCode.D);
        downKey = Input.GetKey(KeyCode.S);
        jumpKey = Input.GetKey(KeyCode.W);

        runKey = Input.GetKey(KeyCode.LeftShift);

        interactKey = Input.GetKey(KeyCode.F);
        
        // down
        leftKeyDown = Input.GetKeyDown(KeyCode.A);
        rightKeyDown = Input.GetKeyDown(KeyCode.D);
        downKeyDown = Input.GetKeyDown(KeyCode.S);
        jumpKeyDown = Input.GetKeyDown(KeyCode.W);

        runKeyDown = Input.GetKeyDown(KeyCode.LeftShift);

        interactKeyDown = Input.GetKeyDown(KeyCode.F);
        
        // up
        leftKeyUp = Input.GetKeyUp(KeyCode.A);
        rightKeyUp = Input.GetKeyUp(KeyCode.D);
        downKeyUp = Input.GetKeyUp(KeyCode.S);
        jumpKeyUp = Input.GetKeyUp(KeyCode.W);

        runKeyUp = Input.GetKeyUp(KeyCode.LeftShift);

        interactKeyUp = Input.GetKeyUp(KeyCode.F);
        
        // mouse
        mousePos = Input.mousePosition;
        
        
        // /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 
        // BASE MOVEMENT
        //
        // /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        float originRelativeMouseX = mousePos.x - (Screen.width / 2f);
        float originRelativeMouseY = mousePos.y - (Screen.height / 2f);

        if (originRelativeMouseX < 0f)
        {
            mouseXDirection = -1;
        }
        else
        {
            mouseXDirection = 1;
        }
        
        // LEFT/RIGHT MOVEMENT :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: //
        int xDirection = 0;

        if (leftKey && !rightKey) 
        {
            xDirection = -1;
        }
        if (!leftKey && rightKey) 
        {
            xDirection = 1;
        }
        
        movementVector.x = xDirection * moveSpeed;
        
        // JUMPING & FALLING (GRAVITY) :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: //
        isGrounded = controller.isGrounded;
        
        if (isGrounded)
        {
            _jumping = false;

            if (movementVector.x == 0)
            {
                if (!Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), 0.1f, layerMaskWall))
                {
                    edgeCorrecting = true;
                
                    Vector3 edgeFallMovement = transform.position - groundHit.point;
                    edgeFallMovement.y = 0;
                    movementVector += (edgeFallMovement);   
                }
                else
                {
                    edgeCorrecting = false;
                }   
            }
        }
        else
        {
            edgeCorrecting = false;
        }
        
        if ((!_jumping && isGrounded) || (ledgeGrabbed))
        {
            _YVel = 0f;
            _coyoteTimer = 0f;
            
            if (jumpKeyDown && !_crouchingCeilingLocked)
            {
                ledgeGrabbed = false;

                _previousYVel = jumpForce;
                jumpRising = true;
                _jumping = true;
            }
        }
        
        if (!isGrounded || jumpRising)
        {
            _YVel = _previousYVel + gravityScale * Time.deltaTime;
            movementVector.y = _YVel + (0.5f) * gravityScale * Time.deltaTime;
            
            
            if (((jumpRising) && ((movementVector.y <= 0f) || jumpKeyUp)))
            {
                jumpRising = false;
                _YVel /= 2f;
            }

            if ((_coyoteTimer < coyoteWindowMax) && (!_jumping))
            {
                _coyoteTimer += Time.deltaTime;
                if (jumpKey)
                {
                    _YVel = jumpForce;
                    _jumping = true;
                }
            }
        }
        
        if (jumpRising &&
            (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), 17f, layerMaskWall)))
        {
            jumpRising = false;
            _YVel = (Mathf.Abs(_YVel) == _YVel) ? (-1) * _YVel / 2f : _YVel;
        }

        // CROUCHING
        if (downKey)
        {
            if (!isGrounded)
            {
                if (ledgeGrabbed)
                {
                    ledgeGrabCooldown = 0f;
                    ledgeGrabbed = false;
                    movementVector.y = 0f;
                }
                
                if (movementVector.y < 0f)
                {
                    movementVector.y *= 2f;
                }
                else
                {
                    movementVector.y /= 2f;
                } 
                
                _crouchingGrounded = false;
                _crouchingCeilingLocked = false;
            }
            else            
            {
                _crouchingGrounded = true;
                movementVector.x *= crouchSpeedMultiplier;
                _crouchingCeilingLocked = false;
            }
            
            controller.height = 5.8125f;
            controller.radius = 1.5f;
            controller.center = new Vector3(0f, 2.9f, 0f);
            _playerCollider.size = new Vector3(3f, 5.8125f, 3f);
            _playerCollider.center = new Vector3(0f, 2.9f, 0f);
        }
        else
        {
            // yDisplacement = gravityScale * Time.deltaTime;
            
            // CHECK IF PLAYER IS ALLOWED TO UNCROUCH WITH OVERHEAD RAY CAST
            LayerMask layerMaskAbove = LayerMask.GetMask("Wall");
            if (!Physics.Raycast(this.transform.position, transform.TransformDirection(Vector3.up),
                    18f,
                    layerMaskAbove))
            {
                controller.height = 9.3f;
                controller.radius = 1.5f;
                controller.center = new Vector3(0f, 4.632017f, 0f);
                _playerCollider.size = new Vector3(3f, 9.3f, 3f);
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

        if (runKey && !_crouchingGrounded && !_crouchingCeilingLocked && isGrounded && 
            (((originRelativeMouseX < 0f) && (movementVector.x < 0f)) || ((originRelativeMouseX > 0f) && (movementVector.x > 0f))))
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
            if ((_playerTakeOffLeft == leftKey) && (_playerTakeOffRight == rightKey) && !isGrounded)
            {
                _playerTakeOffMomentumMaintained = true;
                movementVector.x = _playerTakeOffSpeed;
            }
            else
            {
                _playerTakeOffMomentumMaintained = false;
            }
        }
        
        if ((isGrounded && !_crouchingCeilingLocked && downKeyDown && _previousRunning) || (_playerLanded && downKey && runKey))
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
            if (_slideDirectionSign == -1)
            {
                if ((movementVector.x > 0f) || !isGrounded)
                {
                    _sliding = false;
                }
            }
            else
            {
                if ((movementVector.x < 0f) || !isGrounded)
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
                                   ((moveSpeed * (runSpeedMultiplier - crouchSpeedMultiplier)) *
                                   (1 - (_slideCounter / slideTimeLength)) + (moveSpeed * crouchSpeedMultiplier));
            }
            else
            {
                _sliding = false;   
            }

            
            // SLIDE CANCEL IF MOVEMENT HALTS (HITTING A WALL)
            if ((controller.velocity.x == 0f) && (!_playerLanded))
            {
                _sliding = false;
            }

            if (isGrounded)
            {
                controller.height = 3.03f;
                controller.center = new Vector3(0f, 2.12f, 0f);
                controller.radius = 2.15f;
                _playerCollider.size = new Vector3(4f, 4.3f, 8.5f);
                _playerCollider.center = new Vector3(0f, 2.12f, 0f);
            }
        }

        if (!isGrounded)
        {
            controller.height = 7.65f;
            controller.radius = 1.5f;
            controller.center = new Vector3(0f, 4.632017f, 0f);
            _playerCollider.size = new Vector3(3f, 7.65f, 3f);
            _playerCollider.center = new Vector3(0f, 4.632017f, 0f);
        }


        if (ledgeGrabCooldown < 0.1f)
        {
            ledgeGrabCooldown += Time.deltaTime;
            ledgeGrabber.SetActive(false);
        }
        else
        {
            ledgeGrabber.SetActive(true);
        }
        
        if (ledgeGrabbed)
        {
            transform.position = ledgePoint;
            movementVector = Vector3.zero;
            _YVel = 0f;
        }
        
        controller.Move(movementVector * Time.deltaTime);
        Vector3 finalPosition = new Vector3(transform.position.x, transform.position.y, 0f);
        transform.position = finalPosition; // anti-clipping safety measure
        
        // SETTING ANIMATOR STATES
        string newState;
        
        if (isGrounded)
        {
            if ((edgeCorrecting) || (movementVector.x == 0))
            {
                newState = _crouchingGrounded ? CROUCH_IDLE : STAND_IDLE;
            }
            else
            {
                if (_sliding)
                {
                    newState = SLIDE;
                }
                else if (running)
                {
                    newState = RUN;
                }
                else if (_crouchingGrounded)
                {
                    newState = CROUCH_WALK;
                }
                else
                {
                    newState = STAND_WALK;
                }
            }
        }
        else
        {
            if (ledgeGrabbed)
            {
                newState = HANG;
            }
            else
            {
                newState = STAND_JUMP;
            }
        }
        SetAnimationState(newState);
        
        // PLAYER INTERACTABLE DETECTION VIA RAY CAST
        int layerMask = 255;
        // layerMask = ~layerMask;
        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, new Vector3(0f, 0f, 1f), out hit, 5f,
                layerMask))
        {
            Collider interactableCollider = hit.collider;
            
            // DOOR INTERACTABLE
            if (interactableCollider.CompareTag("Door") && (isGrounded))
            {
                Vector3 arrowPlacement3D = playerCamera.WorldToScreenPoint(interactableCollider.transform.position);
                DoorEntryArrow.rectTransform.anchoredPosition = new Vector2(arrowPlacement3D.x - (Screen.width / 2f),
                    arrowPlacement3D.y - (Screen.height / 2f));
                
                DoorTransport currentDoor = interactableCollider.GetComponent<DoorTransport>();
                
                // Interactable Input for Door to Enter
                if (interactKeyDown && (!currentDoor.doorLocked))
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
        else
        {
            DoorEntryArrow.rectTransform.anchoredPosition = new Vector2((-2f) * Screen.width, (-2f) * Screen.height);
        }
    }

    private void LateUpdate()
    {
        _previousYVel = _YVel;
        
        if (!_previousIsGrounded && isGrounded)
        {
            _playerLanded = true;
        }
        else
        {
            _playerLanded = false;
        }

        if (_previousIsGrounded && !isGrounded)
        {
            _playerTakeOffSpeed = _previousMovementSpeed;
            _playerTakeOffMomentumMaintained = true;
            _playerTakeOffLeft = leftKey;
            _playerTakeOffRight = rightKey;
        }
        
        _previousIsGrounded = isGrounded;
        _previousRunning = running;
        _previousMovementSpeed = movementVector.x;    
        
        Vector3 newCamPos = new Vector3();
        
        float originRelativeMouseX = mousePos.x - (Screen.width / 2f);
        float originRelativeMouseY = mousePos.y - (Screen.height / 2f);
        
        newCamPos.x = (Mathf.Abs(originRelativeMouseX) < (Screen.width / 2f)) ? (originRelativeMouseX) : 
            ((originRelativeMouseX < 0f) ? (-1f * Screen.width / 2f) : (Screen.width / 2f));
        newCamPos.y = (Mathf.Abs(originRelativeMouseY) < (Screen.height / 2f)) ? (originRelativeMouseY) : 
            ((originRelativeMouseY < 0f) ? (-1f * Screen.height / 2f) : (Screen.height / 2f));

        if (ledgeGrabbed)
        {
            this.transform.eulerAngles = new Vector3(0f, ledgeHangDirection * 90f, 0f);
        }
        else if (originRelativeMouseX < 0)
        {
            this.transform.eulerAngles = new Vector3(0f, -90f, 0f);
            mouseXDirection = -1;
        }
        else
        {
            this.transform.eulerAngles = new Vector3(0f, 90f, 0f);
            mouseXDirection = 1;
        }
        
        newCamPos.x /= 20f;
        newCamPos.y /= 20f;
        newCamPos.y += 22f;
        newCamPos.z -= 80f;

        Vector3 playerFollowPos = transform.position;
        playerFollowPos.y += 22f;
        playerFollowPos.z = -80f;

        newCamPos += transform.position;
        
        playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, newCamPos, Time.deltaTime * cameraShiftDamp);
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        groundHit = hit;
    }

    private void OnDrawGizmos()
    {
        if (!controller.isGrounded)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawSphere(transform.position + new Vector3(3f * mouseXDirection, 17f, 0f), 0.5f);
            Gizmos.DrawSphere(transform.position + new Vector3(3f * mouseXDirection, 13f, 0f), 0.5f);

        }
    }

    void SetAnimationState(string nextState)
    {
        if (currentState == nextState)
        {
            return;
        }
        
        _animator.CrossFade(nextState, 0.025f);

        currentState = nextState;
    }
}
